using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;

public class AuricaCaster : MonoBehaviourPun {

    public Aura aura;
    public static AuricaCaster LocalCaster;
    public Dictionary<string, CachedSpell> cachedSpells;
    public Dictionary<string, float> cachedSpellManas = new Dictionary<string, float>();

    [HideInInspector]
    public bool spellManasCached = false;

    // Lists of all components and spells
    private AuricaSpellComponent[] allComponents;
    private AuricaSpell[] allSpells;
    private AuricaPureSpell[] allPureSpells;

    // Runtime variables
    private List<AuricaSpell> discoveredSpells;
    private List<AuricaSpellComponent> currentComponents;
    private ManaDistribution currentDistribution;
    private float currentManaCost, spellStrength;
    private AuricaSpell currentSpellMatch;
    private ComponentUIPanel cpUI;
    private DistributionUIDisplay distDisplay;

    // Start is called before the first frame update
    void Start() {
        if (aura == null) aura = GetComponent<Aura>();

        if (PlayerPrefs.HasKey("CachedSpell_e")) {
            cachedSpells.Add("e", new CachedSpell(PlayerPrefs.GetString("CachedSpell_e")));
        } else {
            CacheSpell("e", "protect, self");
        }

        if (PlayerPrefs.HasKey("CachedSpell_q")) {
            cachedSpells.Add("q", new CachedSpell(PlayerPrefs.GetString("CachedSpell_q")));
        } else {
            CacheSpell("q", "self, protect, ordo");
        }

        if (PlayerPrefs.HasKey("CachedSpell_1")) {
            cachedSpells.Add("1", new CachedSpell(PlayerPrefs.GetString("CachedSpell_1")));
        } else {
            CacheSpell("1", "mana, purify, self");
        }

        if (PlayerPrefs.HasKey("CachedSpell_2")) {
            cachedSpells.Add("2", new CachedSpell(PlayerPrefs.GetString("CachedSpell_2")));
        } else {
            CacheSpell("2", "mana, bolt");
        }

        if (PlayerPrefs.HasKey("CachedSpell_3")) {
            cachedSpells.Add("3", new CachedSpell(PlayerPrefs.GetString("CachedSpell_3")));
        } else {
            CacheSpell("3", "attack, bolt, other, propel");
        }

        if (PlayerPrefs.HasKey("CachedSpell_4")) {
            cachedSpells.Add("4", new CachedSpell(PlayerPrefs.GetString("CachedSpell_4")));
        } else {
            CacheSpell("4", "throw, self");
        }

        if (PlayerPrefs.HasKey("CachedSpell_r")) {
            cachedSpells.Add("r", new CachedSpell(PlayerPrefs.GetString("CachedSpell_r")));
        } else {
            CacheSpell("r", "throw, infernum, expel");
        }


        if (PlayerPrefs.HasKey("CachedSpell_f")) {
            cachedSpells.Add("f", new CachedSpell(PlayerPrefs.GetString("CachedSpell_f")));
        } else {
            CacheSpell("f", "expel, mana, purify, self");
        }



        // GAME SPECIFIC
        cpUI = GameObject.Find("ComponentPanel").GetComponent<ComponentUIPanel>();
        try {
            distDisplay = GameObject.Find("LocalDistributionDisplay").GetComponent<DistributionUIDisplay>();
        } catch {
            // do nothing
        }
    }

    void Awake() {
        if (photonView.IsMine) AuricaCaster.LocalCaster = this;
        allComponents = Resources.LoadAll<AuricaSpellComponent>("AuricaSpellComponents");
        allSpells = Resources.LoadAll<AuricaSpell>("AuricaSpells");
        allPureSpells = Resources.LoadAll<AuricaPureSpell>("AuricaPureSpells");
        currentComponents = new List<AuricaSpellComponent>();
        currentDistribution = new ManaDistribution();
        cachedSpells = new Dictionary<string, CachedSpell>();
        discoveredSpells = new List<AuricaSpell>();
    }

    public void RetrieveDiscoveredSpells() {
        Debug.Log("Aurica Caster: Retrieved discovered spells.");
        discoveredSpells = DiscoveryManager.Instance.GetDiscoveredSpells();
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
        currentComponents.Add(newComponent);
        currentManaCost += newComponent.GetManaCost(aura.GetAura());
        ManaDistribution oldMd = currentDistribution;
        currentDistribution = newComponent.CalculateDistributionChange(currentDistribution, aura.GetAura());

        // Debug.Log("Added component: " + newComponent.c_name + "    Current Mana Cost: " + currentManaCost);
        // Debug.Log("Old Distribution: " + oldMd.ToString() + "    New Distribution: " + currentDistribution.ToString()+"         Change: "+(currentDistribution-oldMd).ToString());

        if (distDisplay != null) distDisplay.SetDistribution(currentDistribution);
        if (cpUI != null) cpUI.AddComponent(newComponent);
    }

