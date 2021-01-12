using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

namespace AdVd.GlyphRecognition{
	[CustomEditor(typeof(GlyphDrawInput))]
	public class GlyphDrawInputEditor : Editor {
		/// <summary>
		/// Create a glyph input.
		/// </summary>
		/// <param name="menuCommand">Menu command.</param>
		[UnityEditor.MenuItem ("GameObject/UI/AdVd/Glyph Input")]
		static public void CreateGlyphInput(MenuCommand menuCommand){
			GameObject parent = AdVd.UIEditorUtility.GetOrCreateCanvasAndEventSystem(menuCommand);

			GameObject go = new GameObject("Glyph Input", typeof(GlyphDrawInput));
			GameObjectUtility.SetParentAndAlign(go, parent);
			RectTransform rt=go.GetComponent<RectTransform>();
			rt.anchoredPosition=Vector2.zero; rt.sizeDelta=Vector2.zero;
			rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one;
			Undo.RegisterCreatedObjectUndo(go, "Create Glyph Input");
			Selection.activeObject = go;
		}

		public override void OnInspectorGUI(){
			GlyphDrawInput gdi=target as GlyphDrawInput;
//			this.DrawDefaultInspector();

			GlyphDrawInput.Matching_Method method = gdi.Method;
			GlyphDrawInput.Series_Generator generator = gdi.SeriesGenerator;
			float alpha = gdi.Alpha, sf = gdi.SobolevFactor;

			method = (GlyphDrawInput.Matching_Method) EditorGUILayout.EnumPopup("Matching Method", method);
			
			if (method == GlyphDrawInput.Matching_Method.SqrDTWMatchingMemoryCostMethod ||
			    method == GlyphDrawInput.Matching_Method.SqrMemoryMatchingMethod){
				alpha=EditorGUILayout.Slider(new GUIContent("Alpha","The bigger the value, more error is \"forgiven\"."), alpha, 0f, 1f);
			}
			if (method == GlyphDrawInput.Matching_Method.LegendreMatchingMethod){
				generator = (GlyphDrawInput.Series_Generator) EditorGUILayout.EnumPopup("Series Generator", generator);//TODO add tooltip!!
				if (generator==GlyphDrawInput.Series_Generator.LegendreSobolevSeries){
					sf=EditorGUILayout.FloatField(new GUIContent("Sobolev Factor","A value of 0 is the same as using Legendre series."), sf);
				}
			}
			EditorGUILayout.Separator();
			GlyphSet gs = EditorGUILayout.ObjectField("Target Glyph Set", gdi.targetGlyphSet, typeof(GlyphSet), false) as GlyphSet;
			float ngs = EditorGUILayout.Slider(new GUIContent("Normalized Size","Relative size of a normalized glyph."), gdi.normalizedGlyphSize, 0f, 1f);
			float mpd = EditorGUILayout.Slider(new GUIContent("Sample Distance","Values smaller than 1e-3 are ignored."), gdi.sampleDistance, 0f, 1f);
			bool tap = EditorGUILayout.ToggleLeft("Cast on tap", gdi.castOnTap);
			
			EditorGUILayout.BeginHorizontal();
			float threshold=gdi.Threshold;
			bool ort = EditorGUILayout.ToggleLeft(new GUIContent("Override threshold"), gdi.overrideThreshold);
			GUI.enabled=ort;
			threshold = EditorGUILayout.FloatField(threshold);
			GUI.enabled=true;
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Separator();
			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("OnGlyphCast"));
			serializedObject.ApplyModifiedProperties();
			
			if (GUI.changed) {
				Undo.RecordObject(gdi, "Inspector Change");
				//Set parameters
				if (method!=gdi.Method) gdi.Method=method;//TODO: Check if game is running?
				if (generator!=gdi.SeriesGenerator) gdi.SeriesGenerator=generator;
				if (alpha!=gdi.Alpha) gdi.Alpha=alpha;
				if (sf!=gdi.SobolevFactor) gdi.SobolevFactor=sf;
				gdi.targetGlyphSet = gs;
				gdi.normalizedGlyphSize=ngs;
				gdi.sampleDistance = mpd;
				gdi.castOnTap = tap;
				gdi.overrideThreshold=ort;
				gdi.Threshold=threshold;
			}

			EditorUtility.SetDirty(target);
		}

	}
}
