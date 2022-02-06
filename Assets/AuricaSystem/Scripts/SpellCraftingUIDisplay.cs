using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellCraftingUIDisplay : MonoBehaviour
{
    public ManaDistribution aura;
    public ComponentUIDisplay componentUIDisplay;
    public SpellUIDisplay spellUIDisplay;

    public void SendAura(ManaDistribution a) {
        aura = a;
        BindingUIPanel.LocalInstance.Startup();
        gameObject.SetActive(false);
    }

    public ManaDistribution GetAura() {
        return aura;
    }

    public void ClearSpell() {
        componentUIDisplay.Hide();
        spellUIDisplay.ClearSpell();
    }
}
