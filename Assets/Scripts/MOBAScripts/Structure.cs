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

    [Tooltip("Does the structure start with immunity")]
    [SerializeField]
    protected bool Immune = false;

    [Tooltip("How much health the nexus starts with")]
    [SerializeField]
    protected float StartingHealth = 250f;

    [Tooltip("How much health the nexus regenerates per second")]
    [SerializeField]
    protected float HealthRegenPerSecond = 1f;

    [Tooltip("The delay before the nexus starts to regen health")]
    [SerializeField]
    protected float DelayBeforeHealhRegen = 10f;

    [Tooltip("Disable the immunity of these structures when this one breaks")]
    [SerializeField]
    protected List<Structure> EnableNextStructures;
    
    [Tooltip("Transform to anchor the structure ui display")]
    [SerializeField]
    protected Transform UIDisplayAnchor;

    [Tooltip("Transform to anchor the damage popups")]
    [SerializeField]
    protected Transform DamagePopupAnchor;

    [Tooltip("The prefab that has the canvas for displaying this structure's data")]
    [SerializeField]
    protected GameObject StructureUIPrefab;


    protected float Health;
    protected bool broken = false, networkBroken = false;
    protected GameObject UIDisplayGO;
    protected StructureUIDisplay UIDisplay;

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            // CRITICAL DATA
            stream.SendNext(Health);
            stream.SendNext(Immune);
            stream.SendNext(broken);
        } else {
            // CRITICAL DATA
            this.Health = (float)stream.ReceiveNext();
            this.Immune = (bool)stream.ReceiveNext();
            networkBroken = (bool)stream.ReceiveNext();
        }
    }
    
    // Start is called before the first frame update
    void Start() {
        Health = StartingHealth;

        if (StructureUIPrefab != null) {
            UIDisplayGO = Instantiate(StructureUIPrefab, UIDisplayAnchor.position, UIDisplayAnchor.rotation, transform);
            UIDisplay = UIDisplayGO.GetComponent<StructureUIDisplay>();
            UIDisplay.SetStructure(this);
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        // If the structure has been broken, nothing more needs to be done to it.
        if (broken) return;

        if (photonView.IsMine) {
            // Owner of the structure
            if (Health <= 0f) {
                Health = 0f;
                LocalEffectExplode();
                NetworkExplode();
                return;
            }
        } else {
            // Remote clients
            if (networkBroken && !broken) {
                LocalEffectExplode();
            }
        }
    }

    protected abstract void NetworkExplode();

    protected abstract void LocalEffectExplode();


    [PunRPC]
    public virtual void OnSpellCollide(float Damage, string SpellEffectType, float Duration, string spellDistributionJson, string ownerID = "") {
        if (!photonView.IsMine || broken) return;
        ManaDistribution spellDistribution = JsonUtility.FromJson<ManaDistribution>(spellDistributionJson);

        // Apply the damage
        float finalDamage = Immune ? 0f : Damage * GameManager.GLOBAL_SPELL_DAMAGE_MULTIPLIER;
        Health -= finalDamage;

        // Create damage popup
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

    public abstract void Restore();

    public abstract string GetName();

    public bool IsBroken() {
        return broken;
    }

    public bool IsImmune() {
        return Immune;
    }

    public void SetImmunity(bool immunity) {
        Immune = immunity;
    }

    public float GetHealth() {
        return Health;
    }

    public float GetStartingHealth() {
        return StartingHealth;
    }
}
