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
    private bool displayed = false;

    void Awake() {
        allSpells = Resources.LoadAll<AuricaSpell>("AuricaSpells");
        allSpellsList = new List<AuricaSpell>(allSpells);
        allComponentGlyphs = Resources.LoadAll<Glyph>("Glyphs");
    }

    void FixedUpdate() {
        if (!displayed) {
            if (DiscoveryManager.Instance.HasFetched()) {
                DisplaySpell();
                displayed = true;
            }
        }
    }

    public void DisplaySpell() {
        if (spell == null) return;
        List<AuricaSpellComponent> components = spell.keyComponents;
        for(int i=0; i < glyphDisplays.Count; i++) {
            if (glyphDisplays[i] == null || !glyphDisplays[i].gameObject.activeInHierarchy) continue;
            if (i < components.Count) {
                foreach (Glyph item in allComponentGlyphs) {
                    if (item.name == components[i].c_name) {
                        glyphDisplays[i].glyph = item;
                        break;
                    }
                }
                // If you already know the spell, turn the glyphs black
                if (DiscoveryManager.Instance != null && DiscoveryManager.Instance.IsSpellDiscovered(spell)) glyphGraphics[i].SetColor(Color.black);
            } else {
                glyphDisplays[i].glyph = null;
            }
        }
    }
}
