using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AdVd.GlyphRecognition;
using TMPro;

public class GrimoireSpellsListUI : MonoBehaviour
{
    public GameObject grimoirePanel, spellElementPrefab;
    public GrimoireSpellUIDisplay spellUIDisplay;
    public TMP_Text manaTitleText;

    public bool restricted = true;

    private AuricaSpell[] allSpells;
    private List<AuricaSpell> allSpellsList;
    private List<AuricaSpell> currentSpellsList;

    private List<AuricaSpell> auricSpellsList;
    private List<AuricaSpell> orderSpellsList;
    private List<AuricaSpell> chaosSpellsList;
    private List<AuricaSpell> lifeSpellsList;
    private List<AuricaSpell> deathSpellsList;
    private List<AuricaSpell> fireSpellsList;
    private List<AuricaSpell> waterSpellsList;
    private List<AuricaSpell> earthSpellsList;
    private List<AuricaSpell> airSpellsList;
    private List<AuricaSpell> divineSpellsList;
    private List<AuricaSpell> demonicSpellsList;

    private List<GameObject> instances = new List<GameObject>();
    private AuricaSpell.ManaType CurrentManaType = 0;

    // Start is called before the first frame update
    void Start() {
        allSpells = Resources.LoadAll<AuricaSpell>("AuricaSpells");
        allSpells = allSpells.Where((s) => s.keyComponents.Count != 0).ToArray();
        allSpellsList = new List<AuricaSpell>(allSpells);
        allSpellsList.Sort((a, b) => a.c_name.CompareTo(b.c_name));

        auricSpellsList = new List<AuricaSpell>(allSpells.Where((s) => s.manaType == AuricaSpell.ManaType.Auric).ToArray());
        allSpellsList.Sort((a, b) => a.c_name.CompareTo(b.c_name));
        
        orderSpellsList = new List<AuricaSpell>(allSpells.Where((s) => s.manaType == AuricaSpell.ManaType.Order).ToArray());
        orderSpellsList.Sort((a, b) => a.c_name.CompareTo(b.c_name));
        
        chaosSpellsList = new List<AuricaSpell>(allSpells.Where((s) => s.manaType == AuricaSpell.ManaType.Chaos).ToArray());
        chaosSpellsList.Sort((a, b) => a.c_name.CompareTo(b.c_name));
        
        lifeSpellsList = new List<AuricaSpell>(allSpells.Where((s) => s.manaType == AuricaSpell.ManaType.Life).ToArray());
        lifeSpellsList.Sort((a, b) => a.c_name.CompareTo(b.c_name));
        
        deathSpellsList = new List<AuricaSpell>(allSpells.Where((s) => s.manaType == AuricaSpell.ManaType.Death).ToArray());
        deathSpellsList.Sort((a, b) => a.c_name.CompareTo(b.c_name));
        
        fireSpellsList = new List<AuricaSpell>(allSpells.Where((s) => s.manaType == AuricaSpell.ManaType.Fire).ToArray());
        fireSpellsList.Sort((a, b) => a.c_name.CompareTo(b.c_name));
        
        waterSpellsList = new List<AuricaSpell>(allSpells.Where((s) => s.manaType == AuricaSpell.ManaType.Water).ToArray());
        waterSpellsList.Sort((a, b) => a.c_name.CompareTo(b.c_name));
        
        earthSpellsList = new List<AuricaSpell>(allSpells.Where((s) => s.manaType == AuricaSpell.ManaType.Earth).ToArray());
        earthSpellsList.Sort((a, b) => a.c_name.CompareTo(b.c_name));
        
        airSpellsList = new List<AuricaSpell>(allSpells.Where((s) => s.manaType == AuricaSpell.ManaType.Air).ToArray());
        airSpellsList.Sort((a, b) => a.c_name.CompareTo(b.c_name));
        
        divineSpellsList = new List<AuricaSpell>(allSpells.Where((s) => s.manaType == AuricaSpell.ManaType.Divine).ToArray());
        divineSpellsList.Sort((a, b) => a.c_name.CompareTo(b.c_name));
        
        demonicSpellsList = new List<AuricaSpell>(allSpells.Where((s) => s.manaType == AuricaSpell.ManaType.Demonic).ToArray());
        demonicSpellsList.Sort((a, b) => a.c_name.CompareTo(b.c_name));
        

        currentSpellsList = auricSpellsList;

        PopulateList();
        grimoirePanel.SetActive(false);
    }

    public void SetManaType(int newManaType) { 
        CurrentManaType = (AuricaSpell.ManaType)newManaType;

        switch(CurrentManaType) {
            case AuricaSpell.ManaType.Auric:
                currentSpellsList = auricSpellsList;
                manaTitleText.text = "Auric Spells";
                break;
            case AuricaSpell.ManaType.Order:
                currentSpellsList = orderSpellsList;
                manaTitleText.text = "Order Spells";
                break;
            case AuricaSpell.ManaType.Chaos:
                currentSpellsList = chaosSpellsList;
                manaTitleText.text = "Chaos Spells";
                break;
            case AuricaSpell.ManaType.Life:
                currentSpellsList = lifeSpellsList;
                manaTitleText.text = "Life Spells";
                break;
            case AuricaSpell.ManaType.Death:
                currentSpellsList = deathSpellsList;
                manaTitleText.text = "Death Spells";
                break;
            case AuricaSpell.ManaType.Fire:
                currentSpellsList = fireSpellsList;
                manaTitleText.text = "Fire Spells";
                break;
            case AuricaSpell.ManaType.Water:
                currentSpellsList = waterSpellsList;
                manaTitleText.text = "Water Spells";
                break;
            case AuricaSpell.ManaType.Earth:
                currentSpellsList = earthSpellsList;
                manaTitleText.text = "Earth Spells";
                break;
            case AuricaSpell.ManaType.Air:
                currentSpellsList = airSpellsList;
                manaTitleText.text = "Air Spells";
                break;
            case AuricaSpell.ManaType.Divine:
                currentSpellsList = divineSpellsList;
                manaTitleText.text = "Divine Spells";
                break;
            case AuricaSpell.ManaType.Demonic:
                currentSpellsList = demonicSpellsList;
                manaTitleText.text = "Demonic Spells";
                break;
        }

        WipeList();
        PopulateList();
    }

    public void PopulateList() {
        List<AuricaSpell> discoveredSpells = DiscoveryManager.Instance.GetDiscoveredSpells();
        foreach (AuricaSpell spell in currentSpellsList) {
            if (restricted && !discoveredSpells.Contains(spell)) continue;
            GameObject newButton = Instantiate(spellElementPrefab, transform.position, transform.rotation, transform);
            instances.Add(newButton);
            newButton.GetComponent<GrimoireSpellUIButton>().SetSpellDisplay(spellUIDisplay);
            newButton.GetComponent<GrimoireSpellUIButton>().SetSpell(spell);
        }
    }

    public void WipeList() {
        if (instances.Count > 0) {
            foreach (var item in instances){
                Destroy(item);
            }
        }
    }
}
