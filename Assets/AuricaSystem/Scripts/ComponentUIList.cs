using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentUIList : MonoBehaviour
{
    public GameObject componentElementPrefab;
    public ComponentUIDisplay componentUIDisplay;
    public float startYPos = 500f, spaceBetweenElements = 50f, xOffset = 10f; 
    private AuricaSpellComponent[] allComponents;
    private float currentYPos;
    private RectTransform rect;

    // Start is called before the first frame update
    void Start()
    {
        allComponents = Resources.LoadAll<AuricaSpellComponent>("AuricaSpellComponents");
        rect = GetComponent<RectTransform>();
        PopulateList();
    }

    void PopulateList() {
        currentYPos = startYPos;
        rect.sizeDelta = new Vector2(150, 60 * allComponents.Length);
        foreach (AuricaSpellComponent component in allComponents) {
            GameObject newButton = Instantiate(componentElementPrefab, transform.position + (Vector3.up * currentYPos)+(Vector3.right * xOffset), transform.rotation, transform);
            newButton.GetComponent<ComponentUIButton>().SetComponent(component);
            newButton.GetComponent<ComponentUIButton>().componentDisplay = componentUIDisplay;
            currentYPos -= spaceBetweenElements;
        }
    }
}
