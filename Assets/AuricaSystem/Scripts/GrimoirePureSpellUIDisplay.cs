using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using AdVd.GlyphRecognition;
using TMPro;

public class GrimoirePureSpellUIDisplay : MonoBehaviour {
    public Text title, description, basisText, formText, focusText, actionText;
    public GameObject placeholder, displayPanel;
    public List<UIGlyphDisplay> basis, form, focus, action;
    public GameObject order, chaos, life, death, fire, water, earth, air, divine, demonic;

    public AuricaPureSpell spell;
    private bool isHidden = true;
    private Glyph[] allComponentGlyphs;
    private AuricaSpellComponent[] allSpellComponents;
    private List<UIGlyphDisplay> allGlyphDisplays; 

    // Start is called before the first frame update
    void Start() {
        HideSpell();
        allComponentGlyphs = Resources.LoadAll<Glyph>("Glyphs");
        allSpellComponents = Resources.LoadAll<AuricaSpellComponent>("AuricaSpellComponents");
        allGlyphDisplays = basis.Concat(form).Concat(focus).Concat(action).ToList();
    }

    public void PopulateFromSpell(AuricaPureSpell s) {
        if (isHidden) ShowSpell();

        spell = s;
        title.text = spell.c_name;
        description.text = spell.description;

        foreach(UIGlyphDisplay gd in allGlyphDisplays) {
            gd.Clear();
        }

        basisText.text = spell.SpellBasis.ToString();
        formText.text = spell.SpellForm.ToString();
        focusText.text = spell.SpellFocus.ToString();
        actionText.text = spell.SpellAction.ToString();

        order.SetActive(spell.OrderSpell != null);
        chaos.SetActive(spell.ChaosSpell != null);
        life.SetActive(spell.LifeSpell != null);
        death.SetActive(spell.DeathSpell != null);
        fire.SetActive(spell.FireSpell != null);
        water.SetActive(spell.WaterSpell != null);
        earth.SetActive(spell.EarthSpell != null);
        air.SetActive(spell.AirSpell != null);
        divine.SetActive(spell.DivineSpell != null);
        demonic.SetActive(spell.DemonicSpell != null);

        List<AuricaSpellComponent> basisComponents = allSpellComponents.Where((s) => s.classification == spell.SpellBasis).ToList();
        List<AuricaSpellComponent> formComponents = allSpellComponents.Where((s) => s.classification == spell.SpellForm).ToList();
        List<AuricaSpellComponent> focusComponents = allSpellComponents.Where((s) => s.classification == spell.SpellFocus).ToList();
        List<AuricaSpellComponent> actionComponents = allSpellComponents.Where((s) => s.classification == spell.SpellAction).ToList();

        int i = 0;
        foreach( var glyphDisplay in basis) {
            if (i > basisComponents.Count-1) continue;
            Glyph g = allComponentGlyphs.ToList().Find((f) => f.name == basisComponents[i].c_name);
            glyphDisplay.SetGlyph(g, g.name);
            i++;
        }
        i = 0;
        foreach( var glyphDisplay in form) {
            if (i > formComponents.Count-1) continue;
            Glyph g = allComponentGlyphs.ToList().Find((f) => f.name == formComponents[i].c_name);
            glyphDisplay.SetGlyph(g, g.name);
            i++;
        }
        i = 0;
        foreach( var glyphDisplay in focus) {
            if (i > focusComponents.Count-1) continue;
            Glyph g = allComponentGlyphs.ToList().Find((f) => f.name == focusComponents[i].c_name);
            glyphDisplay.SetGlyph(g, g.name);
            i++;
        }
        i = 0;
        foreach( var glyphDisplay in action) {
            if (i > actionComponents.Count-1) continue;
            Glyph g = allComponentGlyphs.ToList().Find((f) => f.name == actionComponents[i].c_name);
            glyphDisplay.SetGlyph(g, g.name);
            i++;
        }

    }

    public void ShowSpell() {
        displayPanel.SetActive(true);
        placeholder.SetActive(false);
        isHidden = false;
    }

    public void HideSpell() {
        displayPanel.SetActive(false);
        placeholder.SetActive(true);
        isHidden = true;
    }
}
