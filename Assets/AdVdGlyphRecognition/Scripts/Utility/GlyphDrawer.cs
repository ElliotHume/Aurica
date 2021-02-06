using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AdVd.GlyphRecognition;

/// <summary>
/// Utility monobehaviour to draw glyphs and strokes. The user may re-implement this class
/// in order to draw the strokes in a custom way.
/// </summary>
public class GlyphDrawer : MonoBehaviour {

    public GlyphDrawInput glyphInput;

	public StrokeGraphic targetGlyphGraphic, castedGlyphGraphic, currentGlyphGraphic, currentStrokeGraphic;

	void Start () {
        glyphInput.OnGlyphCast.AddListener(this.OnGlyphCast);

		if (glyphInput.OnStrokeDraw!=this.OnStrokeDraw) glyphInput.OnStrokeDraw+=this.OnStrokeDraw;
		if (glyphInput.OnPointDraw!=this.OnPointDraw) glyphInput.OnPointDraw+=this.OnPointDraw;
	}
	
	void Set(StrokeGraphic strokeGraphic, Glyph glyph)
    {
		if (strokeGraphic != null) strokeGraphic.SetStrokes(glyph);
	}
	void Set(StrokeGraphic strokeGraphic, Stroke[] strokes)
    {
		if (strokeGraphic != null) strokeGraphic.SetStrokes(strokes);
	}
	void Clear(StrokeGraphic strokeGraphic)
    {
		if (strokeGraphic != null) strokeGraphic.ClearStrokes();
	}
	bool IsClear(StrokeGraphic strokeGraphic)
    {
		return strokeGraphic == null || strokeGraphic.IsClear;
	}


	void OnGlyphCast(int index, GlyphMatch match){
		if (match!=null){
			StartCoroutine(Morph (match));
		}
		else{
			Clear(targetGlyphGraphic);
			Clear(castedGlyphGraphic);
		}
		Clear(currentGlyphGraphic);
	}

	const float step=0.04f;

	IEnumerator Morph(GlyphMatch match){
		Clear(castedGlyphGraphic);
		Stroke[] strokes = null;
		for (float t=0;t<1;t+=step){
			match.SetLerpStrokes(t, ref strokes);
			Set(targetGlyphGraphic,strokes);
			yield return new WaitForSeconds(step);
		}
		Set(targetGlyphGraphic,match.target);
		if (IsClear(currentStrokeGraphic) && IsClear(currentGlyphGraphic)){
			Set(castedGlyphGraphic,match.source);
		}
	}
	
	void OnStrokeDraw(Stroke[] strokes){
		Clear(currentStrokeGraphic);
		if (strokes!=null) Set(currentGlyphGraphic,strokes);
		else Clear(currentGlyphGraphic);
	}
	
	void OnPointDraw(Vector2[] points){
		Clear(castedGlyphGraphic);
		if (points!=null) Set(currentStrokeGraphic,new Stroke[]{ new Stroke(points) });
		else Clear(currentStrokeGraphic);
	}
}

