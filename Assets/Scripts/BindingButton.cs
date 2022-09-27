using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BindingButton : MonoBehaviour
{
    public TMP_Text SpellText, KeyText;
    public Text AlternateSpellText;
    public Image SpellIcon;
    public Color CannotCastColorTint;
    public Text SpellComponents;

    private AuricaSpell spell;
    private bool canCast;

    public void SetButtonGraphics(AuricaSpell s, string components="") {
        if (s == null) {
            if (SpellText != null) SpellText.text = "NONE";
            if (AlternateSpellText != null) AlternateSpellText.text = "NONE";
            SpellIcon.sprite = ResourceManager.Instance.AuricIcon;
            return;
        }

        spell = s;
        if (SpellText != null) SpellText.text = spell.c_name;
        if (AlternateSpellText != null) AlternateSpellText.text = spell.c_name;
        SpellIcon.sprite = !spell.isMasterySpell ? ResourceManager.Instance.GetIcon(spell.manaType) : ResourceManager.Instance.GetMasteryIcon(spell.manaType);
        if (SpellComponents != null) SpellComponents.text = components;
    }

    public void CanCast(bool flag) {
        if (flag && !canCast) {
            canCast = true;
            SpellIcon.color = new Color(1f, 1f, 1f);
        }
        else if (!flag && canCast) {
            canCast = false;
            SpellIcon.color = CannotCastColorTint;
        }
    }

    public void SetKeyText(string newText) {
        KeyText.text = newText;
    }
}
