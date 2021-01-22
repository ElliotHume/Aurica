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

    public void SendAura(ManaDistribution a) {
        aura = a;
        Debug.Log("AURA recieved: " + a.ToString());
    }

    public void UpdateComponent(AuricaSpellComponent c) {
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
}
