using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AdVd.GlyphRecognition;
using TMPro;

public class GrimoireSpellUIDisplay : MonoBehaviour {
    public Text title, description;
    public TMP_Text auraCompatibilityText, spellStrengthText, difficultyRankText, startingManaCostText;
    public Image auraCompatibilityColor, difficultyRankColour;
    public DistributionUIDisplay targetDistDisplay;
    public GameObject placeholder, displayPanel;
    public List<UIGlyphDisplay> glyphDisplays;

    public Color excellentColor, goodColor, moderateColor, poorColor, terribleColor;
    public Color rank1, rank2, rank3, rank4;
    public bool standalone = false;

    public AuricaSpell spell;
    private Glyph[] allComponentGlyphs;

    // Start is called before the first frame update
    void Start() {
        if (!standalone) HideSpell();
        allComponentGlyphs = Resources.LoadAll<Glyph>("Glyphs");
    }

    public void PopulateFromSpell(AuricaSpell s, float ss=-1f) {
        ShowSpell();
        // Debug.Log("SHOWING SPELL: "+s.c_name);
        if (allComponentGlyphs == null || allComponentGlyphs.Length == 0) allComponentGlyphs = Resources.LoadAll<Glyph>("Glyphs");

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
        float startingManaCost = AuricaCaster.LocalCaster.GetManaCost();
        if (standalone) AuricaCaster.LocalCaster.ResetCast();

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
        if (spellStrengthText != null) {
            spellStrengthText.text = string.Format("{0:N2}", spellStrength * 100f) + "%";
            if ((spell.difficultyRank == AuricaSpell.DifficultyRank.Rank3 && spellStrength < AuricaSpell.RANK3_SPELL_STRENGTH_CUTOFF) || (spell.difficultyRank == AuricaSpell.DifficultyRank.Rank4 && spellStrength < AuricaSpell.RANK4_SPELL_STRENGTH_CUTOFF)) {
                spellStrengthText.color = Color.red;
            } else {
                spellStrengthText.color = Color.black;
            }
        }
        if (startingManaCostText != null) startingManaCostText.text = string.Format("{0:N2}", startingManaCost);

        if (spell.difficultyRank == AuricaSpell.DifficultyRank.Rank1) {
            difficultyRankText.text = "Rank 1 - Easy";
            difficultyRankColour.color = rank1;
        } else if (spell.difficultyRank == AuricaSpell.DifficultyRank.Rank2) {
            difficultyRankText.text = "Rank 2 - Moderate";
            difficultyRankColour.color = rank2;
        } else if (spell.difficultyRank == AuricaSpell.DifficultyRank.Rank3) {
            difficultyRankText.text = "Rank 3 - Hard";
            difficultyRankColour.color = rank3;
        } else if (spell.difficultyRank == AuricaSpell.DifficultyRank.Rank4) {
            difficultyRankText.text = "Rank 4 - Punishing";
            difficultyRankColour.color = rank4;
        }

        if (gameObject.activeInHierarchy) StartCoroutine(SetTargetDist(spell.targetDistribution));
    }

    IEnumerator SetTargetDist(ManaDistribution target) {
        yield return new WaitForSeconds(0.2f);
        targetDistDisplay.SetDistribution(target);
    }

    public void ShowSpell() {
        displayPanel.SetActive(true);
        placeholder.SetActive(false);
    }

    public void HideSpell() {
        displayPanel.SetActive(false);
        placeholder.SetActive(true);
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
