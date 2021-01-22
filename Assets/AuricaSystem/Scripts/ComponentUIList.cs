using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentUIList : MonoBehaviour
{
    public GameObject componentElementPrefab;
    public ComponentUIDisplay componentUIDisplay;
    public float startYPos = 500f, spaceBetweenElements = 50f, xOffset = 10f; 
    public bool isListOfAll = true;
    public List<AuricaSpellComponent> componentList;
    private AuricaSpellComponent[] allComponents;
    private float currentYPos;
    private RectTransform rect;
    private List<GameObject> instances;

    // Start is called before the first frame update
    void Start()
    {
        allComponents = Resources.LoadAll<AuricaSpellComponent>("AuricaSpellComponents");
        rect = GetComponent<RectTransform>();
        if (isListOfAll) componentList = new List<AuricaSpellComponent>(allComponents);
        instances = new List<GameObject>();
        PopulateList();
    }

    void PopulateList() {
        currentYPos = startYPos;
        rect.sizeDelta = new Vector2(150, 60 * componentList.Count);
        foreach (AuricaSpellComponent component in componentList) {
            GameObject newButton = Instantiate(componentElementPrefab, transform.position + (Vector3.up * currentYPos)+(Vector3.right * xOffset), transform.rotation, transform);
            newButton.GetComponent<ComponentUIButton>().SetComponent(component);
            newButton.GetComponent<ComponentUIButton>().componentDisplay = componentUIDisplay;
            instances.Add(newButton);
            currentYPos -= spaceBetweenElements;
        }
    }

    void WipeList() {
        currentYPos = startYPos;
        foreach (var item in instances){
            Destroy(item);
        }
    }

    public void AddComponent(AuricaSpellComponent newComponent) {
        componentList.Add(newComponent);
        WipeList();
        PopulateList();
    }
}
