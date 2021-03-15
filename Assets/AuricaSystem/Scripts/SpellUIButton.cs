using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellUIButton : MonoBehaviour
{
    public Text title;
    public AuricaSpell spell;
    public SpellUIDisplay spellDisplay;

    public void SetTitle(string newText){
        title.text = newText;
    }

    public void SetSpell(AuricaSpell s) {
        spell = s;
        SetTitle(s.c_name);
    }

    public void SetSpellDisplay(SpellUIDisplay sd) {
        spellDisplay = sd;
    }

    public void DisplaySpell(){
        if (spellDisplay != null) {
            spellDisplay.PopulateFromSpell(spell);
            spellDisplay.ShowSpell();
        }
    }
}
