using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using AdVd.GlyphRecognition;

public class SpellCraftingMatrix : MonoBehaviour {

    public List<GlyphDisplay> glyphDisplays;
    public List<CapStretchStrokeGraphic> glyphGraphics;
    
    private AuricaSpell[] allSpells;
    private List<AuricaSpell> allSpellsList;
    private AuricaCaster caster;
    private Glyph[] allComponentGlyphs;

    // Start is called before the first frame update
    void Start() {
        caster = AuricaCaster.LocalCaster;
        allSpells = Resources.LoadAll<AuricaSpell>("AuricaSpells");
        allSpellsList = new List<AuricaSpell>(allSpells);
        allComponentGlyphs = Resources.LoadAll<Glyph>("Glyphs");
    }

    void OnEnable() {
        CheckComponents();
    }

    public void CheckComponents() {
        if (!gameObject.activeInHierarchy) return;
        if (caster == null) caster = AuricaCaster.LocalCaster;
        List<AuricaSpellComponent> components = caster.GetCurrentComponents();
        if (components.Count == 0) {
            for(int i=0; i < 8; i++) {
                glyphDisplays[i].glyph = null;
                glyphGraphics[i].SetColor(Color.white);
            }
            return;
        }
        for(int i=0; i < Mathf.Min(components.Count, 8); i++) {
            foreach (Glyph item in allComponentGlyphs) {
                if (item.name == components[i].c_name) {
                    glyphDisplays[i].glyph = item;
                    break;
                }
            } 
        }
        for(int i=0; i < Mathf.Min(components.Count, 8); i++) {
            bool found = false;
            foreach(AuricaSpell spell in allSpellsList) {
                if (spell.keyComponents.Contains(components[i])) {
                    found = true;
                }
            }
            if (!found) {
                glyphGraphics[i].SetColor(Color.white);
                continue;
            }
            
            bool validCombination = false;
            foreach(AuricaSpell spell in allSpells) {
                if (allSpells.Where((s) => {
                    bool validSpell = true;
                    foreach(AuricaSpellComponent c in components){
                        if (!s.keyComponents.Contains(c)) {
                            validSpell = false;
                            break;
                        }
                    }
                    return validSpell;
                }).ToArray().Length > 0) {
                    validCombination = true;
                }
            }
            if (validCombination) {
                glyphGraphics[i].SetColor(Color.green);
            } else {
                glyphGraphics[i].SetColor(Color.yellow);
            }
        }
    }

}
