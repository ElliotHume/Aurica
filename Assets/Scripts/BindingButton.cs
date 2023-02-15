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
    public GameObject Spinner, FlashingFrame, InsufficientStrengthOverlay;

    private AuricaSpell spell;
    private bool canCast;
    private int recastState;
    private bool hasSufficientStrength;

    public void SetButtonGraphics(AuricaSpell s, string components="") {
        if (s == null) {
            if (SpellText != null) SpellText.text = "NONE";
            if (AlternateSpellText != null) AlternateSpellText.text = "NONE";
            SpellIcon.sprite = ResourceManager.Instance.AuricIcon;
            return;
        }
        // Debug.Log("Set button graphics for: "+gameObject+" with spell: "+s.c_name+"  components: "+components);

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

    public void HasNecessarySpellStrength(bool flag) {
        if (flag == hasSufficientStrength) return;
        hasSufficientStrength = flag;
        InsufficientStrengthOverlay.SetActive(flag);
    }

    public void CanRecast(bool flag, bool displaySpinner) {
        int newRecastState = 0;
        if (flag) newRecastState += 1;
        if (displaySpinner) newRecastState += 1;

        if (newRecastState != recastState) {
            recastState = newRecastState;
            Spinner.SetActive(displaySpinner);
            FlashingFrame.SetActive(flag);
            SpellIcon.color = flag ? CannotCastColorTint : new Color(1f, 1f, 1f);
        }
    }

    public void SetKeyText(string newText) {
        KeyText.text = newText;
    }
}
