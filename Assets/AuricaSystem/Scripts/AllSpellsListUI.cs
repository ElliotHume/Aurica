using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AdVd.GlyphRecognition;

public class AllSpellsListUI : MonoBehaviour
{
    public GameObject spellListPanelGameObject, spellElementPrefab;
    public SpellUIDisplay spellUIDisplay;
    private AuricaSpell[] allSpells;
    private List<AuricaSpell> allSpellsList;
    private float currentYPos;
    private RectTransform rect;
    private List<GameObject> instances = new List<GameObject>();
    private string sortMode = "alphabetic";

    // Start is called before the first frame update
    void Start()
    {
        allSpells = Resources.LoadAll<AuricaSpell>("AuricaSpells");
        allSpells = allSpells.Where((s) => s.keyComponents.Count != 0).ToArray();
        allSpellsList = new List<AuricaSpell>(allSpells);
        allSpellsList.Sort((a, b) => a.c_name.CompareTo(b.c_name));
        rect = GetComponent<RectTransform>();
        PopulateList();
        if (spellListPanelGameObject != null) spellListPanelGameObject.SetActive(false);
    }

    void Update () {
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            SelectNextComponent(true);
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            SelectNextComponent(false);
        }
    }

    public void PopulateList() {
        //currentYPos = startYPos;
        //rect.sizeDelta = new Vector2(150, 80 * allSpellsList.Count);
        foreach (AuricaSpell spell in allSpellsList) {
            GameObject newButton = Instantiate(spellElementPrefab, transform.position, transform.rotation, transform);
            instances.Add(newButton);
            newButton.GetComponent<SpellUIButton>().SetSpellDisplay(spellUIDisplay);
            newButton.GetComponent<SpellUIButton>().SetSpell(spell);
        }
    }

    public void WipeList() {
        // Debug.Log("Wiping list");
        if (instances.Count > 0) {
            foreach (var item in instances){
                Destroy(item);
            }
        }
    }

    public void ChangeOrdering(string orderType) {
        if (sortMode == orderType) return;
        sortMode = orderType;

        switch(orderType) {
            case "alphabetic":
                allSpellsList = allSpellsList.OrderBy((n) => n.c_name).ToList();
                break;
            case "spellStrength":
                allSpellsList = allSpellsList.OrderBy((spell) => 2f-AuricaCaster.LocalCaster.GetSpellStrengthForSpell(spell)).ToList();
                break;
        }
        WipeList();
        PopulateList();
    }

    void SelectNextComponent(bool traverseDownwards) {
        if (spellUIDisplay.spell == null) return;

        int selectedIndex = allSpellsList.IndexOf(spellUIDisplay.spell);

        if (traverseDownwards) {
            if (selectedIndex == allSpellsList.Count - 1) return;
            spellUIDisplay.PopulateFromSpell(allSpellsList[selectedIndex+1]);
        } else {
            if (selectedIndex == 0) return;
            spellUIDisplay.PopulateFromSpell(allSpellsList[selectedIndex-1]);
        }
        
        spellUIDisplay.ShowSpell();
    }
}
