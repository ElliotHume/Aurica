using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;

public class AuricaCaster : MonoBehaviour {

    public Aura aura;

    // Lists of all components and spells
    private AuricaSpellComponent[] allComponents;
    private AuricaSpell[] allSpells;

    // Runtime variables
    private List<AuricaSpellComponent> currentComponents;
    private ManaDistribution currentDistribution;
    private AuricaSpell spellMatch;
    private float currentManaCost;

    // Start is called before the first frame update
    void Start() {
        allComponents = Resources.LoadAll<AuricaSpellComponent>("AuricaSpellComponents");
        allSpells = Resources.LoadAll<AuricaSpell>("AuricaSpells");

        if (aura == null) aura = GetComponent<Aura>();
    }

    public void AddComponent(string componentName) {
        foreach (AuricaSpellComponent c in allComponents) {
            if (c.c_name == componentName) {
                AddComponent(c);
                break;
            }
        }
    }

    public void AddComponent(AuricaSpellComponent newComponent) {
        if (currentComponents.Count >= 6) {
            currentManaCost += 10f;
        } else if (currentComponents.Count > 8) {
            currentManaCost += 20f;
        }
        currentComponents.Add(newComponent);
        currentManaCost += newComponent.GetManaCost(aura.GetAura());
        ManaDistribution oldMd = currentDistribution;
        currentDistribution = newComponent.CalculateDistributionChange(currentDistribution, aura.GetAura());

        Debug.Log("Added component: " + newComponent.c_name + "    Current Mana Cost: " + currentManaCost);
        Debug.Log("Old Distribution: " + oldMd.ToList() + "    New Distribution: " + currentDistribution.ToString());
    }

    public AuricaSpell Cast() {
        float bestMatchError = 999f;
        foreach (AuricaSpell s in allSpells) {
            if (s.CheckComponents(currentComponents) && s.GetError(currentDistribution) <= s.errorThreshold && s.GetError(currentDistribution) < bestMatchError) {
                spellMatch = s;
            }
        }

        return spellMatch;
    }

}
