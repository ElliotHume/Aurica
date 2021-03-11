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
        cpUI = GameObject.Find("ComponentPanel").GetComponent<ComponentUIPanel>();
        distDisplay = GameObject.Find("LocalDistributionDisplay").GetComponent<DistributionUIDisplay>();
        if (aura == null) aura = GetComponent<Aura>();

        if (PlayerPrefs.HasKey("CachedSpell_e")) {
            cachedSpells.Add("e", new CachedSpell(PlayerPrefs.GetString("CachedSpell_e")));;
        } else {
            cachedSpells.Add("e", new CachedSpell("protect, self"));
        }

        if (PlayerPrefs.HasKey("CachedSpell_q")) {
            cachedSpells.Add("q", new CachedSpell(PlayerPrefs.GetString("CachedSpell_q")));;
        } else {
            cachedSpells.Add("q", new CachedSpell("self, protect, form"));
        }

        if (PlayerPrefs.HasKey("CachedSpell_1")) {
            cachedSpells.Add("1", new CachedSpell(PlayerPrefs.GetString("CachedSpell_1")));;
        } else {
            cachedSpells.Add("1", new CachedSpell("infernum, bolt"));
        }

        if (PlayerPrefs.HasKey("CachedSpell_2")) {
            cachedSpells.Add("2", new CachedSpell(PlayerPrefs.GetString("CachedSpell_2")));;
        } else {
            cachedSpells.Add("2", new CachedSpell("mortuus, bolt"));
        }

        if (PlayerPrefs.HasKey("CachedSpell_3")) {
            cachedSpells.Add("3", new CachedSpell(PlayerPrefs.GetString("CachedSpell_3")));;
        } else {
            cachedSpells.Add("3", new CachedSpell("mana, bolt"));
        }
        
        if (PlayerPrefs.HasKey("CachedSpell_r")) {
            cachedSpells.Add("r", new CachedSpell(PlayerPrefs.GetString("CachedSpell_r")));;
        } else {
            cachedSpells.Add("r", new CachedSpell("throw, infernum, expel"));
        }
    }

    void Awake() {
        if (photonView.IsMine) AuricaCaster.LocalCaster = this;
        allComponents = Resources.LoadAll<AuricaSpellComponent>("AuricaSpellComponents");
        allSpells = Resources.LoadAll<AuricaSpell>("AuricaSpells");
        currentComponents = new List<AuricaSpellComponent>();
        currentDistribution = new ManaDistribution();
        cachedSpells = new Dictionary<string, CachedSpell>();
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

        //Debug.Log("Added component: " + newComponent.c_name + "    Current Mana Cost: " + currentManaCost);
        //Debug.Log("Old Distribution: " + oldMd.ToString() + "    New Distribution: " + currentDistribution.ToString());

        if (distDisplay != null) distDisplay.SetDistribution(currentDistribution);
        if (cpUI != null) cpUI.AddComponent(newComponent);
    }

    public AuricaSpell CastSpellByName(string componentsByName) {
        ResetCast();
        string[] componentSeperator = new string[] { ", " };
        string[] splitComponents = componentsByName.Split(componentSeperator, System.StringSplitOptions.None);
        foreach (string item in splitComponents) {
            AddComponent(item);
        }
        return Cast();
    }

    public AuricaSpell Cast() {
        return GetSpellMatch(currentComponents, currentDistribution);
    }

    public AuricaSpell GetSpellMatch(List<AuricaSpellComponent> components, ManaDistribution distribution) {
        int bestMatchCorrectComponents = 0;
        AuricaSpell spellMatch = null;
        foreach (AuricaSpell s in allSpells) {
            // Debug.Log("Check Spell: " + s.c_name + "   IsMatch: " + s.CheckComponents(components) + "     Error:  " + s.GetError(distribution)+"  Num matching components: "+s.GetNumberOfMatchingComponents(components));
            if (s.CheckComponents(components) && s.GetNumberOfMatchingComponents(components) > bestMatchCorrectComponents) {
                spellMatch = s;
                bestMatchCorrectComponents = s.GetNumberOfMatchingComponents(components);
                spellStrength = (spellMatch.errorThreshold - s.GetError(distribution)) / spellMatch.errorThreshold + 0.3f;
                if (spellStrength < 0.25f) spellStrength = 0.25f;
            }
        }

        return spellMatch;
    }

    public string GetSpellMatchString(string componentString) {
        string[] componentSeperator = new string[] { ", " };
        string[] splitComponents = componentString.Split(componentSeperator, System.StringSplitOptions.None);
        List<AuricaSpellComponent> components = new List<AuricaSpellComponent>();
        foreach (string componentName in splitComponents) {
            foreach (AuricaSpellComponent c in allComponents) {
                if (c.c_name == componentName) {
                    components.Add(c);
                    break;
                }
            }
        }
        string spellMatch = "";
        int bestNumCorrectComponents = 0;
        foreach (AuricaSpell s in allSpells) {
            if (s.CheckComponents(components) && s.GetNumberOfMatchingComponents(components) > bestNumCorrectComponents) {
                spellMatch = s.c_name;
                bestNumCorrectComponents = s.GetNumberOfMatchingComponents(components);
            }
        }

        return spellMatch;
    }

    public void ResetCast() {
        Debug.Log("Resetting Aurica Cast");
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

    public void CacheCurrentSpell(string key) {
        string componentString = "";
        foreach(AuricaSpellComponent c in currentComponents) {
            componentString += c.c_name+", ";
        }
        componentString = componentString.Substring(0, componentString.Length-2);
        Debug.Log("Caching spell: "+componentString);

        if (cachedSpells.ContainsKey(key)) {
            cachedSpells[key] = new CachedSpell(componentString);
        } else {
            cachedSpells.Add(key, new CachedSpell(componentString));
        }
        PlayerPrefs.SetString("CachedSpell_"+key, componentString);
        Debug.Log("Spell cached under key: CachedSpell_"+key+" with string -> "+componentString);
        AuricaSpell match = Cast();
        try {
            BindingUIPanel.LocalInstance.SetBindText(key, match.c_name);
        } catch {
            Debug.Log("No spell found for binding...");
            BindingUIPanel.LocalInstance.SetBindText(key, "NONE");
        }
    }

    public void CacheSpell(string key, string spell) {
        Debug.Log("Caching spell: "+spell);
        if (cachedSpells.ContainsKey(key)) {
            cachedSpells[key] = new CachedSpell(spell);
        } else {
            cachedSpells.Add(key, new CachedSpell(spell));
        }
        PlayerPrefs.SetString("CachedSpell_"+key, spell);
        AuricaSpell match = CastSpellByName(spell);
        try {
            BindingUIPanel.LocalInstance.SetBindText(key, match.c_name);
        } catch {
            Debug.Log("No spell found for binding...");
            BindingUIPanel.LocalInstance.SetBindText(key, "NONE");
        }
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