    public void RemoveLastComponent() {
        if (currentComponents.Count <= 0) {
            TipWindow.Instance.ShowTip("Cannot Remove Component", "There are no more components left to remove.", 1f);
            return;
        }
        AuricaSpellComponent component = currentComponents[currentComponents.Count - 1];
        if (component.hasFluxDistribution || component.hasSiphonDistribution) {
            TipWindow.Instance.ShowTip("Cannot Remove Component", "The component: "+component.c_name+" cannot be removed since it has either a flux or siphon mana distribution.", 2f);
            return;
        }
        currentManaCost -= component.GetManaCost(aura.GetAura());
        currentDistribution = component.RemoveDistribution(currentDistribution, aura.GetAura());
        currentComponents.Remove(component);
        Debug.Log("REMOVED COMPONENT components left: "+currentComponents.Count);
        if (distDisplay != null) distDisplay.SetDistribution(currentDistribution);
        if (cpUI != null) cpUI.RemoveComponent(component);
        if (TipWindow.Instance != null) TipWindow.Instance.ShowTip("Removed Component", "Removed component: "+component.c_name, 1f);
    }

    public bool CanRemoveLastComponent() {
        AuricaSpellComponent component = currentComponents[currentComponents.Count - 1];
        return !(component.hasFluxDistribution || component.hasSiphonDistribution);
    }

    public AuricaSpell CastSpellByName(string componentsByName) {
        if (componentsByName == null || componentsByName == "") return null;
        ResetCast();
        string[] componentSeperator = new string[] { ", " };
        string[] splitComponents = componentsByName.Split(componentSeperator, System.StringSplitOptions.None);
        foreach (string item in splitComponents) {
            AddComponent(item);
        }
        return Cast();
    }

    public AuricaSpell CastSpellByObject(AuricaSpell spell) {
        ResetCast();
        if (spell.keyComponents.Count == 0) return null;
        foreach (AuricaSpellComponent component in spell.keyComponents) {
            AddComponent(component);
        }
        return Cast();
    }

    public float GetSpellStrengthForSpell(AuricaSpell spell) {
        if (spell.keyComponents.Count == 0) return 0f;
        ResetCast();
        foreach( AuricaSpellComponent component in spell.keyComponents) {
            AddComponent(component);
        }
        GetSpellMatch(currentComponents, currentDistribution);
        ResetCast();
        return spellStrength;
    }

    public AuricaSpell Cast() {
        AuricaPureSpell pureSpell = GetPureMagicSpellMatch(currentComponents, currentDistribution);
        AuricaSpell spell = pureSpell == null ? GetSpellMatch(currentComponents, currentDistribution) :  pureSpell.GetAuricaSpell(pureSpell.GetManaType(currentDistribution));
        if (spell != null) {
            if (pureSpell != null) currentManaCost += pureSpell.addedManaCost;
            return spell;
        }
        return null;
    }

    public AuricaSpell CastFinal() {
        AuricaPureSpell pureSpell = GetPureMagicSpellMatch(currentComponents, currentDistribution);
        AuricaSpell spell = pureSpell == null ? GetSpellMatch(currentComponents, currentDistribution) : pureSpell.GetAuricaSpell(pureSpell.GetManaType(currentDistribution));
        if (spell != null) {
            if (pureSpell != null) currentManaCost += pureSpell.addedManaCost;
            if (discoveredSpells.Count > 0 && !discoveredSpells.Contains(spell) && (!spell.isMasterySpell || MasteryManager.Instance.HasMasteryForSpell(spell))) {
                DiscoveryManager.Instance.Discover(spell);
            }
            return spell;
        }
        return null;
    }

    public AuricaSpell GetSpellMatch(List<AuricaSpellComponent> components, ManaDistribution distribution) {
        int bestMatchCorrectComponents = 0;
        AuricaSpell spellMatch = null;
        foreach (AuricaSpell s in allSpells) {
            // Debug.Log("Check Spell: " + s.c_name + "   IsMatch: " + s.CheckComponents(components) + "     Error:  " + s.GetError(distribution)+"  Num matching components: "+s.GetNumberOfMatchingComponents(components));
            if (s.CheckComponents(components) && s.GetNumberOfMatchingComponents(components) > bestMatchCorrectComponents) {
                spellMatch = s;
                bestMatchCorrectComponents = s.GetNumberOfMatchingComponents(components);
                spellStrength = (spellMatch.errorThreshold - s.GetError(distribution)) / spellMatch.errorThreshold + 0.5f;
                if (spellStrength < 0.5f) spellStrength = 0.5f;
            }
        }

        currentSpellMatch = spellMatch == null ? null : spellMatch;
        return currentSpellMatch;
    }

