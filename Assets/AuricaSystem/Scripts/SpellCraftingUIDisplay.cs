using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellCraftingUIDisplay : MonoBehaviour
{
    public ManaDistribution aura;
    public ComponentUIDisplay componentUIDisplay;

    public void SendAura(ManaDistribution a) {
        aura = a;
        componentUIDisplay.SendAura(a);
    }

    public ManaDistribution GetAura() {
        return aura;
    }
}