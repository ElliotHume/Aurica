using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AdVd.GlyphRecognition;

public class ComponentUIList : MonoBehaviour
{
    public GameObject componentElementPrefab;
    public ComponentUIDisplay componentUIDisplay;
    public bool isListOfAll = true;
    public List<AuricaSpellComponent> componentList;
    public AudioSource ChangeComponentSound;

    private AuricaSpellComponent[] allComponents;
    private Glyph[] allComponentGlyphs;
    private float currentYPos;
    private RectTransform rect;
    private List<GameObject> instances = new List<GameObject>();
    private string sortMode = "alphabetic";

    // Start is called before the first frame update
    void Awake()
    {
        allComponents = Resources.LoadAll<AuricaSpellComponent>("AuricaSpellComponents");
        allComponentGlyphs = Resources.LoadAll<Glyph>("Glyphs");
        rect = GetComponent<RectTransform>();
        if (isListOfAll) {
            componentList = new List<AuricaSpellComponent>(allComponents);
            // componentList.Sort((a, b) => a.CompareTo(b));
        }
    }

    void Start() {
        PopulateList();
    }

    void Update () {
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            SelectNextComponent(true);
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            SelectNextComponent(false);
        }
    }

    public void PopulateList() {
        rect = GetComponent<RectTransform>();
        //rect.sizeDelta = new Vector2(1, 60 * componentList.Count);
        foreach (AuricaSpellComponent component in componentList) {
            GameObject newButton = Instantiate(componentElementPrefab, transform.position, transform.rotation, transform);
            instances.Add(newButton);
            newButton.GetComponent<ComponentUIButton>().SetComponent(component);
            newButton.GetComponent<ComponentUIButton>().componentDisplay = componentUIDisplay;
            newButton.GetComponent<ComponentUIButton>().SetGlyph(allComponentGlyphs, component.c_name);
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

    public void ResetList() {
        if (!isListOfAll) componentList.Clear();
    }

    public void AddComponent(AuricaSpellComponent newComponent) {
        componentList.Add(newComponent);
        WipeList();
        PopulateList();
    }

    public void ChangeOrdering(string orderType) {
        if (sortMode == orderType) return;
        sortMode = orderType;

        switch(orderType) {
            case "alphabetic":
                componentList = componentList.OrderBy((n) => n.name).ToList();
                break;
            case "category":
                componentList.Sort((a, b) => a.CompareTo(b));
                break;
        }
        WipeList();
        PopulateList();
    }

    void SelectNextComponent(bool traverseDownwards) {
        if (componentUIDisplay.component == null) return;

        int selectedIndex = componentList.IndexOf(componentUIDisplay.component);

        if (traverseDownwards) {
            if (selectedIndex == componentList.Count - 1) return;
            componentUIDisplay.UpdateComponent(componentList[selectedIndex+1]);
        } else {
            if (selectedIndex == 0) return;
            componentUIDisplay.UpdateComponent(componentList[selectedIndex-1]);
        }
        
        if (ChangeComponentSound != null) ChangeComponentSound.Play();
    }
}