    public AuricaPureSpell GetPureMagicSpellMatch(List<AuricaSpellComponent> components, ManaDistribution distribution) {
        AuricaPureSpell spellMatch = null;
        foreach (AuricaPureSpell s in allPureSpells) {
            // Debug.Log("Check Pure Spell: " + s.c_name + "   IsMatch: " + s.CheckComponents(components) + "     Error:  " + s.GetError(s.GetManaType(distribution), distribution));
            if (s.CheckComponents(components, distribution)) {
                spellMatch = s;
                spellStrength = (spellMatch.errorThreshold - s.GetError(s.GetManaType(distribution), distribution)) / spellMatch.errorThreshold + 0.5f;
                if (spellStrength < 0.5f) spellStrength = 0.5f;
                // Debug.Log("Pure match: "+s.c_name+"   mana type:"+s.GetManaType(distribution)+"  error:"+s.GetError(s.GetManaType(distribution), distribution));
            }
        }
        currentSpellMatch = spellMatch == null ? null : spellMatch.GetAuricaSpell(spellMatch.GetManaType(currentDistribution));
        return spellMatch == null ? null : spellMatch;
    }

    public void ResetCast() {
        // Debug.Log("Resetting Aurica Cast");
        currentComponents.Clear();
        currentManaCost = 0f;
        currentDistribution = new ManaDistribution();
        if (cpUI != null) cpUI.HideAllComponents();
        if (distDisplay != null) distDisplay.SetDistribution(currentDistribution);
    }

    public AuricaSpell CastBindSlot(string slot) {
        if (cachedSpells.ContainsKey(slot)) {
            ResetCast();
            CachedSpell cachedSpell = cachedSpells[slot];
            cachedSpell.AddComponents(this);
            return CastFinal();
        }

        return GetSpellMatch(new List<AuricaSpellComponent>(), new ManaDistribution());
    }

    public void CacheCurrentSpell(string k) {
        string key = k.ToLower();
        string componentString = "";
        foreach (AuricaSpellComponent c in currentComponents) {
            componentString += c.c_name + ", ";
        }
        componentString = componentString.Substring(0, componentString.Length - 2);
        // Debug.Log("Caching spell: " + componentString);

        CachedSpell cs = new CachedSpell(componentString);
        if (cachedSpells.ContainsKey(key)) {
            cachedSpells[key] = cs;
        } else {
            cachedSpells.Add(key, cs);
        }

        PlayerPrefs.SetString("CachedSpell_" + key, componentString);
        Debug.Log("Spell cached under key: CachedSpell_" + key + " with string -> " + componentString);
        AuricaSpell match = Cast();


        // GAME SPECIFIC
        try {
            BindingUIPanel.LocalInstance.SetBind(key, match);
        } catch {
            Debug.Log("No spell found for binding...");
            BindingUIPanel.LocalInstance.SetBind(key, null);
        }

        if (cachedSpellManas.ContainsKey(key)) {
            cachedSpellManas[key] = GetManaCost();
        } else {
            cachedSpellManas.Add(key, GetManaCost());
        }
    }

    public void CacheSpell(string k, string spell) {
        string key = k.ToLower();
        Debug.Log("Caching key "+key+" with spell: " + spell);
        CachedSpell cs = new CachedSpell(spell);
        if (cachedSpells.ContainsKey(key)) {
            cachedSpells[key] = cs;
        } else {
            cachedSpells.Add(key, cs);
        }

        if (cachedSpellManas.ContainsKey(key)) {
            cachedSpellManas[key] = cs.CalculateManaCost(this);
        } else {
            cachedSpellManas.Add(key, cs.CalculateManaCost(this));
        }
        PlayerPrefs.SetString("CachedSpell_" + key, spell);
        AuricaSpell match = CastSpellByName(spell);

        // DISCOVER SPELL
        if (match != null && discoveredSpells.Count > 0 && !discoveredSpells.Contains(match) && (!match.isMasterySpell || MasteryManager.Instance.HasMasteryForSpell(match))) {
            DiscoveryManager.Instance.Discover(match);
        }
        
        // GAME SPECIFIC
        try {
            BindingUIPanel.LocalInstance.SetBind(key, match);
        } catch {
            Debug.Log("No spell found for binding...");
            BindingUIPanel.LocalInstance.SetBind(key, null);
        }
    }

    public float GetManaCost() {
        if (currentSpellMatch != null) {
            return (currentSpellMatch.baseManaCost + (currentSpellMatch.componentManaMultiplier * currentManaCost)) * GameManager.GLOBAL_MANA_COST_MULTIPLIER;
        }
        return currentManaCost * GameManager.GLOBAL_MANA_COST_MULTIPLIER;
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
    

    public void CacheSpellManas() {
        cachedSpellManas.Clear();
        foreach(string key in cachedSpells.Keys) {
            cachedSpellManas.Add(key, cachedSpells[key].CalculateManaCost(this));
        }
        spellManasCached = true;
    }

    public bool CanCastCachedSpell(string key, float availableMana) {
        if (!cachedSpellManas.ContainsKey(key)) CacheSpellManas();
        return cachedSpellManas[key] <= availableMana;
    }

}
