using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Spell : MonoBehaviourPun {
    // How much damage the spell does
    public float Damage = 10f;

    // Aurica Spell ScriptableObject
    public AuricaSpell auricaSpell;

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

    public virtual void SetSpellStrength(float newStrength) {
        spellStrength = newStrength;
    }

    public float GetSpellStrength() {
        return spellStrength;
    }

    public virtual void SetOwner(GameObject ownerGO) {
        owner = ownerGO;
    }

    public GameObject GetOwner() {
        return owner;
    }
}
