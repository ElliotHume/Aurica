using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AdVd.GlyphRecognition;
using TMPro;

public class GrimoireSpellUIDisplay : MonoBehaviour {
    public Text title, description;
    public TMP_Text auraCompatibilityText, spellStrengthText;
    public Image auraCompatibilityColor;
    public DistributionUIDisplay targetDistDisplay;
    public GameObject placeholder, displayPanel;
    public List<UIGlyphDisplay> glyphDisplays;

    public Color excellentColor, goodColor, moderateColor, poorColor, terribleColor;

    public AuricaSpell spell;
    private bool isHidden = true;
    private Glyph[] allComponentGlyphs;

    // Start is called before the first frame update
    void Start() {
        HideSpell();
        allComponentGlyphs = Resources.LoadAll<Glyph>("Glyphs");
    }

    public void PopulateFromSpell(AuricaSpell s) {
        if (isHidden) ShowSpell();

        spell = s;
        title.text = spell.c_name;
        description.text = spell.description;
        targetDistDisplay.SetDistribution(spell.targetDistribution);
        if (glyphDisplays.Count > 0) {
            foreach(UIGlyphDisplay gd in glyphDisplays) {
                gd.Clear();
            }
            for(int i=0; i < Mathf.Min(s.keyComponents.Count, 8); i++ ) {
                foreach (Glyph item in allComponentGlyphs) {
                    if (item.name == s.keyComponents[i].c_name) {
                        glyphDisplays[i].SetGlyph(item, s.keyComponents[i].c_name);
                        break;
                    }
                }
            }
        }

        AuricaCaster.LocalCaster.CastSpellByObject(s);
        float spellStrength = AuricaCaster.LocalCaster.GetSpellStrength();
        if (spellStrength <= 0.5) {
            auraCompatibilityText.text = "Terrible";
            auraCompatibilityColor.color = terribleColor;
        } else if (spellStrength <= 0.75) {
            auraCompatibilityText.text = "Poor";
            auraCompatibilityColor.color = poorColor;
        } else if (spellStrength <= 1) {
            auraCompatibilityText.text = "Moderate";
            auraCompatibilityColor.color = moderateColor;
        } else if (spellStrength <= 1.25) {
            auraCompatibilityText.text = "Good";
            auraCompatibilityColor.color = goodColor;
        } else {
            auraCompatibilityText.text = "Excellent";
            auraCompatibilityColor.color = excellentColor;
        }
        spellStrengthText.text = string.Format("{0:N2}", spellStrength * 100f) + "%";

        if (gameObject.activeInHierarchy) StartCoroutine(SetTargetDist(spell.targetDistribution));
    }

    IEnumerator SetTargetDist(ManaDistribution target) {
        yield return new WaitForSeconds(0.2f);
        targetDistDisplay.SetDistribution(target);
    }

    public void ShowSpell() {
        displayPanel.SetActive(true);
        placeholder.SetActive(false);
        isHidden = false;
    }

    public void HideSpell() {
        displayPanel.SetActive(false);
        placeholder.SetActive(true);
        isHidden = true;
    }

    public void CastSpellComponents() {
        if (spell != null && spell.c_name != null) {
            AuricaCaster.LocalCaster.ResetCast();
            foreach(var keyComponent in spell.keyComponents) {
                AuricaCaster.LocalCaster.AddComponent(keyComponent);
            }
        }
    }
}
