using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DistributionUIDisplayValues : MonoBehaviour {
    public Text structure, essence, fire, water, earth, air, nature;
    public string suffix = "";
    public float multiplier = 100f;
    private List<Text> bars;
    // Start is called before the first frame update
    void Start() {
        bars = new List<Text>();
        bars.Add(structure);
        bars.Add(essence);
        bars.Add(fire);
        bars.Add(water);
        bars.Add(earth);
        bars.Add(air);
        bars.Add(nature);
        ResetAll();
    }

    public void ResetAll() {
        foreach (Text bar in bars) {
            bar.text = "0" + suffix;
        }
    }

    public void SetDistribution(ManaDistribution mana) {
        structure.text = GetShorterString(mana.structure);
        essence.text = GetShorterString(mana.essence);
        fire.text = GetShorterString(mana.fire);
        water.text = GetShorterString(mana.water);
        earth.text = GetShorterString(mana.earth);
        air.text = GetShorterString(mana.air);
        nature.text = GetShorterString(mana.nature);
    }

    string GetShorterString(float mana) {
        int comparisonLength = mana > 0f ? 5 : 6; 
        bool useExtraPrecision = (mana * multiplier).ToString().Length >= comparisonLength;
        return (mana * multiplier).ToString(useExtraPrecision ? "F3" : "F2") + suffix;
    }
}
