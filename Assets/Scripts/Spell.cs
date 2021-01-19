using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Spell : MonoBehaviourPun
{
    // How much damage the spell does
    public float Damage = 10f;

    // How much Mana it costs to cast this spell
    public float ManaCost = 30f;

    // Aurica Spell ScriptableObject
    public AuricaSpell auricaSpell;

    // How much Mana it costs to channel this spell (Mana/second)
    public float ManaChannelCost = 40f;

    // What mana type of damage it does (ex. Fire, Water, Air, Earth, Order, Chaos, Life, Death, Divine, Demonic)
    public string ManaDamageType;

    // What type of spell cast it creates (ex. Projectile, AoE, Fog, Trap, Ground)
    public string SpellEffectType;

    // Duration of the spell, used for DoT, AoE, etc..
    public float Duration = 1f;
}
