using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public abstract class Structure : MonoBehaviourPun {

    [Tooltip("The Nexus' team")]
    [SerializeField]
    protected MOBATeam.Team Team;
    public MOBATeam.Team GetTeam {
        get { return Team; }
    }

    [Tooltip("Does the structure start with immunity")]
    [SerializeField]
    protected bool StartsImmune = false;
    protected bool Immune;

    [Tooltip("How much health the nexus starts with")]
    [SerializeField]
    protected float StartingHealth = 250f;

    [Tooltip("How much health the nexus regenerates per second")]
    [SerializeField]
    protected float HealthRegenPerSecond = 1f;

    [Tooltip("The delay before the nexus starts to regen health")]
    [SerializeField]
    protected float DelayBeforeHealhRegen = 10f;

    [Tooltip("Maximum damage based on this percentage of max health that a structure can take in one spell cast")]
    [SerializeField]
    protected float MaxDamageThreshold = 0.1f;

    [Tooltip("Disable the immunity of these structures when this one breaks")]
    [SerializeField]
    protected List<Structure> EnableNextStructures;

    [Tooltip("Events to fire off when the structure takes damage")]
    [SerializeField]
    protected UnityEvent OnTakeDamage;
    
    [Tooltip("Transform to anchor the structure ui display")]
    [SerializeField]
    protected Transform UIDisplayAnchor;

    [Tooltip("Transform to anchor the damage popups")]
    [SerializeField]
    protected Transform DamagePopupAnchor;

    [Tooltip("The prefab that has the canvas for displaying this structure's data")]
    [SerializeField]
    protected GameObject StructureUIPrefab;

    [Tooltip("Effects to play when the structure is disabled by the null sphere")]
    [SerializeField]
    protected List<ParticleSystem> DisabledParticles;
    
    [Tooltip("Objects to toggle when the structure is disabled by the null sphere")]
    [SerializeField]
    protected List<GameObject> ToggleObjectsOnDisable;


    protected float Health;
    protected bool broken = false, disabled = false, playingDisableParticles = false;
    protected GameObject UIDisplayGO;
    protected StructureUIDisplay UIDisplay;
    protected float healthRegenTimer;

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            // CRITICAL DATA
            stream.SendNext(Health);
        } else {
            // CRITICAL DATA
            this.Health = (float)stream.ReceiveNext();
        }
    }

    void FixedUpdate() {
        // Regen health if the structure is not broken or disabled
        if (photonView.IsMine && !broken && !disabled && HealthRegenPerSecond > 0f){
            healthRegenTimer += Time.deltaTime;
            if (healthRegenTimer >= DelayBeforeHealhRegen && Health < StartingHealth) {
                Health += HealthRegenPerSecond * Time.deltaTime;
            }
            if (Health > StartingHealth) {
                Health = StartingHealth;
            }
        }
    }
    
    // Start is called before the first frame update
    void Start() {
        Health = StartingHealth;
        Immune = StartsImmune;

        if (StructureUIPrefab != null) {
            UIDisplayGO = Instantiate(StructureUIPrefab, UIDisplayAnchor.position, UIDisplayAnchor.rotation, transform);
            UIDisplay = UIDisplayGO.GetComponent<StructureUIDisplay>();
            UIDisplay.SetStructure(this);
            UIDisplay.SetImmunity(Immune);
        }
    }

    [PunRPC]
    public virtual void OnSpellCollide(float Damage, string SpellEffectType, float Duration, string spellDistributionJson, string ownerID = "") {
        if (!photonView.IsMine || broken) return;

        // Check if the player is on the allied team
        MOBAPlayer attackingPlayer = MOBAPlayer.GetMOBAPlayerFromID(ownerID);
        bool isAllied = attackingPlayer.Side == Team;

        // Apply the damage if the structure is not immune and the player is not on the same team
        float finalDamage = Immune || isAllied ? 0f : Damage * GameManager.GLOBAL_SPELL_DAMAGE_MULTIPLIER;
        finalDamage = Mathf.Min(finalDamage, StartingHealth * MaxDamageThreshold);
        Health -= finalDamage;

        if (finalDamage > 0f) {
            healthRegenTimer = 0f;
            photonView.RPC("TookDamage", RpcTarget.All);
        }

        // Create damage popup if the player is on the opposing team
        if (!isAllied) {
            GameObject newPopup = PhotonNetwork.Instantiate("ZZZ Damage Popup Canvas", DamagePopupAnchor.position, DamagePopupAnchor.rotation, 0);
            DamagePopup dmgPopup = newPopup.GetComponent<DamagePopup>();
            if (dmgPopup != null) {
                if (Immune) {
                    dmgPopup.ShowText("Immune");
                } else {
                    dmgPopup.ShowDamage(finalDamage);
                }
                dmgPopup.isSceneObject = true;
            }
            Debug.Log("Structure ["+GetName()+"] was hit by ["+ownerID+"] for ["+finalDamage+"] damage. Remaining health: "+Health+ (Immune ? ". Structure is Immune!" : "."));
        }

        if (Health <= 0f) {
            Health = 0f;
            photonView.RPC("LocalBreak", RpcTarget.All);
            NetworkBreakStructure();
            NetworkExplode();
        }
    }

    [PunRPC]
    public void TookDamage() {
        OnTakeDamage.Invoke();
    }

    [PunRPC]
    public void LocalBreak() {
        LocalEffectExplode();
    }

    // ONLY RUN BY THE OWNER
    // This method handles the events for breaking a structure that are common to all structures
    protected void NetworkBreakStructure() {
        // Remove the immunity for the next structures
        foreach(Structure structure in EnableNextStructures) {
            structure.NetworkSetImmunity(false);
        }
    }

    public void NetworkRestoreStructure() {
        if (!photonView.IsMine) return;
        photonView.RPC("ClientRestore", RpcTarget.All);
    }

    [PunRPC]
    protected void ClientRestore() {
        Restore();
    }

    public void NetworkSetImmunity(bool immunity) {
        if (photonView.IsMine) photonView.RPC("SetImmunity", RpcTarget.All, immunity);
    }

    [PunRPC]
    public void SetImmunity(bool immunity) {
        Immune = immunity;
        UIDisplay.SetImmunity(Immune);
    }

    public void NetworkResetImmunity() {
        if (photonView.IsMine) photonView.RPC("ResetImmunity", RpcTarget.All);
    }

    [PunRPC]
    public void ResetImmunity() {
        Immune = StartsImmune;
        UIDisplay.SetImmunity(Immune);
    }

    public void NetworkMasterToggleDisabledParticles(bool on) {
        if (!photonView.IsMine) return;
        photonView.RPC("ClientToggleDisabledParticles", RpcTarget.All, on);
    }

    [PunRPC]
    public void ClientToggleDisabledParticles(bool on) {
        if (on) {
            foreach(ParticleSystem effect in DisabledParticles) effect.Play();
        } else {
            foreach(ParticleSystem effect in DisabledParticles) effect.Stop();
        }
    }

    public void SetColors() {
        Debug.Log("Structure"+gameObject+"SettingColors -- local player team: "+MOBAPlayer.LocalPlayer.Side.ToString());
        UIDisplay.SetColors(MOBAPlayer.LocalPlayer.Side == Team);
    }

    public static List<GameObject> GetPlayersInRadius(Transform transform, float radius, Vector3 offset) {
        Collider[] hits = Physics.OverlapSphere(transform.position+offset, radius, 1 << 3);
        List<GameObject> playerList = new List<GameObject>();
        foreach(var hit in hits) {
            PlayerManager pm = hit.gameObject.GetComponent<PlayerManager>();
            if (pm != null) playerList.Add(hit.gameObject);
        }
        return playerList;
    }

    public static bool IsNullSphereInRadius(Transform transform, float radius, Vector3 offset) {
        Collider[] hits = Physics.OverlapSphere(transform.position+offset, radius, 1 << 12);
        List<GameObject> objectList = new List<GameObject>();
        foreach(var hit in hits) {
            NullSphere pm = hit.gameObject.GetComponent<NullSphere>();
            if (pm != null) return true;
        }
        return false;
    }

    /* -------------- ABSTRACT METHODS --------------------- */
    protected abstract void NetworkExplode();

    protected abstract void LocalEffectExplode();

    public abstract void Restore();

    public abstract string GetName();

    /* -------------- GET METHODS --------------------- */
    public bool IsBroken() { return broken; }
    
    public bool IsImmune() { return Immune; }

    public float GetHealth() { return Health; }

    public float GetStartingHealth() { return StartingHealth; }
}
