using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellUIDisplay : MonoBehaviour {
    public Text title, description, spellEfficacyText, manaCostText;
    public DistributionUIDisplay targetDistDisplay;
    public DistributionUIDisplayValues targetDistDisplayValues;
    public ComponentUIList componentUIList;
    public ComponentUIDisplay componentUIDisplay;

    private AuricaSpell spell;
    private bool isHidden = true;

    // Start is called before the first frame update
    void Start() {
        HideSpell();
    }

    public void AddComponentFromComponentDisplay() {
        componentUIList.AddComponent(componentUIDisplay.component);
        AuricaCaster.LocalCaster.AddComponent(componentUIDisplay.component);
        CheckComponents();
    }

    public void CheckComponents() {
        spell = AuricaCaster.LocalCaster.Cast();
        if (spell.c_name != null) {
            PopulateFromSpell(spell);
            if (isHidden) ShowSpell();
        }
    }

    public void PopulateFromSpell(AuricaSpell spell) {
        title.text = spell.c_name;
        description.text = spell.description;
        targetDistDisplay.SetDistribution(spell.targetDistribution);
        targetDistDisplayValues.SetDistribution(spell.targetDistribution);
        spellEfficacyText.text = string.Format("{0:N2}", AuricaCaster.LocalCaster.GetSpellStrength() * 100f)+"%";
        manaCostText.text = string.Format("{0:N2}", AuricaCaster.LocalCaster.GetManaCost());
        StartCoroutine(SetTargetDist(spell.targetDistribution));
    }

    IEnumerator SetTargetDist(ManaDistribution target) {
        yield return new WaitForSeconds(0.5f);
        targetDistDisplay.SetDistribution(target);
    }

    public void ShowSpell() {
        title.gameObject.SetActive(true);
        description.gameObject.SetActive(true);
        spellEfficacyText.gameObject.SetActive(true);
        manaCostText.gameObject.SetActive(true);
        targetDistDisplay.gameObject.SetActive(true);
        targetDistDisplayValues.gameObject.SetActive(true);
        isHidden = false;
    }

    public void HideSpell() {
        title.gameObject.SetActive(false);
        description.gameObject.SetActive(false);
        spellEfficacyText.gameObject.SetActive(false);
        manaCostText.gameObject.SetActive(false);
        targetDistDisplay.gameObject.SetActive(false);
        targetDistDisplayValues.gameObject.SetActive(false);
        isHidden = true;
    }

    public void Discard(){
        AuricaCaster.LocalCaster.ResetCast();
        ClearSpell();
    }

    public void ClearSpell() {
        componentUIList.ResetList();
        componentUIList.WipeList();
        HideSpell();
    }

    public void CacheSpell(string key) {
        AuricaCaster.LocalCaster.CacheCurrentSpell(key);
    }
}
