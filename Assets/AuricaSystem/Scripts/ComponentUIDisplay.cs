using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ComponentUIDisplay : MonoBehaviour {
    public Text Title, Description;
    public GameObject ManaDistDisplayGO, FluxDistDisplayGO;
    public DistributionUIDisplay ManaDistDisplay;
    public DistributionUIDisplayValues FluxDistDisplay;
    public AuricaSpellComponent component;
    public GameObject placeholder;

    public UnityEvent onComponentCast;
    
    private ManaDistribution aura;
    private bool isHidden = true;
    private AuricaSpellComponent[] allComponents;

    void Start() {
        Hide();
        allComponents = Resources.LoadAll<AuricaSpellComponent>("AuricaSpellComponents");
    }

    public void UpdateComponent(AuricaSpellComponent c) {
        if (isHidden) Show();
        ActivateCP(c);
        // There is a bug with enabling objects and changing the values at the same time,
        // so... change the values again soon after enabling to display values correctly
        StartCoroutine(UpdateComponentAgain(c));
    }

    IEnumerator UpdateComponentAgain(AuricaSpellComponent c) {
        yield return new WaitForSeconds(0.1f);
        ActivateCP(c);
    }

    void ActivateCP(AuricaSpellComponent c) {
        component = c;
        Title.text = component.c_name;
        Description.text = component.description;

        ManaDistribution md = (component.hasBasicDistribution ? component.basicDistribution : new ManaDistribution()) + (component.hasAuricDistribution ? (component.auricDistribution * PlayerManager.LocalInstance.aura.GetAura()) : new ManaDistribution());
        ManaDistDisplayGO.SetActive(true);
        ManaDistDisplay.SetDistribution(md);

        if (component.hasFluxDistribution) {
            FluxDistDisplayGO.SetActive(true);
            FluxDistDisplay.SetDistribution(component.fluxDistribution);
        } else {
            FluxDistDisplayGO.SetActive(false);
        }
    }

    public void UpdateComponentByName(string componentName) {
        foreach (AuricaSpellComponent c in allComponents) {
            if (c.c_name.ToLower() == componentName.ToLower()) {
                UpdateComponent(c);
                break;
            }
        }
    }

    public void Hide() {
        Title.gameObject.SetActive(false);
        Description.gameObject.SetActive(false);
        ManaDistDisplayGO.SetActive(false);
        FluxDistDisplayGO.SetActive(false);
        if (placeholder != null) placeholder.SetActive(true);
        isHidden = true;
    }

    public void Show() {
        Title.gameObject.SetActive(true);
        Description.gameObject.SetActive(true);
        ManaDistDisplayGO.SetActive(true);
        FluxDistDisplayGO.SetActive(true);
        if (placeholder != null) placeholder.SetActive(false);
        isHidden = false;
    }
}
