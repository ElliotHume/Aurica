using System.Collections;
using System.Collections.Generic;
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

    // Start is called before the first frame update
    void Start()
    {
        allSpells = Resources.LoadAll<AuricaSpell>("AuricaSpells");
        allSpellsList = new List<AuricaSpell>(allSpells);
        // allSpellsList.Sort((a, b) => a.manaType.CompareTo(b.manaType) == 0 ? a.c_name.CompareTo(b.c_name) : a.manaType.CompareTo(b.manaType));
        rect = GetComponent<RectTransform>();
        PopulateList();
        spellListPanelGameObject.SetActive(false);
    }

    public void PopulateList() {
        //currentYPos = startYPos;
        //rect.sizeDelta = new Vector2(150, 80 * allSpellsList.Count);
    foreach (AuricaSpell spell in allSpellsList) {
            if (spell.keyComponents.Count == 0) continue;
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
}
