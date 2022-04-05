using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "AuricaSpellComponent", menuName = "Aurica/AuricaSpellComponent", order = 0)]
public class AuricaSpellComponent : ScriptableObject {
    public enum Category {
        SpellBasis, SpellForm, SpellFocus, SpellAction, ManaType, ManaSiphon, WildGlyph
    };

    public enum Classification {
        // Spell Basis
        Attack, Defend, Support, Summon,

        // Spell Form
        Sphere, Wall, Aura, Area, Infusion, Blade, Shield,

        // Spell Focus
        Self, Other, Surface, Mana, Form,

        // Spell Action
        Propel, Throw, Target, Sustain, Control, Collect, Contain, Expel, Pull, Bless, Curse,

        // Mana Type
        ManaType,

        // Mana Siphon
        Siphon,

        // Wild Glyphs
        Wild

    }

    public string c_name;
    [TextArea(15, 3)]

    public Category category;
    public Classification classification;
    public string description;
    public float manaCostMultiplier = 1f;
    public bool hasBasicDistribution, hasAuricDistribution, hasFluxDistribution, hasSiphonDistribution;
    public ManaDistribution basicDistribution, auricDistribution, fluxDistribution, siphonDistribution;

    public ManaDistribution CalculateDistributionChange(ManaDistribution givenDistribution, ManaDistribution aura) {
        ManaDistribution calculatedDistribution = new ManaDistribution(givenDistribution.ToString());
        if (hasBasicDistribution) calculatedDistribution.AddBasicDist(basicDistribution);
        if (hasAuricDistribution) calculatedDistribution.AddAuricDist(auricDistribution, aura);
        if (hasFluxDistribution) calculatedDistribution.ApplyFluxDist(fluxDistribution);
        if (hasSiphonDistribution) calculatedDistribution.ApplySiphon(siphonDistribution);
        calculatedDistribution.ClampElementalValues();
        return calculatedDistribution;
    }

    public ManaDistribution RemoveDistribution(ManaDistribution givenDistribution, ManaDistribution aura) {
        ManaDistribution calculatedDistribution = new ManaDistribution(givenDistribution.ToString());
        if (hasBasicDistribution) calculatedDistribution.SubtractBasicDist(basicDistribution);
        if (hasAuricDistribution) calculatedDistribution.SubtractAuricDist(auricDistribution, aura);
        calculatedDistribution.ClampElementalValues();
        return calculatedDistribution;
    }

    public float GetManaCost(ManaDistribution aura) {
        if (!hasAuricDistribution && !hasBasicDistribution && !hasSiphonDistribution) return 0;
        float auricContribution = hasAuricDistribution ? (auricDistribution * aura).GetAggregate() : 0f;
        float basicContribution = hasBasicDistribution ? basicDistribution.GetAggregate() * 0.25f : 0f;
        float siphonContribution = hasSiphonDistribution ? 0.05f : 0f;
        return (auricContribution + basicContribution + siphonContribution) * 100f * manaCostMultiplier;
    }

    public int CompareTo(AuricaSpellComponent other) {
        if (this.category.CompareTo(other.category) == 0) {
            return this.c_name.CompareTo(other.c_name);
        }

        return this.category.CompareTo(other.category);
    }
}