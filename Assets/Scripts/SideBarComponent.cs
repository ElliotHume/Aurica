using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AdVd.GlyphRecognition;

[ExecuteInEditMode]
public class SideBarComponent : MonoBehaviour
{
    public Color color, outlineColor;
    public GlyphDisplay glyph;
    public CapStretchStrokeGraphic glyphGraphic;
    public Text text;
    public Image flashingOutline;

    public float outlineFadingSpeed = 1f;

    public bool outline = false;
    private bool fadingIn = true;
    private AuricaSpellComponent component;

    // Start is called before the first frame update
    void Start()
    {
        UpdateColor();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateColor();
        if (outline) {
            if (fadingIn) {
                flashingOutline.color = new Color(outlineColor.r, outlineColor.g, outlineColor.b, Mathf.Lerp(flashingOutline.color.a, 255f, Time.deltaTime * outlineFadingSpeed));
                if (flashingOutline.color.a >= 0.98f) {
                    fadingIn = false;
                }
            } else {
                // Fading out
                flashingOutline.color = new Color(outlineColor.r, outlineColor.g, outlineColor.b, Mathf.Lerp(flashingOutline.color.a, 0f, Time.deltaTime * outlineFadingSpeed));
                if (flashingOutline.color.a <= 0.02f) {
                    fadingIn = true;
                }
            }
        }
    }

    public void SetComponent(AuricaSpellComponent c, Glyph[] allGlyphs) {
        component = c;
        text.text = component.c_name;
        foreach (Glyph item in allGlyphs) {
            if (item.name == component.c_name) {
                glyph.glyph = item;
                break;
            }
        }  
    }

    public void SetColor(Color c) {
        color = c;
        UpdateColor();
    }

    void UpdateColor() {
        text.color = color;
        glyphGraphic.SetColor(color);
    }

    public void OutlinePulse(bool active) {
        outline = active;
        flashingOutline.color = outlineColor;
        fadingIn = true;
    }

    public void ActivateComponent(AuricaSpell spell) {
        OutlinePulse( true );
        if (!spell.keyComponents.Contains(component)) return;
        color = ResourceManager.Instance.GetColor(spell.manaType);
    }

    public void DeactivateComponent() {
        color = Color.white;
        OutlinePulse( false );
        gameObject.SetActive(false);
    }
}
