using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistributionUIDisplay : MonoBehaviour
{
    public HealthBar order, chaos, life, death, fire, water, earth, air, divine, demonic;
    public bool fixedValues = false;
    private List<HealthBar> bars;
    // Start is called before the first frame update
    void Start()
    {
        bars = new List<HealthBar>();
        bars.Add(order);
        bars.Add(chaos);
        bars.Add(life);
        bars.Add(death);
        bars.Add(fire);
        bars.Add(water);
        bars.Add(earth);
        bars.Add(air);
        bars.Add(divine);
        bars.Add(demonic);

        if (!fixedValues) ResetAll();
    }

    public void ResetAll() {
        foreach (HealthBar bar in bars) {
            bar.SetHealth(0f);
        }
    }

    public void SetDistribution(ManaDistribution mana) {
        order.SetHealth(mana.structure > 0f ? mana.structure : 0f);
        chaos.SetHealth(mana.structure < 0f ? -mana.structure : 0f);
        life.SetHealth(mana.essence > 0f ? mana.essence : 0f);
        death.SetHealth(mana.essence < 0f ? -mana.essence : 0f);
        fire.SetHealth(mana.fire);
        water.SetHealth(mana.water);
        earth.SetHealth(mana.earth);
        air.SetHealth(mana.air);
        divine.SetHealth(mana.nature > 0f ? mana.nature : 0f);
        demonic.SetHealth(mana.nature < 0f ? -mana.nature : 0f);
    }
}
