using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AdVd.GlyphRecognition;

public class ComponentUIButton : MonoBehaviour
{
    public Text title;
    public AuricaSpellComponent component;
    public ComponentUIDisplay componentDisplay;
    public GlyphDisplay glyphDisplay;

    public void SetTitle(string newText){
        title.text = newText;
    }

    public void SetComponent(AuricaSpellComponent c) {
        component = c;
        SetTitle(c.c_name);
    }

    public void SetGlyph(Glyph[] allGlyphs, string glyphName) {
        if (glyphDisplay == null) return;
        foreach (Glyph item in allGlyphs) {
            if (item.name == glyphName) {
                // Debug.Log("Found glyph: "+item.name +"from: "+glyphName);
                glyphDisplay.glyph = item;
                break;
            }
        }  
    }

    public void DisplayComponent(){
        if (componentDisplay != null) componentDisplay.UpdateComponent(component);
    }
}
