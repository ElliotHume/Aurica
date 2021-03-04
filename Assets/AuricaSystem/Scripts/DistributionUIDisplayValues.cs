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
        structure.text = (mana.structure * multiplier).ToString("F2") + suffix;
        essence.text = (mana.essence * multiplier).ToString("F2") + suffix;
        fire.text = (mana.fire * multiplier).ToString("F2") + suffix;
        water.text = (mana.water * multiplier).ToString("F2") + suffix;
        earth.text = (mana.earth * multiplier).ToString("F2") + suffix;
        air.text = (mana.air * multiplier).ToString("F2") + suffix;
        nature.text = (mana.nature * multiplier).ToString("F2") + suffix;
    }
}
