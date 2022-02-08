using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AdVd.GlyphRecognition;

[ExecuteAlways]
public class SpellDiscoveryPillar : MonoBehaviour {
    
    public AuricaSpell spell;

    public List<GlyphDisplay> glyphDisplays;
    public List<CapStretchStrokeGraphic> glyphGraphics;
    
    private AuricaSpell[] allSpells;
    private List<AuricaSpell> allSpellsList;
    private Glyph[] allComponentGlyphs;

    void Awake() {
        allSpells = Resources.LoadAll<AuricaSpell>("AuricaSpells");
        allSpellsList = new List<AuricaSpell>(allSpells);
        allComponentGlyphs = Resources.LoadAll<Glyph>("Glyphs");
    }

    void Start() {
        if (spell != null) DisplaySpell();
    }

    public void DisplaySpell() {
        List<AuricaSpellComponent> components = spell.keyComponents;
        for(int i=0; i < glyphDisplays.Count; i++) {
            if (i < components.Count) {
                foreach (Glyph item in allComponentGlyphs) {
                    if (item.name == components[i].c_name) {
                        glyphDisplays[i].glyph = item;
                        break;
                    }
                }
                if (ResourceManager.Instance != null) glyphGraphics[i].SetColor(ResourceManager.Instance.GetColor(spell.manaType));
            } else {
                glyphDisplays[i].glyph = null;
            }
        }
    }
}
