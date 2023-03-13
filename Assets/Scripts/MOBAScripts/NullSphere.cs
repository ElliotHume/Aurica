using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class NullSphere : MonoBehaviourPun {

    [Tooltip("Origin point, where the ball resets to")]
    [SerializeField]
    private Transform Origin;

    [Tooltip("How much health the ball starts with")]
    [SerializeField]
    private float StartingHealth;
    private float Health;

    [Tooltip("At what health threshold does the ball drop onto the ground")]
    [SerializeField]
    private float DropHealthThreshold;
    
    [Tooltip("Holding position offset, where the sphere is placed when being picked up by a player")]
    [SerializeField]
    private Vector3 HoldingOffset = Vector3.up;

    [Tooltip("Status effect to apply to the player who has picked up the ball")]
    [SerializeField]
    private StatusEffect AppliedStatusEffect;

    [Tooltip("Local client event to invoke when the ball is grabbed by a player")]
    [SerializeField]
    private UnityEvent OnGrab;

    [Tooltip("Local client event to invoke when the ball is dropped by a player")]
    [SerializeField]
    private UnityEvent OnDrop;

    [Tooltip("Local client event to invoke when the ball is reset to it's origin")]
    [SerializeField]
    private UnityEvent OnReset;

    [Tooltip("Local client event to invoke when the ball is damaged")]
    [SerializeField]
    private UnityEvent OnDamage;

    // Holding state
    // 0 - ball is at origin, not picked up or dropped
    // 1 - ball has been picked up, is actively being held
    // 2 - ball has been dropped, is at rest but not at the origin point
    private int State = 0;

    private GameObject holdingPlayerGO;
    private MOBAPlayer holdingMOBAPlayer;
    private MOBATeam.Team holdingPlayersTeam = MOBATeam.Team.None;

    private PlayerManager localPlayerManager;
    private InputManager inputManager;

    // AoE damage popup variables
    private float aoeDamageTotal=0f, aoeDamageTick=0f, accumulatingDamageTimout=1f, accumulatingDamageTimer=0f;
    private DamagePopup accumulatingDamagePopup;

    // Start is called before the first frame update
    void Start() {
        Health = StartingHealth;

        localPlayerManager = PlayerManager.LocalInstance;
        inputManager = InputManager.Instance;
    }

    // Update is called once per frame
    void Update() {
        // Position the ball over the holding player
        if (holdingPlayerGO != null) {
            transform.position = holdingPlayerGO.transform.position + HoldingOffset;
            transform.rotation = holdingPlayerGO.transform.rotation;
        }

        // Clients can press a keybind to drop the ball
        if (localPlayerManager == null) localPlayerManager = PlayerManager.LocalInstance;
        if (localPlayerManager != null && holdingPlayerGO == localPlayerManager.gameObject && (inputManager.GetKeyDown(KeybindingActions.DropObjective) || localPlayerManager.dead)) {
            NetworkClientDropRequest();
        }
    }

    void FixedUpdate() {
        if (photonView.IsMine) {
            // Compute AoE tick damage and total sum, if no new damage ticks come in for a while 
            if (aoeDamageTotal == 0f && aoeDamageTick > 0f) {
                // Add damage tick to the total and reset the tick
                aoeDamageTotal += aoeDamageTick;
                aoeDamageTick = 0f;

                // Initiate an accumulating damage popup
                GameObject newPopup = PhotonNetwork.Instantiate("ZZZ Damage Popup Canvas", transform.position+ (Vector3.up * 0.5f), transform.rotation, 0);
                newPopup.transform.SetParent(gameObject.transform);
                DamagePopup dmgPopup = newPopup.GetComponent<DamagePopup>();
                if (dmgPopup != null) {
                    dmgPopup.AccumulatingDamagePopup(aoeDamageTotal);
                    accumulatingDamagePopup = dmgPopup;
                }
            } else if (aoeDamageTotal > 0f && aoeDamageTick > 0f) {
                // Add damage tick to the total and reset the tick
                aoeDamageTotal += aoeDamageTick;
                aoeDamageTick = 0f;

                // Update the accumulating damage popup
                accumulatingDamagePopup.AccumulatingDamagePopup(aoeDamageTotal);

                // Reset the tick timout timer
                accumulatingDamageTimer = 0f;
            } else if (aoeDamageTotal > 0f && aoeDamageTick == 0f && accumulatingDamageTimer < accumulatingDamageTimout) {
                // If there is a running total but no new damage tick, start the timer to end the accumulating process
                accumulatingDamageTimer += Time.deltaTime;
            } else if (aoeDamageTotal > 0f && aoeDamageTick == 0f && accumulatingDamageTimer >= accumulatingDamageTimout) {
                // Timout has been reached for new damage ticks, end the accumulation process and reset all variables
                accumulatingDamagePopup.EndAccumulatingDamagePopup();
                aoeDamageTotal = 0f;
                aoeDamageTick = 0f;
                accumulatingDamageTimer = 0f;
            }
        }
    }

    // CLIENT
    void OnTriggerEnter(Collider collider) {
        // If the ball has been picked up, ignore trigger collision
        if (State == 1) return;

        // If the collision is not with a player, we ignore it
        MOBAPlayer player;
        if (collider.gameObject.tag == "Player") {
            player = collider.gameObject.GetComponent<MOBAPlayer>();
            if (player == null) return;
        } else {
            return;
        }

        // If the colliding player is not ours, ignore it
        PhotonView pv = PhotonView.Get(player);
        if (!pv.IsMine) return;

        // If the player is ours, send a request to the master to pickup the sphere
        photonView.RPC("MasterRequestPickup", RpcTarget.All, player.GetUniqueName());
    }

    [PunRPC]
    void MasterRequestPickup(string playerId) {
        // If you are not the master client or if the ball has already been picked up, ignore pickup requests
        if (!photonView.IsMine || State == 1) return;

        // Get the MOBAPlayer trying to pickup the ball
        MOBAPlayer player = MOBAPlayer.GetMOBAPlayerFromID(playerId);

        // If the ball is at origin, send the pickup event to the clients
        if (State == 0) {
            // If the player is ours, send a request to the master to pickup the sphere
            SetHoldingPlayer(player);
            photonView.RPC("ClientPickup", RpcTarget.All, playerId);
        } else if (State == 2) {
            // Only allow picking up a dropped ball if the player is on the opposing side of the last player to pickup the ball
            if (player.Side != holdingPlayersTeam) {
                photonView.RPC("ClientPickup", RpcTarget.All, playerId);
                // Reset the health if the opposing team picks it up
                Health = StartingHealth;
            }
        }
    }

    [PunRPC]
    void OnSpellCollide(float Damage, string SpellEffectType, float Duration, string spellDistributionJson, string ownerID = "") {
        // Ignore this event if you are not the master client or the ball is at origin
        if (!photonView.IsMine || State == 0) return;

        // Check if the attacking player is on the allied team
        MOBAPlayer attackingPlayer = MOBAPlayer.GetMOBAPlayerFromID(ownerID);
        bool isAllied = holdingPlayersTeam != MOBATeam.Team.None && attackingPlayer.Side == holdingPlayersTeam;

        // Calculate the final damage
        // Damage is set to 0 if the ball is picked up and hit by an allied player
        float finalDamage = !isAllied || State == 2 ? Damage * GameManager.GLOBAL_SPELL_DAMAGE_MULTIPLIER : 0f;

        // Create damage popup
        if (finalDamage > 1.5f) {
            GameObject newPopup = PhotonNetwork.Instantiate("ZZZ Damage Popup Canvas", transform.position + (Vector3.up*2.75f), transform.rotation, 0);

            DamagePopup dmgPopup = newPopup.GetComponent<DamagePopup>();
            if (dmgPopup != null) {
                dmgPopup.ShowDamage(finalDamage);
                dmgPopup.isSceneObject = true;
            }
        } else if (finalDamage > 0f) {
            aoeDamageTick += finalDamage;
        }
        
        // Apply the damage
        Health -= finalDamage;

        // Debug.LogError("Null sphere took ["+finalDamage+"] damage from "+ownerID+"    remaining health: "+Health);

        if (State == 1 && Health <= DropHealthThreshold ) {
            // If the ball has been picked up and health is now below the drop threshold, drop the ball
            NetworkMasterDrop();
        } else if (Health <= 0f && State == 2) {
            // If the ball is on the ground and drops to or below 0 health, reset it
            NetworkMasterReset();
        }
        
        // If there is some health left and damage was applied, let the clients know to run local damage effects
        if (Health > 0f && finalDamage > 1.5f) {
            NetworkMasterOnDamage();
        }
    }

    // CLIENT call this to inform the master that they would like to drop the ball
    void NetworkClientDropRequest() {
        // If the ball has already been dropped, ignore this
        if (State != 1) return;
        photonView.RPC("MasterDropRequest", RpcTarget.All, holdingMOBAPlayer.GetUniqueName());
    }

    [PunRPC]
    void MasterDropRequest(string playerId) {
        if (!photonView.IsMine || State != 1) return;
        MOBAPlayer player = MOBAPlayer.GetMOBAPlayerFromID(playerId);
        if (player.gameObject == holdingPlayerGO) {
            NetworkMasterDrop();
        }
    }

    void NetworkMasterDrop() {
        // If you are not the master client or if the ball has already been dropped, ignore this
        if (!photonView.IsMine || State != 1) return;
        photonView.RPC("ClientDrop", RpcTarget.All, holdingMOBAPlayer.GetUniqueName());
    }

    public void NetworkMasterReset() {
        // If you are not the master client or if the ball has already been reset, ignore this
        if (!photonView.IsMine || State == 0) return;
        // Reset health
        Health = StartingHealth;
        photonView.RPC("ClientReset", RpcTarget.All);
    }

    public void NetworkMasterOnDamage() {
        // If you are not the master client or if the ball has already been reset, ignore this
        if (!photonView.IsMine || State == 0) return;
        photonView.RPC("ClientOnDamage", RpcTarget.All);
    }

    [PunRPC]
    void ClientOnDamage() {
        OnDamage.Invoke();
    }

    [PunRPC]
    void ClientPickup(string playerId) {
        Debug.Log("Client ["+playerId+"] picked up null sphere");

        // Set the state to picked up
        State = 1;

        // Run the local effects for picking up the sphere
        OnGrab.Invoke();

        // Get the MOBAPlayer that has picked up the ball
        MOBAPlayer player = MOBAPlayer.GetMOBAPlayerFromID(playerId);

        // Apply the status effect to the player that picked it up
        if (AppliedStatusEffect != null) AppliedStatusEffect.ManualContinuousActivation(player.gameObject);

        // Assign them as the ball holder
        SetHoldingPlayer(player);
    }

    [PunRPC]
    private void ClientDrop(string playerId) {
        Debug.Log("Client ["+playerId+"] dropped null sphere");

        // Set the state to dropped
        State = 2;
        
        // Run the local effects for dropping the sphere
        OnDrop.Invoke();

        // Drop the ball onto the ground
        transform.position -= HoldingOffset;

        // Get the MOBAPlayer that has dropped the ball
        MOBAPlayer player = MOBAPlayer.GetMOBAPlayerFromID(playerId);

        // Remove the status effect from the player that picked it up
        if (AppliedStatusEffect != null) AppliedStatusEffect.ManualContinuousDeactivation(player.gameObject);

        // Unassign them as the ball holder
        holdingPlayerGO = null;
        holdingMOBAPlayer = null;
    }

    [PunRPC]
    public void ClientReset() {
        Debug.Log("Null sphere reset");

        // Set the state to at origin
        State = 0;
        
        // Run the local effects for dropping the sphere
        OnReset.Invoke();

        // Place the ball back at origin
        transform.position = Origin.position;
        transform.rotation = Origin.rotation;

        // If a player is holding the ball when we reset it, remove the applied status effect
        if (holdingPlayerGO != null && AppliedStatusEffect != null) AppliedStatusEffect.ManualContinuousDeactivation(holdingPlayerGO);
        
        // Unassign them as the ball holder, and clear the team on the ball
        holdingPlayerGO = null;
        holdingMOBAPlayer = null;
        holdingPlayersTeam = MOBATeam.Team.None;
    }

    void SetHoldingPlayer(MOBAPlayer player) {
        holdingPlayerGO = player.gameObject;
        holdingMOBAPlayer = player;
        holdingPlayersTeam = player.Side;
    }
}
