using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AdVd.GlyphRecognition;

[ExecuteAlways]
public class SpellWorldDisplay : MonoBehaviour {
    public static Glyph[] allGlyphs;
    public AuricaSpell spell;
    public Text Title, Description;
    public List<GlyphDisplay> displays;
    // Start is called before the first frame update

    void Awake() {
        if (SpellWorldDisplay.allGlyphs == null) {
            SpellWorldDisplay.allGlyphs = Resources.LoadAll<Glyph>("Glyphs");
        }
    }

    void Start() {
        Title.text = spell.c_name;
        Description.text = spell.description;

        for (int i = 0; i < spell.keyComponents.Count; i++) {
            foreach (Glyph item in SpellWorldDisplay.allGlyphs) {
                if (item.name == spell.keyComponents[i].c_name) {
                    // Debug.Log("Found glyph: "+item.name +"from: "+glyphName);
                    displays[i].glyph = item;
                    break;
                }
            }
        }
    }
}
