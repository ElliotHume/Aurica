using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AuricaSpellComponent", menuName = "Aurica/AuricaSpellComponent", order = 0)]
public class AuricaSpellComponent : ScriptableObject {
    public string c_name;
    [TextArea(15, 3)]
    public string description;
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
        if (!hasAuricDistribution) return 0;
        return (auricDistribution * aura).GetAggregate() * 100f;
    }
}