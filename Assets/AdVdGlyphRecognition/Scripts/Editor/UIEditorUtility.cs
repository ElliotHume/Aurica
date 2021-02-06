//Source: https://bitbucket.org/ddreaper/unity-ui-extensions/src/3456cfee9b5531fc6070299dc3599258b622d467/Scripts/Editor/UIExtensionsMenuOptions.cs?at=default&fileviewer=file-view-default
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace AdVd{
	/// <summary>
	/// UI editor utility. Finds or creates a canvas to be the parent of a new UI object.
	/// </summary>
	static public class UIEditorUtility {

		static public GameObject GetOrCreateCanvasAndEventSystem(MenuCommand menuCommand){
			GameObject parent = menuCommand.context as GameObject;
			if (parent == null || parent.GetComponentInParent<Canvas>() == null)
			{
				GameObject selectedGo = Selection.activeGameObject;
				
				// Try to find a gameobject that is the selected GO or one if its parents.
				Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
				if (canvas != null && canvas.gameObject.activeInHierarchy) return canvas.gameObject;
				
				// No canvas in selection or its parents? Then use just any canvas..
				canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
				if (canvas != null && canvas.gameObject.activeInHierarchy) return canvas.gameObject;
				
				// No canvas in the scene at all? Then create a new one.
				// Root for the UI
				GameObject root = new GameObject("Canvas");
				root.layer = LayerMask.NameToLayer("UI");
				canvas = root.AddComponent<Canvas>();
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				root.AddComponent<CanvasScaler>();
				root.AddComponent<GraphicRaycaster>();
				Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

				// if there is no event system add one...
				CreateEventSystem();
				return root;
				////
			}
			return parent;
		}

		static public void CreateEventSystem(){
			EventSystem esys = Object.FindObjectOfType<EventSystem>();
			if (esys == null)
			{
				GameObject eventSystem = new GameObject("EventSystem");
				esys = eventSystem.AddComponent<EventSystem>();
				eventSystem.AddComponent<StandaloneInputModule>();
				//eventSystem.AddComponent<TouchInputModule>();
				
				Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
			}
		}

	}
}
