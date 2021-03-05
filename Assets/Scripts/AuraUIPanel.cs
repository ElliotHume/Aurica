using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuraUIPanel : MonoBehaviour {
    public DistributionUIDisplay auraDisplay;
    public DistributionUIDisplayValues auraValues, resistanceValues, strengthValues;
    public Text manaText;

    Aura aura;

    public void SetAura(Aura a) {
        aura = a;
        auraDisplay.SetDistribution(aura.GetAura());
        auraValues.SetDistribution(aura.GetAura());
        resistanceValues.SetDistribution(aura.GetInnateStrength());
        strengthValues.SetDistribution(aura.GetInnateStrength());
        manaText.text = "Maximum Mana: " + (aura.GetAggregatePower() * 100f);

        gameObject.SetActive(false);
    }
}
