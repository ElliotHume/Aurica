using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellCraftingUIDisplay : MonoBehaviour
{
    public ManaDistribution aura;
    public ComponentUIDisplay componentUIDisplay;
    public SpellUIDisplay spellUIDisplay;
    public BindingUIPanel bindingPanel;

    public void SendAura(ManaDistribution a) {
        aura = a;
        componentUIDisplay.SendAura(a);
        bindingPanel.Startup();
    }

    public ManaDistribution GetAura() {
        return aura;
    }

    public void ClearSpell() {
        componentUIDisplay.Hide();
        spellUIDisplay.ClearSpell();
    }
}
