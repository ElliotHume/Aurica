using UnityEngine;
using UnityEditor;
using System.Collections;

namespace AdVd.GlyphRecognition{
	[CustomEditor(typeof(Glyph))][CanEditMultipleObjects]
	public class GlyphEditor : Editor {
		Camera glyphCamera;

		[UnityEditor.MenuItem ("Assets/Create/Glyph")]
		public static void CreateGlyph(){
			Glyph glyph=Glyph.CreateGlyph();
			UnityEditor.ProjectWindowUtil.CreateAsset(glyph,"New Glyph.asset");
		}

		SerializedProperty strokes;
		void OnEnable(){
			strokes = serializedObject.FindProperty("strokes");

			if (GlyphEditorWindow.currentWindow!=null){//currentWindows becomes null on sync/compile/deserialize/save?
				GlyphEditorWindow.currentWindow.glyphEditor=this;
				GlyphEditorWindow.currentWindow.Repaint();
			}
		}
		void OnDisable(){
			if (pru!=null) pru.Cleanup();
		}

		public override void OnInspectorGUI ()
		{
			if (targets.Length>1) return;

			serializedObject.Update();
			if (strokes!=null && EditorGUILayout.PropertyField(strokes, new GUIContent(target.name), false)){
				EditorGUI.indentLevel++;
				SerializedProperty child = strokes.Copy();
				child.Next (true);//array
				child.Next (true);//size
				EditorGUILayout.PropertyField(child, new GUIContent("Number of strokes"), true);
				int s=0, nStrokes=child.intValue;
				while(child.Next(false) && s<nStrokes){
					if (child!=null && EditorGUILayout.PropertyField(child, new GUIContent("Stroke "+s), false)){
						EditorGUI.indentLevel++;
						SerializedProperty childchild = child.Copy();
						childchild.Next (true);//points
						childchild.Next (true);//array
						childchild.Next (true);//size
						EditorGUILayout.PropertyField(childchild, new GUIContent("Size"), true);
						int p=0, nPoints=childchild.intValue;
						while (childchild.Next(false) && p<nPoints){
							if (childchild!=null){
								EditorGUILayout.PropertyField(childchild, new GUIContent("Point "+p));
							}
							p++;
						}
						EditorGUI.indentLevel--;
					}
					s++;
				}
				EditorGUI.indentLevel--;
			}
			serializedObject.ApplyModifiedProperties ();
		}
		
		Matrix4x4 baseMatrix=Matrix4x4.TRS(Vector3.forward*3,Quaternion.identity,Vector3.one);

		/// <summary>
		/// Draw the glyph using GL calls.
		/// </summary>
		public void GLDrawGlyph()
		{
			Glyph glyph = target as Glyph;
			foreach (Stroke stroke in glyph){

				GL.Begin(GL.LINES);
				GL.Color(Color.gray);
				if (stroke.Length>1){
					Vector2 prevPoint, currPoint=stroke[0];
					for (int i = 1; i < stroke.Length; i++){
						prevPoint=currPoint; currPoint=stroke[i];
						GL.Vertex3(prevPoint.x, prevPoint.y, 3f);
						GL.Vertex3(currPoint.x, currPoint.y, 3f);
					}
				}
				GL.End();
			} 
		}
		/// <summary>
		/// Draws the glyph handle lines.
		/// </summary>
		public void DrawGlyphHandleLines(){
			Glyph glyph = target as Glyph;
			
			foreach (Stroke stroke in glyph){
				if (stroke.Length>1){
					Vector2 prevPoint, currPoint=stroke[0];
					for (int i = 1; i < stroke.Length; i++){
						prevPoint=currPoint; currPoint=stroke[i];
						Handles.DrawLine(prevPoint, currPoint);
					}
				}
			} 
		}
		/// <summary>
		/// Resample glyph and record undo.
		/// </summary>
		/// <param name="sampleDist">Sample dist.</param>
		public void Resample(float sampleDist){
			Glyph glyph = target as Glyph;
			Undo.RecordObject(glyph, "Glyph Resample");
			glyph.Resample(sampleDist);
			EditorUtility.SetDirty(glyph);
		}
		/// <summary>
		/// Normalize glyph and record undo.
		/// </summary>
		public void Normalize(){
			Glyph glyph = target as Glyph;
			Undo.RecordObject(glyph, "Glyph Normalize");
			glyph.Normalize();
			EditorUtility.SetDirty(glyph);
		}

