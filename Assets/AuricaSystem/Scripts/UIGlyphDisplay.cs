using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AdVd.GlyphRecognition;
using UnityEngine.UI;

public class UIGlyphDisplay : MonoBehaviour {
    public GlyphDisplay glyphDisplay;
    public Text glyphName;

    public void SetGlyph(Glyph glyph, string name) {
        glyphDisplay.glyph = glyph;
        glyphName.text = name;
    }

    public void Clear() {
        glyphDisplay.glyph = null;
        glyphName.text = "";
    }
}
