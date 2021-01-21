using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComponentUIPanel : MonoBehaviour
{
    public List<GameObject> ComponentPanels;
    private int currentIndex = 0, maxComponents;

    // Start is called before the first frame update
    void Start() {
        HideAllComponents();
        maxComponents = ComponentPanels.Count;
    }

    public void HideAllComponents() {
        foreach (GameObject item in ComponentPanels) {
            item.SetActive(false);
        }
        currentIndex = 0;
    }

    public void AddComponent(AuricaSpellComponent component) {
        if (currentIndex >= maxComponents) {
            HideAllComponents();
        }
        ComponentPanels[currentIndex].transform.Find("Text").GetComponent<Text>().text = component.c_name;
        ComponentPanels[currentIndex].SetActive(true);
        currentIndex += 1;
    }
}
