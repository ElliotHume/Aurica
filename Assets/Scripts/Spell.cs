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

    public bool TurnToAimPoint = true;

    public bool IsChannel = false, IsSelfTargeted = false, IsOpponentTargeted = false;

    private float spellStrength = 1f;
    private GameObject owner;
    private ManaDistribution damageModifier;

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

    public virtual void SetOwner(GameObject ownerGO) {
        owner = ownerGO;
    }

    public GameObject GetOwner() {
        return owner;
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
