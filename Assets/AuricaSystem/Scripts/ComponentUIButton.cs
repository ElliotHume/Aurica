using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComponentUIButton : MonoBehaviour
{
    public Text title;
    public AuricaSpellComponent component;
    public ComponentUIDisplay componentDisplay;

    public void SetTitle(string newText){
        title.text = newText;
    }

    public void SetComponent(AuricaSpellComponent c) {
        component = c;
        SetTitle(c.c_name);
    }

    public void DisplayComponent(){
        if (componentDisplay != null) componentDisplay.UpdateComponent(component);
    }
}
