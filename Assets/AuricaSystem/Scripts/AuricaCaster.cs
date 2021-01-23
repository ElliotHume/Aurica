using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;

public class AuricaCaster : MonoBehaviourPun {

    public Aura aura;
    public static AuricaCaster LocalCaster;
    public Dictionary<string, CachedSpell> cachedSpells;

    // Lists of all components and spells
    private AuricaSpellComponent[] allComponents;
    private AuricaSpell[] allSpells;

    // Runtime variables
    private List<AuricaSpellComponent> currentComponents;
    private ManaDistribution currentDistribution;
    private float currentManaCost, spellStrength;
    private ComponentUIPanel cpUI;
    private DistributionUIDisplay distDisplay;

    // Start is called before the first frame update
    void Start() {
        allComponents = Resources.LoadAll<AuricaSpellComponent>("AuricaSpellComponents");
        allSpells = Resources.LoadAll<AuricaSpell>("AuricaSpells");
        currentComponents = new List<AuricaSpellComponent>();

        cpUI = GameObject.Find("ComponentPanel").GetComponent<ComponentUIPanel>();
        distDisplay = GameObject.Find("LocalDistributionDisplay").GetComponent<DistributionUIDisplay>();
        if (aura == null) aura = GetComponent<Aura>();

        currentDistribution = new ManaDistribution();
        cachedSpells = new Dictionary<string, CachedSpell>();
        cachedSpells.Add("1", new CachedSpell("infernum, bolt"));
        cachedSpells.Add("2", new CachedSpell("mortuus, bolt"));
        cachedSpells.Add("3", new CachedSpell("mana, bolt"));
        cachedSpells.Add("4", new CachedSpell("ordo, bolt"));
        cachedSpells.Add("5", new CachedSpell("demuus, bolt"));
        cachedSpells.Add("6", new CachedSpell("terrak, contain, curse"));
        cachedSpells.Add("7", new CachedSpell("travel, control, terrak, other"));
        cachedSpells.Add("8", new CachedSpell("throw, infernum, expel"));
        cachedSpells.Add("9", new CachedSpell("collect, divinus, expel, curse"));
        cachedSpells.Add("0", new CachedSpell("collect, vivus, self"));
        cachedSpells.Add("e", new CachedSpell("protect, self"));
    }

    void Awake() {
        if (photonView.IsMine) AuricaCaster.LocalCaster = this;
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
        Debug.Log("Old Distribution: " + oldMd.ToString() + "    New Distribution: " + currentDistribution.ToString());

        if (distDisplay != null) distDisplay.SetDistribution(currentDistribution);
        if (cpUI != null) cpUI.AddComponent(newComponent);
    }

    public AuricaSpell Cast() {
        return GetSpellMatch(currentComponents, currentDistribution);
    }

    public AuricaSpell GetSpellMatch(List<AuricaSpellComponent> components, ManaDistribution distribution) {
        float bestMatchError = 999f;
        AuricaSpell spellMatch = null;
        foreach (AuricaSpell s in allSpells) {
            //Debug.Log("Check Spell: " + s.c_name + "   IsMatch: " + s.CheckComponents(components) + "     Error:  " + s.GetError(distribution));
            if (s.CheckComponents(components) && s.GetError(distribution) <= s.errorThreshold && s.GetError(distribution) < bestMatchError) {
                spellMatch = s;
                spellStrength = (spellMatch.errorThreshold - s.GetError(distribution)) / spellMatch.errorThreshold + 0.3f;
            }
        }

        return spellMatch;
    }

    public void ResetCast() {
        currentComponents.Clear();
        currentManaCost = 0f;
        currentDistribution = new ManaDistribution();
        if (cpUI != null) cpUI.HideAllComponents();
        if (distDisplay != null) distDisplay.SetDistribution(currentDistribution);
    }

    public AuricaSpell CastBindSlot(string slot) {
        if(cachedSpells.ContainsKey(slot)) {
            ResetCast();
            CachedSpell cachedSpell = cachedSpells[slot];
            cachedSpell.AddComponents(this);
            return Cast();
        }

        return GetSpellMatch(new List<AuricaSpellComponent>(), new ManaDistribution());
    }

    public float GetManaCost() {
        return currentManaCost;
    }

    public float GetSpellStrength() {
        return spellStrength;
    }

    public ManaDistribution GetCurrentDistribution() {
        return currentDistribution;
    }

    public List<AuricaSpellComponent> GetCurrentComponents() {
        return currentComponents;
    }

}
