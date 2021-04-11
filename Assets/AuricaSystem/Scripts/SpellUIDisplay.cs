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
    public GameObject placeholder, spellStrengthTitle, manaCostTitle;
    public bool isCasterAgnostic = false;

    private AuricaSpell spell;
    private bool isHidden = true;

    // Start is called before the first frame update
    void Start() {
        HideSpell();
    }

    public void AddComponentFromComponentDisplay() {
        // componentUIList.AddComponent(componentUIDisplay.component);
        AuricaCaster.LocalCaster.AddComponent(componentUIDisplay.component);
        CheckComponents();
    }

    public void CheckComponents() {
        spell = AuricaCaster.LocalCaster.Cast();
        try {
            if (spell.c_name != null) {
                if (isHidden) ShowSpell();
                PopulateFromSpell(spell);
            }
        } catch {
            // do nothing, no spell match
        }
    }

    public void PopulateFromSpell(AuricaSpell spell) {
        title.text = spell.c_name;
        description.text = spell.description;
        targetDistDisplay.SetDistribution(spell.targetDistribution);
        targetDistDisplayValues.SetDistribution(spell.targetDistribution);
        // Debug.Log("IDEAL AURA:    "+spell.IdealAuraCalculation().ToString());
        if (!isCasterAgnostic) {
            spellEfficacyText.text = string.Format("{0:N2}", AuricaCaster.LocalCaster.GetSpellStrength() * 100f) + "%";
            manaCostText.text = string.Format("{0:N2}", AuricaCaster.LocalCaster.GetManaCost());
        } 
        if (componentUIList != null) {
            componentUIList.ResetList();
            foreach ( var component in spell.keyComponents) {
                componentUIList.AddComponent(component);
            }
        }

        StartCoroutine(SetTargetDist(spell.targetDistribution));
    }

    IEnumerator SetTargetDist(ManaDistribution target) {
        yield return new WaitForSeconds(0.2f);
        targetDistDisplay.SetDistribution(target);
        targetDistDisplayValues.SetDistribution(target);
    }

    public void ShowSpell() {
        title.gameObject.SetActive(true);
        description.gameObject.SetActive(true);
        if (spellEfficacyText != null) spellEfficacyText.gameObject.SetActive(true);
        if (manaCostText != null) manaCostText.gameObject.SetActive(true);
        targetDistDisplay.gameObject.SetActive(true);
        targetDistDisplayValues.gameObject.SetActive(true);
        if (spellStrengthTitle != null) spellStrengthTitle.SetActive(true);
        if (manaCostTitle != null) manaCostTitle.SetActive(true);
        if (componentUIList != null) componentUIList.gameObject.transform.parent.parent.gameObject.SetActive(true);
        if (placeholder != null) placeholder.SetActive(false);
        isHidden = false;
    }

    public void HideSpell() {
        title.gameObject.SetActive(false);
        description.gameObject.SetActive(false);
        if (spellEfficacyText != null) spellEfficacyText.gameObject.SetActive(false);
        if (manaCostText != null) manaCostText.gameObject.SetActive(false);
        targetDistDisplay.gameObject.SetActive(false);
        targetDistDisplayValues.gameObject.SetActive(false);
        if (componentUIList != null) componentUIList.gameObject.transform.parent.parent.gameObject.SetActive(false);
        if (spellStrengthTitle != null) spellStrengthTitle.SetActive(false);
        if (manaCostTitle != null) manaCostTitle.SetActive(false);
        if (placeholder != null) placeholder.SetActive(true);
        isHidden = true;
    }

    public void Discard() {
        if (!isCasterAgnostic) AuricaCaster.LocalCaster.ResetCast();
        ClearSpell();
    }

    public void ClearSpell() {
        if (componentUIList != null) {
            componentUIList.ResetList();
            componentUIList.WipeList();
        }
        HideSpell();
    }

    public void CacheSpell(string key) {
        AuricaCaster.LocalCaster.CacheCurrentSpell(key);
    }
}
