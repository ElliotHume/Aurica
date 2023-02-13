using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuraUIPanel : MonoBehaviour {
    public static AuraUIPanel Instance;
    public DistributionUIDisplay auraDisplay;
    public DistributionUIDisplayValues auraValues, resistanceValues, strengthValues;
    public Text manaText;

    Aura aura;

    void Start() {
        AuraUIPanel.Instance = this;
    }

    public void SetAura(Aura a) {
        gameObject.SetActive(true);
        aura = a;
        auraDisplay.SetDistribution(aura.GetAura());
        auraValues.SetDistribution(aura.GetAura());
        resistanceValues.SetDistribution(aura.GetInnateStrength());
        if (strengthValues != null) strengthValues.SetDistribution(aura.GetInnateStrength());
        manaText.text = "Maximum Mana: " + PlayerManager.MAXIMUM_MANA;

        gameObject.SetActive(false);
    }
}
