using UnityEngine;
using UnityEditor;
using System.Collections;

namespace AdVd.GlyphRecognition{
	[CustomEditor(typeof(GlyphDisplay))][CanEditMultipleObjects]
	public class GlyphDisplayEditor : Editor {

		[MenuItem ("GameObject/UI/AdVd/Glyph Display")]
		static public void CreateGlyphDisplay(MenuCommand menuCommand){
			int opt=EditorUtility.DisplayDialogComplex("Create Glyph Display", "Choose the stroke graphic component to use.",
			                                           "Repeat Texture", "Cap & Stretch", "Cancel");

			if (opt==2) return;//Cancel
			GameObject parent = UIEditorUtility.GetOrCreateCanvasAndEventSystem(menuCommand);

			GameObject go = new GameObject("Glyph Display", 
			                               (opt==0 ? typeof(RepeatStrokeGraphic) : typeof(CapStretchStrokeGraphic)),
			                               typeof(GlyphDisplay));
			GameObjectUtility.SetParentAndAlign(go, parent);
			RectTransform rt=go.GetComponent<RectTransform>();
			rt.anchoredPosition=Vector2.zero; rt.sizeDelta=Vector2.zero;
			rt.anchorMin=new Vector2(0.1f,0.1f); rt.anchorMax=new Vector2(0.9f,0.9f);
			Undo.RegisterCreatedObjectUndo(go, "Create Glyph Display");
			Selection.activeObject = go;
		}

		public override void OnInspectorGUI ()
		{
			if (targets.Length==1){
				GlyphDisplay display = target as GlyphDisplay;
				Glyph glyph = EditorGUILayout.ObjectField("Glyph", display.glyph, typeof(Glyph), false) as Glyph;
				if (GUI.changed){
					Undo.RecordObject(display, "Inspector Change");
					display.glyph=glyph;
				}
				EditorUtility.SetDirty(display);
			}
			else{
				foreach(Object t in targets){
					GlyphDisplay display = t as GlyphDisplay;
					Glyph glyph = EditorGUILayout.ObjectField(display.name, display.glyph, typeof(Glyph), false) as Glyph;
					if (GUI.changed){
						Undo.RecordObject(display, "Inspector Change");
						display.glyph=glyph;
					}
					EditorUtility.SetDirty(display);
				}
			}
		}
	}
}

