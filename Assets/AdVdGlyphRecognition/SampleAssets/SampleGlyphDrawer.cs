using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AdVd.GlyphRecognition;

public class SampleGlyphDrawer : MonoBehaviour {

	public GlyphDrawInput glyphInput;
	public StrokeGraphic targetGlyphGraphic, currentGlyphGraphic, currentStrokeGraphic;
	public Material drawMaterial, morphMaterial;
	public AnimationCurve morph;
	public float morphDuration=1f, fadeOutDuration=0.5f;
	void Start () {
		glyphInput.OnGlyphCast.AddListener(this.OnGlyphCast);

		if (glyphInput.OnStrokeDraw!=this.OnStrokeDraw) glyphInput.OnStrokeDraw+=this.OnStrokeDraw;
		if (glyphInput.OnPointDraw!=this.OnPointDraw) glyphInput.OnPointDraw+=this.OnPointDraw;

        drawMaterial = Instantiate(drawMaterial) as Material;
        morphMaterial = Instantiate(morphMaterial) as Material;
        targetGlyphGraphic.material=morphMaterial;
		currentGlyphGraphic.material=drawMaterial;
		currentStrokeGraphic.material=drawMaterial;
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

	public void OnGlyphCast(int index, GlyphMatch match){
		if (match!=null && match.Cost<match.Threshold){
			StartCoroutine(Morph (match));
			Clear(currentGlyphGraphic);
		}
		else{
			Clear(targetGlyphGraphic);
			StartCoroutine(FadeOut(currentGlyphGraphic, Color.red));
		}
	}

    const float step = 0.05f;

	IEnumerator Morph(GlyphMatch match){
		Stroke[] strokes = null;
		for (float time=0;time<morphDuration;time+=step){
			float progress=time/morphDuration;
			float t=morph.Evaluate(progress);
			match.SetLerpStrokes(t, ref strokes);
			foreach(Stroke s in strokes) { 
				float size=Mathf.Lerp(Mathf.Max(drawingBounds.height,drawingBounds.width),1f,t);
				s.Scale(new Vector2(size,size)); 
				s.Translate(Vector2.Lerp(drawingBounds.center,Vector2.zero,t)); 
			}
			Set(targetGlyphGraphic,strokes);
			yield return new WaitForSeconds(step);
		}
		Set(targetGlyphGraphic,match.target);
        StartCoroutine(FadeOut(targetGlyphGraphic, Color.white));
	}

	IEnumerator FadeOut(StrokeGraphic strokeGraphic, Color targetColor){
        targetColor.a = 0f;
        if (strokeGraphic!=null) strokeGraphic.CrossFadeColor(targetColor, fadeOutDuration, true, true);
        yield return new WaitForSeconds(fadeOutDuration*1.5f);//The order of crossfade finalization and the end of this couroutine is inconsistent if the duration is the same
        
        
		Clear(strokeGraphic);
        if (strokeGraphic != null) strokeGraphic.canvasRenderer.SetColor(Color.white);
    }
	
	void OnStrokeDraw(Stroke[] strokes){
		GetBounds(strokes);
		Clear(currentStrokeGraphic);
		if (strokes!=null) Set(currentGlyphGraphic,strokes);
		else Clear(currentGlyphGraphic);
	}
	
	void OnPointDraw(Vector2[] points){
		if (points!=null) Set(currentStrokeGraphic,new Stroke[]{ new Stroke(points) });
		else Clear(currentStrokeGraphic);
	}

	Rect drawingBounds = new Rect();
	void GetBounds(Stroke[] strokes){
		if (strokes==null || strokes.Length==0) return;
		Rect bounds0 = strokes[0].Bounds;
		Vector2 min = bounds0.min, max = bounds0.max;
		for (int s=1;s<strokes.Length;s++)
		{
			Rect bounds = strokes[s].Bounds;
			Vector2 sMin = bounds.min, sMax = bounds.max;
			if (sMin.x < min.x) min.x = sMin.x;
			if (sMax.x > max.x) max.x = sMax.x;
			if (sMin.y < min.y) min.y = sMin.y;
			if (sMax.y > max.y) max.y = sMax.y;
			
		}
		drawingBounds.min=min; drawingBounds.max=max;
	}
}