		/// <summary>
		/// Draws the glyph point handles.
		/// </summary>
		public void DrawGlyphPointHandles(){
			serializedObject.Update();
			handleSize=HandleUtility.GetHandleSize(Vector3.zero)*0.04f;
			if (strokes!=null){
				SerializedProperty child = strokes.Copy();
				child.Next (true); child.Next (true);
				int s=0, nStrokes=child.intValue;
				while(child.Next(false) && s<nStrokes){
					if (child!=null){
						SerializedProperty childchild = child.Copy();
						childchild.Next (true); childchild.Next (true); childchild.Next (true);
						int p=0, nPoints=childchild.intValue;
						while (childchild.Next(false) && p<nPoints){
							if (childchild!=null){
								DrawPointHandle(childchild);
							}
							p++;
						}
					}
					s++;
				}
			}
			serializedObject.ApplyModifiedProperties ();
		}
		/// <summary>
		/// Draws the glyph point delete handles.
		/// </summary>
		public void DrawGlyphPointDeleteHandles(){
			serializedObject.Update();
			handleSize=HandleUtility.GetHandleSize(Vector3.zero)*0.04f;
			if (strokes!=null){
				SerializedProperty child=strokes.Copy();
				child.Next(true);
				foreach(SerializedProperty childStroke in child){
					SerializedProperty childArray = childStroke.Copy();
					childArray.Next(true); childArray.Next(true);
					foreach(SerializedProperty childPoint in childArray){
						if (Handles.Button(childPoint.vector2Value, Quaternion.identity, handleSize, handleSize, Handles.DotHandleCap)){
							childPoint.DeleteCommand();
						}
					}
				}
			}
			serializedObject.ApplyModifiedProperties ();
		}
		/// <summary>
		/// Draws the glyph add-point-to-edge handles.
		/// </summary>
		public void DrawGlyphEdgeHandles(){
			serializedObject.Update();
			handleSize=HandleUtility.GetHandleSize(Vector3.zero)*0.04f;
			if (strokes!=null){
				SerializedProperty child=strokes.Copy();
				child.Next(true);
				float bestDist=float.PositiveInfinity;
				SerializedProperty bestPoint=null;
				foreach(SerializedProperty childStroke in child){
					SerializedProperty childArray = childStroke.Copy();
					childArray.Next(true); childArray.Next(true);
					SerializedProperty prev, curr=childArray.GetArrayElementAtIndex(0);
					for (int p=1;p<childArray.arraySize;p++){
						prev=curr; curr=childArray.GetArrayElementAtIndex(p);
						float dist=HandleUtility.DistanceToLine(prev.vector2Value, curr.vector2Value);
						if (dist<bestDist){
							bestDist=dist;
							bestPoint=prev;
						}
					}
				}
				if (bestDist < 5f){
					SerializedProperty nextPoint=bestPoint.Copy(); nextPoint.Next(false);
					Vector3 closestPoint = HandleUtility.ClosestPointToPolyLine(bestPoint.vector2Value, nextPoint.vector2Value);
					if (Handles.Button(closestPoint, Quaternion.identity, handleSize, handleSize, Handles.DotHandleCap)){
						//dup and move
						if (bestPoint.DuplicateCommand()){
							bestPoint.Next(false);
							bestPoint.vector2Value=(Vector2) closestPoint;
						}
					}
				}
			}
			serializedObject.ApplyModifiedProperties ();
		}
		
		float handleSize;
		void DrawPointHandle(SerializedProperty point){
			Vector3 p = point.vector2Value;
			p = Handles.Slider2D(p, Vector3.forward, Vector3.right, Vector3.up, handleSize, 
			                     Handles.DotHandleCap, Vector2.zero);
			point.vector2Value = p;
		}

		/// <summary>
		/// Draws the glyph stroke handles.
		/// </summary>
		/// <param name="delete">If set to <c>true</c> draws delete handles, 
		/// if <c>flase</c> draws move handles.</param>
		public void DrawGlyphStrokeHandles(bool delete=false){
			serializedObject.Update();
			handleSize=HandleUtility.GetHandleSize(Vector3.zero)*0.16f;
			if (strokes!=null){
				SerializedProperty child=strokes.Copy();
				child.Next(true);
				foreach(SerializedProperty childStroke in child){
					if (delete) DrawStrokeDeleteHandle(childStroke);
					else DrawStrokeHandle(childStroke);
				}
			}
			serializedObject.ApplyModifiedProperties ();
		}
		
