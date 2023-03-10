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

    // AoE damage popup variables
    private float aoeDamageTotal=0f, aoeDamageTick=0f, accumulatingDamageTimout=1f, accumulatingDamageTimer=0f;
    private DamagePopup accumulatingDamagePopup;

    // Start is called before the first frame update
    void Start() {
        Health = StartingHealth;
    }

    // Update is called once per frame
    void Update() {
        if (State == 1 && holdingPlayerGO != null) {
            transform.position = holdingPlayerGO.transform.position + HoldingOffset;
            transform.rotation = holdingPlayerGO.transform.rotation;
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

    void OnTriggerEnter(Collider collider) {
        MOBAPlayer player;
        if (collider.gameObject.tag == "Player") {
            player = collider.gameObject.GetComponent<MOBAPlayer>();
            if (player == null) return;
        } else {
            return;
        }

        if (State == 0 && collider.gameObject.tag == "Player") {
            // Attach to the player, we dont need to check what side they are on
            Pickup(collider.gameObject, player);
        } else if (State == 2 && collider.gameObject.tag == "Player") {
            // If the ball is dropped on the ground only the enemy team can pick it up
            // picking it up also resets it's health
            if (player.Side != holdingPlayersTeam) {
                Pickup(collider.gameObject, player);
                Health = StartingHealth;
            }
        }
    }

    [PunRPC]
    void OnSpellCollide(float Damage, string SpellEffectType, float Duration, string spellDistributionJson, string ownerID = "") {
        if (State == 0) return;

        // Check if the player is on the allied team
        MOBAPlayer attackingPlayer = MOBAPlayer.GetMOBAPlayerFromID(ownerID);
        bool isAllied = holdingPlayersTeam != MOBATeam.Team.None && attackingPlayer.Side == holdingPlayersTeam;

        // Apply the damage
        float finalDamage = !isAllied ? Damage * GameManager.GLOBAL_SPELL_DAMAGE_MULTIPLIER : 0f;

        if (photonView.IsMine) {
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
        }
        
        Health -= finalDamage;

        Debug.LogError("Null sphere took ["+finalDamage+"] damage from "+ownerID+"    remaining health: "+Health);

        if (Health <= DropHealthThreshold && State == 1) {
            Drop();
        } else if (Health <= 0f && State == 2) {
            Reset();
        }
        
        if (Health > 0f && finalDamage > 1.5f) {
            OnDamage.Invoke();
        }
    }

    private void Pickup(GameObject playerGO, MOBAPlayer player) {
        if (State == 1) return;
        State = 1;
        OnGrab.Invoke();

        holdingPlayerGO = playerGO;
        holdingMOBAPlayer = player;
        holdingPlayersTeam = player.Side;

        if (AppliedStatusEffect != null) AppliedStatusEffect.ManualContinuousActivation(playerGO);
    }


    private void Drop() {
        if (State != 1) return;
        State = 2;
        OnDrop.Invoke();

        transform.position -= HoldingOffset;

        if (AppliedStatusEffect != null) AppliedStatusEffect.ManualContinuousDeactivation(holdingPlayerGO);
        
        holdingPlayerGO = null;
        holdingMOBAPlayer = null;
    }

    public void Reset(bool force = false) {
        if (State != 2 && !force) return;
        State = 0;
        OnReset.Invoke();

        Health = StartingHealth;

        transform.position = Origin.position;
        transform.rotation = Origin.rotation;

        holdingPlayerGO = null;
        holdingMOBAPlayer = null;
        holdingPlayersTeam = MOBATeam.Team.None;
    }
}
