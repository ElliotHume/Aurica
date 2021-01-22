using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComponentUIDisplay : MonoBehaviour
{
    public Text Title, Description;
    public GameObject BasicDistDisplayGO, AuricDistDisplayGO, FluxDistDisplayGO;
    public DistributionUIDisplay BasicDistDisplay, AuricDistDisplay, FluxDistDisplay;

    private ManaDistribution aura;

    public void SendAura(ManaDistribution a) {
        aura = a;
    }

    public void UpdateComponent(AuricaSpellComponent component) {
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
