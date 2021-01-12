using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AdVd.GlyphRecognition{
	public class GlyphSet : ScriptableObject, IEnumerable {
		[SerializeField]
		Glyph[] glyphs;

		public int Length
		{
			get { return glyphs.Length; }
		}
		
		public Glyph this[int index]
		{
			get
			{
				return glyphs[index];
			}
		}

		public IEnumerator GetEnumerator ()
		{
			return glyphs.GetEnumerator();
		}

		/// <summary>
		/// Gets a copy of the glyphs array or sets the glyphs array.
		/// </summary>
		/// <value>The glyphs.</value>
		public Glyph[] Glyphs{
			get{ return glyphs.Clone() as Glyph[]; }
			set{ glyphs=value; }
		}

		static public implicit operator Glyph[](GlyphSet gs)
		{
			return (gs==null ? null : gs.Glyphs);
		}
		
		#if UNITY_EDITOR
		[UnityEditor.MenuItem ("Assets/Create/Glyph Set")]
		public static void CreateGlyphSet(){
			GlyphSet glyphSet= GlyphSet.CreateInstance<GlyphSet>();
			Object[] selectedObjs=UnityEditor.Selection.objects;
			if (selectedObjs!=null){
				List<Glyph> glyphs=new List<Glyph>(selectedObjs.Length);
				foreach(Object obj in selectedObjs) if (obj is Glyph) glyphs.Add((Glyph) obj);
				glyphSet.Glyphs=glyphs.ToArray();
			}
			UnityEditor.ProjectWindowUtil.CreateAsset(glyphSet,"New GlyphSet.asset");
		}
		#endif
	}
}
