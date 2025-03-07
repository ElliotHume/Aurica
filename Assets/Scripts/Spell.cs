using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Spell : MonoBehaviourPun {
    // How much damage the spell does
    public float Damage = 10f;

    // Aurica Spell ScriptableObject
    public AuricaSpell auricaSpell;

    // How much Mana is refund for this spell, used only for balancing reasons
    public float ManaRefund = 0f;

    // How much Mana it costs to channel this spell (Mana/second)
    public float ManaChannelCost = 40f;

    // What type of spell cast it creates (ex. Projectile, AoE, Fog, Trap, Ground)
    public string SpellEffectType;

    // Duration of the spell, used for DoT, AoE, etc..
    public float Duration = 1f;

    // What animation to play for the spell (if any)
    public int CastAnimationType = 1;

    // Which casting anchor to spawn the spell from
    public string CastingAnchor = "front";

    public bool TurnToAimPoint = true, UseAimPointNormal = true;

    public bool IsChannel = false, IsSelfTargeted = false, IsOpponentTargeted = false;

    protected float spellStrength = 1f, expertise = -1f;
    protected GameObject owner;
    protected PlayerManager ownerPM;
    protected ManaDistribution damageModifier;
    protected bool canHitOwner = true;

    public virtual void SetSpellStrength(float newStrength) {
        // Debug.Log("New Spell Strength: "+newStrength);
        spellStrength = newStrength;
    }

    public virtual void SetSpellDamageModifier(ManaDistribution newMod) {
        damageModifier = newMod;
        // Debug.Log("New modifier set: "+auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()));
    }

    public float GetSpellStrength() {
        return spellStrength;
    }

    public ManaDistribution GetSpellDamageModifier() {
        return damageModifier;
    }

    public virtual void SetOwner(GameObject ownerGO, bool _canHitOwner = true) {
        owner = ownerGO;
        ownerPM = ownerGO.GetComponent<PlayerManager>();
        canHitOwner = _canHitOwner;
    }

    public bool GetCanHitOwner() {
        return canHitOwner;
    }

    public GameObject GetOwner() {
        return owner;
    }

    public PlayerManager GetOwnerPM() {
        return ownerPM;
    }

    public virtual void SetExpertiseParameters(int exp) {
        expertise = exp;
    }

    protected void FlashHitMarker(bool majorDamage) {
        if (owner == null) return;
        PlayerManager pm = owner.GetComponent<PlayerManager>();
        if (pm != null) {
            pm.FlashHitMarker(majorDamage);
        }
    }

    [PunRPC]
    public void DestroySpell() {
        if (photonView.IsMine) PhotonNetwork.Destroy(gameObject);
    }
}
