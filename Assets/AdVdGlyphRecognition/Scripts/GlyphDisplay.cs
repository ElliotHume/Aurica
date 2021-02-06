using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace AdVd.GlyphRecognition{
	/// <summary>
	/// Glyph display component. Needs a stroke graphic componenet to work.
	/// </summary>
	[RequireComponent(typeof(RectTransform))][ExecuteInEditMode]
	public class GlyphDisplay : MonoBehaviour{

		[SerializeField]
		private Glyph _glyph;
		/// <summary>
		/// Gets or sets the glyph to display.
		/// </summary>
		/// <value>The glyph.</value>
		public Glyph glyph{
			get{ return _glyph; }
			set{ _glyph=value; RebuildGlyph(); }
		}

		private StrokeGraphic strokeGraphic;

		void OnEnable(){
			RebuildGlyph();
		}

		#if UNITY_EDITOR
		void OnValidate()
		{
			if (gameObject.activeInHierarchy) RebuildGlyph();
		}
		#endif

		/// <summary>
		/// Rebuilds the glyph.
		/// </summary>
		public void RebuildGlyph () {
			if (strokeGraphic == null) strokeGraphic = GetComponent<StrokeGraphic>();
			if (strokeGraphic != null){
				if (_glyph!=null) strokeGraphic.SetStrokes(_glyph);
				else strokeGraphic.ClearStrokes();
			}
			else{
				Debug.LogError("A stroke graphic component is required to display the glyph.");
			}
		}
	}
}
