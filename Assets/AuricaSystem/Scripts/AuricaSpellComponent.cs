using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AuricaSpellComponent", menuName = "Aurica/AuricaSpellComponent", order = 0)]
public class AuricaSpellComponent : ScriptableObject {
    public enum Category {
        Elemental, Basic, Minor, Siphon, Advanced, Balancer, Uncommon
    };

    public string c_name;
    [TextArea(15, 3)]

    public Category category;
    public string description;
    public float manaCostMultiplier = 1f;
    public bool hasBasicDistribution, hasAuricDistribution, hasFluxDistribution, hasSiphonDistribution;
    public ManaDistribution basicDistribution, auricDistribution, fluxDistribution, siphonDistribution;
    //public List<float> basicDistribution = new List<float>(7), auricDistribution = new List<float>(7), fluxDistribution = new List<float>(7);

    public ManaDistribution CalculateDistributionChange(ManaDistribution givenDistribution, ManaDistribution aura) {
        ManaDistribution calculatedDistribution = new ManaDistribution(givenDistribution.ToString());
        if (hasBasicDistribution) calculatedDistribution.AddBasicDist(basicDistribution);
        if (hasAuricDistribution) calculatedDistribution.AddAuricDist(auricDistribution, aura);
        if (hasFluxDistribution) calculatedDistribution.ApplyFluxDist(fluxDistribution);
        if (hasSiphonDistribution) calculatedDistribution.ApplySiphon(siphonDistribution);
        calculatedDistribution.ClampElementalValues();
        return calculatedDistribution;
    }

    public float GetManaCost(ManaDistribution aura) {
        if (!hasAuricDistribution && !hasBasicDistribution) return 0;
        float auricContribution = hasAuricDistribution ? (auricDistribution * aura).GetAggregate() : 0f;
        float basicContribution = hasBasicDistribution ? basicDistribution.GetAggregate() * 0.25f : 0f;
        return (auricContribution + basicContribution) * 100f * manaCostMultiplier;
    }

    public int CompareTo(AuricaSpellComponent other) {
        if (this.category.CompareTo(other.category) == 0) {
            return this.c_name.CompareTo(other.c_name);
        }

        return this.category.CompareTo(other.category);
    }
}