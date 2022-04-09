using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MasteryUIRow : MonoBehaviour {

    public MasteryManager.MasteryCategories category;

    public Slider masterySlider;
    public GameObject noviceSpellSlot, adeptSpellSlot, masterSpellSlot, legendSpellSlot;
    public Text noviceSpellText, adeptSpellText, masterSpellText, legendSpellText, ValueText;
    public Image noviceSpellIcon, adeptSpellIcon, masterSpellIcon, legendSpellIcon;
    public Color lockedColor;

    private AuricaSpell[] allSpells;
    private List<AuricaSpell> spellsList = new List<AuricaSpell>();
    private AuricaSpell noviceSpell, adeptSpell, masterSpell, legendSpell;

    void Start() {
        allSpells = Resources.LoadAll<AuricaSpell>("AuricaSpells");
        allSpells = allSpells.Where((s) => s.isMasterySpell && s.masteryCategory == category).ToArray();
        spellsList = new List<AuricaSpell>(allSpells);
        ReRender();
    }

    void OnEnable() {
        if (spellsList != null) ReRender();
    }

    public void ReRender() {
        noviceSpellSlot.SetActive(false);
        adeptSpellSlot.SetActive(false);
        masterSpellSlot.SetActive(false);
        legendSpellSlot.SetActive(false);

        

        int mastery = MasteryManager.Instance.GetMastery(category);
        if (ValueText != null) ValueText.text = mastery.ToString();
        float sliderValue = Mathf.Min(mastery, 10f) + (22 * Mathf.Clamp(mastery-10f, 0f, 90f)/100f) + (34 * Mathf.Clamp(mastery-110f, 0f, 890f)/1000f) + (45 * Mathf.Clamp(mastery-1110f, 0f, 8890f)/10000f);
        masterySlider.value = Mathf.Clamp(sliderValue, 0f, 100f);

        if (spellsList != null && spellsList.Count == 0) {
            Debug.Log("No mastery spells found for: "+category.ToString());
            return;
        }
        foreach(AuricaSpell spell in spellsList) {
            switch (spell.masteryLevel) {
                case MasteryManager.MasteryLevel.Novice:
                    noviceSpell = spell;
                    noviceSpellSlot.SetActive(true);
                    noviceSpellText.text = spell.c_name;
                    noviceSpellIcon.sprite = ResourceManager.Instance.GetMasteryIcon(spell.manaType);
                    noviceSpellIcon.color = MasteryManager.Instance.HasMasteryForSpell(spell) ? new Color(1f, 1f, 1f) : lockedColor;
                    break;
                case MasteryManager.MasteryLevel.Adept:
                    adeptSpell = spell;
                    adeptSpellSlot.SetActive(true);
                    adeptSpellText.text = spell.c_name;
                    adeptSpellIcon.sprite = ResourceManager.Instance.GetMasteryIcon(spell.manaType);
                    adeptSpellIcon.color = MasteryManager.Instance.HasMasteryForSpell(spell) ? new Color(1f, 1f, 1f) : lockedColor;
                    break;
                case MasteryManager.MasteryLevel.Master:
                    masterSpell = spell;
                    masterSpellSlot.SetActive(true);
                    masterSpellText.text = spell.c_name;
                    masterSpellIcon.sprite = ResourceManager.Instance.GetMasteryIcon(spell.manaType);
                    masterSpellIcon.color = MasteryManager.Instance.HasMasteryForSpell(spell) ? new Color(1f, 1f, 1f) : lockedColor;
                    break;
                case MasteryManager.MasteryLevel.Legend:
                    legendSpell = spell;
                    legendSpellSlot.SetActive(true);
                    legendSpellText.text = spell.c_name;
                    legendSpellIcon.sprite = ResourceManager.Instance.GetMasteryIcon(spell.manaType);
                    legendSpellIcon.color = MasteryManager.Instance.HasMasteryForSpell(spell) ? new Color(1f, 1f, 1f) : lockedColor;
                    break;
            }
        }
    }

    public void CastMasterySpell(int level) {
        switch (level) {
            case 0:
                if (noviceSpell != null) AuricaCaster.LocalCaster.CastSpellByObject(noviceSpell);
                break;
            case 1:
                if (adeptSpell != null) AuricaCaster.LocalCaster.CastSpellByObject(adeptSpell);
                break;
            case 2:
                if (masterSpell != null) AuricaCaster.LocalCaster.CastSpellByObject(masterSpell);
                break;
            case 3:
                if (legendSpell != null) AuricaCaster.LocalCaster.CastSpellByObject(legendSpell);
                break;
        }
    }
}
