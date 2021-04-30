using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AdVd.GlyphRecognition;

public class ComponentUIPanel : MonoBehaviour
{
    public List<SideBarComponent> ComponentPanels;
    public Text spellText;
    private Glyph[] allGlyphs;
    private int currentIndex = 0, maxComponents;
    private AuricaSpell spell;


    // Start is called before the first frame update
    void Start() {
        allGlyphs = Resources.LoadAll<Glyph>("Glyphs");
        HideAllComponents();
        maxComponents = ComponentPanels.Count;
    }

    public void HideAllComponents() {
        foreach (SideBarComponent item in ComponentPanels) {
            item.DeactivateComponent();
        }
        spellText.gameObject.SetActive(false);
        currentIndex = 0;
    }

    public void AddComponent(AuricaSpellComponent component) {
        if (currentIndex >= maxComponents) {
            HideAllComponents();
        }
        ComponentPanels[currentIndex].gameObject.SetActive(true);
        ComponentPanels[currentIndex].SetComponent(component, allGlyphs);
        currentIndex += 1;

        spell = AuricaCaster.LocalCaster.Cast();
        if (spell != null && spell.c_name != null) {
            spellText.gameObject.SetActive(true);
            spellText.text = spell.c_name.ToUpper();

            foreach (SideBarComponent item in ComponentPanels) {
                item.ActivateComponent(spell);
            }
        }
    }
}
