using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AdVd.GlyphRecognition;

public class SpellUIDisplay : MonoBehaviour {
    public GameObject displayPanel, craftingPanel;
    public SpellCraftingMatrix spellMatrix;
    public Text title, description, spellEfficacyText, manaCostText;
    public DistributionUIDisplay targetDistDisplay;
    public DistributionUIDisplayValues targetDistDisplayValues;
    public ComponentUIList componentUIList;
    public ComponentUIDisplay componentUIDisplay;
    public GameObject placeholder, spellStrengthTitle, manaCostTitle;
    public List<GlyphDisplay> glyphDisplays;
    public bool isCasterAgnostic = false;

    public AuricaSpell spell;
    private bool isHidden = true;
    private Glyph[] allComponentGlyphs;
    // private bool addAttempted = false;
    private InputManager inputManager;

    // Start is called before the first frame update
    void Start() {
        HideSpell();
        allComponentGlyphs = Resources.LoadAll<Glyph>("Glyphs");
        inputManager = InputManager.Instance;
    }

    void OnEnable() {
        CheckComponents();
    }

    void Update() {
        if (GameUIPanelManager.Instance.IsEditingInputField()) return;
        if (inputManager == null) inputManager = InputManager.Instance;
        if (inputManager.GetKeyDown(KeybindingActions.SpellSlot1)) {
            AuricaCaster.LocalCaster.CastBindSlot("1");
            CheckComponents();
        } else if ((inputManager.GetKeyDown(KeybindingActions.SpellSlot2))) {
            AuricaCaster.LocalCaster.CastBindSlot("2");
            CheckComponents();
        } else if ((inputManager.GetKeyDown(KeybindingActions.SpellSlot3))) {
            AuricaCaster.LocalCaster.CastBindSlot("3");
            CheckComponents();
        } else if ((inputManager.GetKeyDown(KeybindingActions.SpellSlot4))) {
            AuricaCaster.LocalCaster.CastBindSlot("4");
            CheckComponents();
        } else if ((inputManager.GetKeyDown(KeybindingActions.SpellSlotE))) {
            AuricaCaster.LocalCaster.CastBindSlot("e");
            CheckComponents();
        } else if ((inputManager.GetKeyDown(KeybindingActions.SpellSlotQ))) {
            AuricaCaster.LocalCaster.CastBindSlot("q");
            CheckComponents();
        } else if ((inputManager.GetKeyDown(KeybindingActions.SpellSlotR))) {
            AuricaCaster.LocalCaster.CastBindSlot("r");
            CheckComponents();
        } else if ((inputManager.GetKeyDown(KeybindingActions.SpellSlotF))) {
            AuricaCaster.LocalCaster.CastBindSlot("f");
            CheckComponents();
        }
        
        // if (Input.GetKeyDown(KeyCode.Return)) {
        //     // If the player double taps enter when searching for a component, add the component to the spell.
        //     if (GameUIPanelManager.Instance.HasSelectedInputField()) {
        //         if (addAttempted) {
        //             AddComponentFromComponentDisplay();
        //             addAttempted = false;
        //         } else {
        //             addAttempted = true;
        //         }
        //     } else {
        //         AddComponentFromComponentDisplay();
        //     }
        // }
    }

    public void AddComponentFromComponentDisplay() {
        if (componentUIDisplay.component == null) return;
        AuricaCaster.LocalCaster.AddComponent(componentUIDisplay.component);
        CheckComponents();
    }

    public void CheckComponents() {
        if (AuricaCaster.LocalCaster == null) return;
        // addAttempted = false;
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
        if (glyphDisplays.Count > 0) {
            foreach(GlyphDisplay gd in glyphDisplays) {
                gd.glyph = null;
            }
            for(int i=0; i < Mathf.Min(s.keyComponents.Count, 8); i++ ) {
                foreach (Glyph item in allComponentGlyphs) {
                    if (item.name == s.keyComponents[i].c_name) {
                        glyphDisplays[i].glyph = item;
                        break;
                    }
                }
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
