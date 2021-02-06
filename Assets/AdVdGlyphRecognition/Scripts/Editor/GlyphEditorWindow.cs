using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace AdVd.GlyphRecognition{
	public class GlyphEditorWindow : EditorWindow {

		[MenuItem ("Window/Glyph Editor")]
		static internal void Init() {
			GlyphEditorWindow window = GetWindow<GlyphEditorWindow>(false, "Glyph Editor");
			window.Show();
			window.autoRepaintOnSceneChange=true;
//			window.CameraInit();
		}
		static internal GlyphEditorWindow currentWindow;

		
		void OnEnable(){
			if (Undo.undoRedoPerformed != this.OnUndoRedo){
				Undo.undoRedoPerformed += this.OnUndoRedo;
			}
			if (glyphCamera==null) CameraInit();
			currentWindow=this;
		}
		
		void OnFocus(){
			Repaint();
		}

		void OnSelectionChange(){
			Repaint();
		}

		void OnHierachyChange(){ Repaint(); }
		void OnInspectorUpdate() { Repaint(); }

		void OnUndoRedo(){ Repaint(); }

		Camera glyphCamera;
		void CameraInit(){
			if (glyphCamera !=null) return;
			GameObject cameraObject = new GameObject("_Custom Editor Camera", typeof(Camera));
			cameraObject.hideFlags = HideFlags.HideAndDontSave;
			cameraObject.SetActive(false);
			
			glyphCamera = cameraObject.GetComponent<Camera>();

			glyphCamera.orthographic=true;
			glyphCamera.aspect=1f;
			glyphCamera.orthographicSize=0.625f;
			glyphCamera.backgroundColor=new Color(0.25f,0.25f,0.25f);
			glyphCamera.cullingMask=0;
			glyphCamera.clearFlags=CameraClearFlags.Depth; 
		}

		void OnDestroy(){
			Undo.undoRedoPerformed -= this.OnUndoRedo;
			if (glyphCamera!=null) DestroyImmediate(glyphCamera.gameObject, true);
		}

		Vector2 scrollPos;
		int selectedTool;
		string[] toolbarNames=new string[]{ "Edit Glyph", "Edit Stroke", "Edit Point" };
		public GlyphEditor glyphEditor;
		bool drawStroke;
		List<Vector2> stroke;
		Vector2 prevPos;
		Rect displayRect;
		float minPointDistance=0.05f;
		void OnGUI(){
			selectedTool=GUILayout.Toolbar(selectedTool,toolbarNames,EditorStyles.toolbarButton);
			if (selectedTool!=0) { drawStroke=false; stroke=null; }
			float guiLineHeight = EditorGUIUtility.singleLineHeight;
			Vector2 glyphAreaSize=this.position.size;
			glyphAreaSize.y-=guiLineHeight*2.5f; glyphAreaSize.x-=6f;
			float dimDiff=glyphAreaSize.x-glyphAreaSize.y, minDim=Mathf.Min(glyphAreaSize.x,glyphAreaSize.y);

			displayRect = new Rect(Mathf.Max(dimDiff*0.5f,0f)+3f,guiLineHeight+3f,minDim,minDim);
			// Mouse Event Handling for stroke drawing
			if (drawStroke && Event.current.isMouse && displayRect.Contains(Event.current.mousePosition)){
				switch(Event.current.type){
				case EventType.MouseDown:
					stroke=new List<Vector2>();
					prevPos=CurrentMousePosition();
					stroke.Add (prevPos);
					break;
				case EventType.MouseDrag:
					if (stroke!=null){
						Vector2 currPos=CurrentMousePosition();
						if (stroke.Count==0 || (prevPos-currPos).sqrMagnitude>minPointDistance*minPointDistance){
							stroke.Add(currPos);
							prevPos=currPos;
							Repaint();
						}
					}
					break;
				case EventType.MouseUp:
					break;
				default:
					break;
				}
			}
			EditorGUI.DrawRect(new Rect(displayRect.x-1, displayRect.y-1,
			                            displayRect.width+2, displayRect.height+2), new Color(0.6f,0.6f,0.6f));
			EditorGUI.DrawRect(displayRect, new Color(0.3f,0.3f,0.3f));

			// Options/Info
			GUILayout.FlexibleSpace();
			EditorGUILayout.BeginHorizontal();
			GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
			centeredStyle.alignment=TextAnchor.UpperCenter;
			switch(selectedTool){
			case 0:
				GUI.enabled=glyphEditor!=null;
				if (!drawStroke){
					if (GUILayout.Button("New Stroke")) drawStroke=true;
					minPointDistance=Mathf.Max (0f,EditorGUILayout.FloatField(new GUIContent("Sample Distance","Values smaller than 1e-3 are ignored."), minPointDistance));
					if (GUILayout.Button("Resample All")) glyphEditor.Resample(minPointDistance);
					if (GUILayout.Button("Normalize")) glyphEditor.Normalize();
				}
				else{
					if (GUILayout.Button("Add Stroke")){
						if (stroke!=null && stroke.Count>1) glyphEditor.AddStroke(stroke.ToArray());
						stroke=null; drawStroke=false;
					}
					minPointDistance=Mathf.Max (0f,EditorGUILayout.FloatField(new GUIContent("Sample Distance","Values smaller than 1e-3 are ignored."), minPointDistance));
					if (GUILayout.Button("Cancel")){
						stroke=null; drawStroke=false;
					}
				}
				GUI.enabled=true;
				break;
			case 1:
				EditorGUILayout.LabelField("Default action: Move Stroke  -  Ctrl+Click: Delete Stroke",centeredStyle);
				break;
			case 2:
				EditorGUILayout.LabelField("Default action: Move point  -  Shift+Click: New point  -  Ctrl+Click: Delete point",centeredStyle);
				break;
			default:
				break;
			}
			EditorGUILayout.EndHorizontal();

			// Draw glyph handles
			if (glyphEditor!=null){
				if (glyphCamera==null) Debug.LogWarning("Camera is null");
				else{
					Handles.SetCamera(glyphCamera);
					Handles.matrix = baseMatrix*glyphCamera.cameraToWorldMatrix;
					Handles.DrawCamera(displayRect, glyphCamera, 
					                   DrawCameraMode.Normal);//Other than normal draws light/cam gizmos
					//Draw handles
					DrawGrid();
					Handles.color = Color.blue;
					glyphEditor.DrawGlyphHandleLines();
					if (Event.current.button==0 && displayRect.Contains(Event.current.mousePosition)){// && EditorWindow.focusedWindow==this
						if (selectedTool==2) {
							if (Event.current.control){
								Handles.color=Color.red;
								glyphEditor.DrawGlyphPointDeleteHandles();
							}
							else{
								Handles.color=Handles.centerColor;
								glyphEditor.DrawGlyphPointHandles();
								if (Event.current.shift) glyphEditor.DrawGlyphEdgeHandles();
							}
						}
						else if (selectedTool==1){
							if (Event.current.control){
								Handles.color=Color.red;
								glyphEditor.DrawGlyphStrokeHandles(true);
								//Handles.color=Color.blue;
							}
							else{
								Handles.color=Handles.centerColor;
								glyphEditor.DrawGlyphStrokeHandles(false);
							}
						}
					}
					if (drawStroke){// Draw current stroke
						Handles.color=Handles.centerColor;
						if (stroke!=null && stroke.Count>1){
							Vector3 prev, curr=stroke[0];
							for(int p=1;p<stroke.Count;p++){
								prev=curr; curr=stroke[p];
								Handles.DrawLine(prev,curr);
							}
						}
					}
				} 
				EditorUtility.SetDirty(glyphEditor.target);  
			}

		}

		void DrawGrid(){
			Handles.color = new Color(0.5f,0.5f,0.5f,0.5f);

			for(float t=-0.6f;t<0;t+=0.1f){
				Handles.DrawLine(new Vector2(-1,t),new Vector2(1,t));
				Handles.DrawLine(new Vector2(-1,-t),new Vector2(1,-t));
				Handles.DrawLine(new Vector2(t,-1),new Vector2(t,1));
				Handles.DrawLine(new Vector2(-t,-1),new Vector2(-t,1));
			}
			Handles.color = Handles.centerColor;
			Handles.DrawLine(Vector3.right, Vector3.left);
			Handles.DrawLine(Vector3.up, Vector3.down);
		}
		
		Vector2 CurrentMousePosition(){
			Vector2 p=Event.current.mousePosition-displayRect.center;
			p.x/=displayRect.width*0.8f; p.y/=-displayRect.height*0.8f;
			return p;
		}
		
		Matrix4x4 baseMatrix=Matrix4x4.TRS(Vector3.forward,Quaternion.identity,Vector3.one);


	}

}

