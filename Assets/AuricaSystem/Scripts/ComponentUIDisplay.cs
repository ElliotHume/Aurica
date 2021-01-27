using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComponentUIDisplay : MonoBehaviour {
    public Text Title, Description;
    public GameObject BasicDistDisplayGO, AuricDistDisplayGO, FluxDistDisplayGO;
    public DistributionUIDisplay BasicDistDisplay, AuricDistDisplay;
    public DistributionUIDisplayValues FluxDistDisplay;
    public AuricaSpellComponent component;
    
    private ManaDistribution aura;
    private bool isHidden = true;
    private AuricaSpellComponent[] allComponents;

    void Start() {
        Hide();
        allComponents = Resources.LoadAll<AuricaSpellComponent>("AuricaSpellComponents");
    }

    public void SendAura(ManaDistribution a) {
        aura = a;
        Debug.Log("AURA recieved: " + a.ToString());
    }

    public void UpdateComponent(AuricaSpellComponent c) {
        if (isHidden) Show();

        component = c;
        Title.text = component.c_name;
        Description.text = component.description;

        if (component.hasBasicDistribution) {
            BasicDistDisplayGO.SetActive(true);
            BasicDistDisplay.SetDistribution(component.basicDistribution);
        } else {
            BasicDistDisplayGO.SetActive(false);
        }

        if (component.hasAuricDistribution) {
            AuricDistDisplayGO.SetActive(true);
            AuricDistDisplay.SetDistribution(component.auricDistribution * aura);
        } else {
            AuricDistDisplayGO.SetActive(false);
        }

        if (component.hasFluxDistribution) {
            FluxDistDisplayGO.SetActive(true);
            FluxDistDisplay.SetDistribution(component.fluxDistribution);
        } else {
            FluxDistDisplayGO.SetActive(false);
        }
    }

    public void UpdateComponentByName(string componentName) {
        foreach (AuricaSpellComponent c in allComponents) {
            if (c.c_name == componentName) {
                UpdateComponent(c);
                break;
            }
        }
    }

    public void Hide() {
        Title.gameObject.SetActive(false);
        Description.gameObject.SetActive(false);
        BasicDistDisplayGO.SetActive(false);
        AuricDistDisplayGO.SetActive(false);
        FluxDistDisplayGO.SetActive(false);
        isHidden = true;
    }

    public void Show() {
        Title.gameObject.SetActive(true);
        Description.gameObject.SetActive(true);
        BasicDistDisplayGO.SetActive(true);
        AuricDistDisplayGO.SetActive(true);
        FluxDistDisplayGO.SetActive(true);
        isHidden = false;
    }
}