		void DrawStrokeHandle(SerializedProperty stroke){
			SerializedProperty array = stroke.Copy();
			array.Next(true); array.Next(true);
			int count=0;
			Vector2 center=Vector2.zero;//Get center of the glyph.
			foreach(SerializedProperty point in array){
				Vector2 p = point.vector2Value;
				count++;
				center+=p;
			}
			if (count>0) center/=count;
			Vector2 v=new Vector2(handleSize,0);
			Handles.DrawLine(center+v,center-v);
			v=new Vector2(0,handleSize);
			Handles.DrawLine(center+v,center-v);
			Vector3 newCenter = Handles.Slider2D(center,Vector3.forward, Vector3.right, Vector3.up,
			                                     handleSize, Handles.CircleHandleCap, Vector2.zero);
			Vector2 displacement = (Vector2)newCenter-center;
			foreach(SerializedProperty point in array){
				point.vector2Value+=displacement;
			}
		}
		void DrawStrokeDeleteHandle(SerializedProperty stroke){
			SerializedProperty array = stroke.Copy();
			array.Next(true); array.Next(true);
			int count=0;
			Vector2 center=Vector2.zero;
			foreach(SerializedProperty point in array){
				Vector2 p = point.vector2Value;
				count++;
				center+=p;
			}
			if (count>0) center/=count;
			Vector2 v=new Vector2(handleSize,handleSize);
			Handles.DrawLine(center+v,center-v);
			v.y=-v.y;
			Handles.DrawLine(center+v,center-v);
			if (Handles.Button(center,Quaternion.identity, handleSize, handleSize, Handles.RectangleHandleCap)){
				stroke.DeleteCommand();
			}
		}

		/// <summary>
		/// Adds a stroke.
		/// </summary>
		/// <param name="newStroke">New stroke.</param>
		public void AddStroke(Vector2[] newStroke){
			serializedObject.Update();
			if (strokes!=null){
				//EditorGUILayout.PropertyField(strokes, true);
				SerializedProperty child = strokes.Copy(); //isArray->true
				child.Next(true);//array //isArray->true
				if (child.isArray){
					child.InsertArrayElementAtIndex(child.arraySize);
					//child.GetArrayElementAtIndex(0) type: Generic Mono, name: data
					SerializedProperty newChild = child.GetArrayElementAtIndex(child.arraySize-1);
					newChild.Next(true);
					newChild.Next(true);
					newChild.ClearArray();
					foreach(Vector2 v in newStroke){
						newChild.InsertArrayElementAtIndex(newChild.arraySize);
						newChild.GetArrayElementAtIndex(newChild.arraySize-1).vector2Value=v;
					}
				}
			}
			serializedObject.ApplyModifiedProperties();
		}

		public override void OnPreviewSettings() {
			if (GUILayout.Button("Show Editor", EditorStyles.whiteMiniLabel)){//toolbarButton)){//
				GlyphEditorWindow.Init();//Create window
				if (GlyphEditorWindow.currentWindow!=null) GlyphEditorWindow.currentWindow.glyphEditor=this;
			}
		}


		public override bool HasPreviewGUI ()
		{
			return true;
		}

		PreviewRenderUtility pru;
		public override void OnPreviewGUI (Rect r, GUIStyle background)
		{
			if (Event.current.type==EventType.Repaint){
				if (pru==null){
					pru=new PreviewRenderUtility(true);//Draws handles?
					glyphCamera=pru.camera;
					glyphCamera.orthographic=true;
					glyphCamera.aspect=1f;
					glyphCamera.orthographicSize=0.625f;
					glyphCamera.cullingMask=0;
				}
				Vector2 rCenter = r.center;
				if (r.width>r.height) r.width=r.height; else r.height=r.width;
				r.center = rCenter; 
				pru.BeginPreview(r, background);
				
				if (glyphCamera==null) Debug.LogWarning("Camera is null.");
				else{ 
					Handles.SetCamera(glyphCamera); 
					Handles.matrix = baseMatrix*glyphCamera.cameraToWorldMatrix;
					//Draw handles
					float d=0.62f;
					Handles.DrawSolidRectangleWithOutline(new Vector3[]{
						new Vector3(-d,-d,0), new Vector3(-d,d,0), new Vector3(d,d,0), new Vector3(d,-d,0)
					}, new Color(0.1f,0.1f,0.1f), new Color(0.7f,0.7f,0.7f));
					Handles.color = new Color(0.7f,0.7f,0.7f);
					DrawGlyphHandleLines();
//					GLDrawGlyph();
				}
				
				glyphCamera.Render();
				Texture previewTexture = pru.EndPreview();
				GUI.DrawTexture(r, previewTexture, ScaleMode.ScaleToFit);
			}
		}
	}
}
