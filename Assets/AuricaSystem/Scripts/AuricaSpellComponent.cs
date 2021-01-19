using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AuricaSpellComponent", menuName = "Aurica/AuricaSpellComponent", order = 0)]
public class AuricaSpellComponent : ScriptableObject {
    public string c_name;
    [TextArea(15, 3)]
    public string description;
    public bool hasBasicDistribution, hasAuricDistribution, hasFluxDistribution;
    public ManaDistribution basicDistribution, auricDistribution, fluxDistribution;
    //public List<float> basicDistribution = new List<float>(7), auricDistribution = new List<float>(7), fluxDistribution = new List<float>(7);

    public ManaDistribution CalculateDistributionChange(ManaDistribution givenDistribution, ManaDistribution aura) {
        ManaDistribution calculatedDistribution = new ManaDistribution(givenDistribution.ToString());
        if (hasBasicDistribution) calculatedDistribution.AddBasicDist(basicDistribution);
        // TODO: For now, there is no aura system or flux calculations
        // if (hasAuricDistribution) calculatedDistribution.AddAuricDist(auricDistribution, aura);
        // if (hasFluxDistribution) calculatedDistribution.ApplyFluxDist(fluxDistribution);
        return calculatedDistribution;
    }
}