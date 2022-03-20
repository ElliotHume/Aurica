using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrimoireSpellUIButton : MonoBehaviour {
    public Text title;
    public AuricaSpell spell;
    public GrimoireSpellUIDisplay spellDisplay;
    public Image spellIcon;

    public void SetTitle(string newText){
        title.text = newText;
    }

    public void SetSpell(AuricaSpell s) {
        spell = s;
        SetTitle(s.c_name);
        if (spellIcon != null) spellIcon.sprite = ResourceManager.Instance.GetIcon(spell.manaType);
    }

    public void SetSpellDisplay(GrimoireSpellUIDisplay sd) {
        spellDisplay = sd;
    }

    public void DisplaySpell(){
        if (spellDisplay != null) {
            spellDisplay.PopulateFromSpell(spell);
            spellDisplay.ShowSpell();
        }
    }
}
