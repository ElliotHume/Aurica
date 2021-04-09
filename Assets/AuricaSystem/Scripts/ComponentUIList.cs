using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AdVd.GlyphRecognition;

public class ComponentUIList : MonoBehaviour
{
    public GameObject componentElementPrefab;
    public ComponentUIDisplay componentUIDisplay;
    public float startYPos = 500f, spaceBetweenElements = 50f, xOffset = 10f; 
    public bool isListOfAll = true;
    public List<AuricaSpellComponent> componentList;
    private AuricaSpellComponent[] allComponents;
    private Glyph[] allComponentGlyphs;
    private float currentYPos;
    private RectTransform rect;
    private List<GameObject> instances = new List<GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        allComponents = Resources.LoadAll<AuricaSpellComponent>("AuricaSpellComponents");
        allComponentGlyphs = Resources.LoadAll<Glyph>("Glyphs");
        rect = GetComponent<RectTransform>();
        if (isListOfAll) {
            componentList = new List<AuricaSpellComponent>(allComponents);
            componentList.Sort((a, b) => a.CompareTo(b));;
        }
    }

    void Start() {
        PopulateList();
    }

    public void PopulateList() {
        rect = GetComponent<RectTransform>();
        currentYPos = startYPos;
        rect.sizeDelta = new Vector2(1, 60 * componentList.Count);
        foreach (AuricaSpellComponent component in componentList) {
            GameObject newButton = Instantiate(componentElementPrefab, transform.position + (Vector3.up * currentYPos)+(Vector3.right * xOffset), transform.rotation, transform);
            instances.Add(newButton);
            currentYPos -= spaceBetweenElements;
            newButton.GetComponent<ComponentUIButton>().SetComponent(component);
            newButton.GetComponent<ComponentUIButton>().componentDisplay = componentUIDisplay;
            newButton.GetComponent<ComponentUIButton>().SetGlyph(allComponentGlyphs, component.c_name);
            
        }
    }

    public void WipeList() {
        Debug.Log("Wiping list");
        currentYPos = startYPos;
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
}
