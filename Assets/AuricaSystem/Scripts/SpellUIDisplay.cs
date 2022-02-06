using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellUIDisplay : MonoBehaviour {
    public GameObject displayPanel, craftingPanel;
    public SpellCraftingMatrix spellMatrix;
    public Text title, description, spellEfficacyText, manaCostText;
    public DistributionUIDisplay targetDistDisplay;
    public DistributionUIDisplayValues targetDistDisplayValues;
    public ComponentUIList componentUIList;
    public ComponentUIDisplay componentUIDisplay;
    public GameObject placeholder, spellStrengthTitle, manaCostTitle;
    public bool isCasterAgnostic = false;

    public AuricaSpell spell;
    private bool isHidden = true;

    // Start is called before the first frame update
    void Start() {
        HideSpell();
    }

    void Update() {
        if (Input.GetKey(KeyCode.LeftShift)) {
            if (Input.GetKeyDown("1")) {
                AuricaCaster.LocalCaster.CastBindSlot("1");
                CheckComponents();
            } else if (Input.GetKeyDown("2")) {
                AuricaCaster.LocalCaster.CastBindSlot("2");
                CheckComponents();
            } else if (Input.GetKeyDown("3")) {
                AuricaCaster.LocalCaster.CastBindSlot("3");
                CheckComponents();
            } else if (Input.GetKeyDown("4")) {
                AuricaCaster.LocalCaster.CastBindSlot("4");
                CheckComponents();
            } else if (Input.GetKeyDown("e")) {
                AuricaCaster.LocalCaster.CastBindSlot("e");
                CheckComponents();
            } else if (Input.GetKeyDown("q")) {
                AuricaCaster.LocalCaster.CastBindSlot("q");
                CheckComponents();
            } else if (Input.GetKeyDown("r")) {
                AuricaCaster.LocalCaster.CastBindSlot("r");
                CheckComponents();
            } else if (Input.GetKeyDown("f")) {
                AuricaCaster.LocalCaster.CastBindSlot("f");
                CheckComponents();
            }

            if (Input.GetKeyDown(KeyCode.Return)) {
                AddComponentFromComponentDisplay();
            }
        }
    }

    public void AddComponentFromComponentDisplay() {
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
            if (spellMatrix != null) spellMatrix.CheckComponents();
        }
    }

    public void PopulateFromSpell(AuricaSpell s) {
        spell = s;
        title.text = spell.c_name;
        description.text = spell.description;
        targetDistDisplay.SetDistribution(spell.targetDistribution);
        targetDistDisplayValues.SetDistribution(spell.targetDistribution);
        if (Input.GetKey(KeyCode.RightShift)) {
            Debug.Log("IDEAL AURA:    "+spell.IdealAuraCalculation().ToString());
            Debug.Log("COMBINED AURIC DISTRIBUTION:    "+spell.GetCombinedAuricDistribution().ToString());
            Debug.Log("COMBINED BASIC DISTRIBUTION:    "+spell.GetCombinedBasicDistribution().ToString());
        }
        if (isCasterAgnostic) AuricaCaster.LocalCaster.CastSpellByObject(s);
        spellEfficacyText.text = string.Format("{0:N2}", AuricaCaster.LocalCaster.GetSpellStrength() * 100f) + "%";
        manaCostText.text = string.Format("{0:N2}", AuricaCaster.LocalCaster.GetManaCost()); 
        if (componentUIList != null) {
            componentUIList.ResetList();
            foreach ( var component in spell.keyComponents) {
                componentUIList.AddComponent(component);
            }
        }

        if (gameObject.activeInHierarchy) StartCoroutine(SetTargetDist(spell.targetDistribution));
    }

    IEnumerator SetTargetDist(ManaDistribution target) {
        yield return new WaitForSeconds(0.2f);
        targetDistDisplay.SetDistribution(target);
        targetDistDisplayValues.SetDistribution(target);
    }

    public void ShowSpell() {
        if (displayPanel != null) {
            displayPanel.SetActive(true);
            craftingPanel.SetActive(false);
        } else {
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
        }
        isHidden = false;
    }

    public void HideSpell() {
        if (displayPanel != null) {
            displayPanel.SetActive(false);
            craftingPanel.SetActive(true);
        } else {
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
        }
        isHidden = true;
    }

    public void Discard() {
        if (!isCasterAgnostic) AuricaCaster.LocalCaster.ResetCast();
        if (spellMatrix != null) spellMatrix.CheckComponents();
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

    public void CastSpellComponents() {
        if (spell != null && spell.c_name != null) {
            AuricaCaster.LocalCaster.ResetCast();
            foreach(var keyComponent in spell.keyComponents) {
                AuricaCaster.LocalCaster.AddComponent(keyComponent);
            }
        }
    }
}
