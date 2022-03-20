using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrimoireSpellUIButton : MonoBehaviour {
    public Text title;
    public AuricaSpell spell;
    public AuricaPureSpell pureSpell;
    public GrimoireSpellUIDisplay spellDisplay;
    public GrimoirePureSpellUIDisplay pureSpellDisplay;
    public Image spellIcon;

    public void SetTitle(string newText){
        title.text = newText;
    }

    public void SetSpell(AuricaSpell s) {
        spell = s;
        SetTitle(s.c_name);
        if (spellIcon != null) spellIcon.sprite = ResourceManager.Instance.GetIcon(spell.manaType);
    }

    public void SetPureSpell(AuricaPureSpell sp) {
        pureSpell = sp;
        SetTitle(sp.c_name);
        if (spellIcon != null) spellIcon.sprite = ResourceManager.Instance.GetPureIcon();
    }

    public void SetSpellDisplay(GrimoireSpellUIDisplay sd) {
        spellDisplay = sd;
    }

    public void SetPureSpellDisplay(GrimoirePureSpellUIDisplay sd){
        pureSpellDisplay = sd;
    }

    public void DisplaySpell(){
        if (spellDisplay != null && spell != null) {
            spellDisplay.PopulateFromSpell(spell);
            spellDisplay.ShowSpell();
        }
        if (pureSpellDisplay != null && pureSpell != null) {
            pureSpellDisplay.PopulateFromSpell(pureSpell);
            pureSpellDisplay.ShowSpell();
        }
    }
}
