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
    private List<AuricaSpellComponent> components;
    private ManaDistribution distribution;
    private bool isHidden = true;

    // Start is called before the first frame update
    void Start() {
        distribution = new ManaDistribution();
        components = new List<AuricaSpellComponent>();
        HideSpell();
    }

    public void AddComponentFromComponentDisplay() {
        componentUIList.AddComponent(componentUIDisplay.component);
        components.Add(componentUIDisplay.component);

        if (CheckComponents() && isHidden) ShowSpell();
    }

    public bool CheckComponents() {
        spell = AuricaCaster.LocalCaster.GetSpellMatch(components, distribution);
        return spell != null;
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
        targetDistDisplay.gameObject.SetActive(false);
        targetDistDisplayValues.gameObject.SetActive(false);
        isHidden = true;
    }
}
