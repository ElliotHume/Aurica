using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BindingButton : MonoBehaviour
{
    public TMP_Text SpellText;
    public Image SpellIcon;
    public string key;

    private AuricaSpell spell;

    public void SetButtonGraphics(AuricaSpell s) {
        if (s == null) {
            SpellText.text = "NONE";
            SpellIcon.sprite = ResourceManager.Instance.AuricIcon;
            return;
        }

        spell = s;
        SpellText.text = spell.c_name;
        SpellIcon.sprite = ResourceManager.Instance.GetIcon(spell.manaType);
    }
}
