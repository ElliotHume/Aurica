using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace AdVd.GlyphRecognition
{
    /// <summary>
    /// Component for graphical visualization of glyphs and strokes.
    /// </summary>
    [ExecuteInEditMode]
    public abstract class StrokeGraphic : Graphic
    {
        /// <summary>
        /// Relative width of the strokes.
        /// </summary>
        [Range(0,0.5f)]
        public float relativeWidth = 0.02f;

        protected Vector2 scale = Vector2.one;
        protected float width;

        private Stroke[] strokes;

        public override Texture mainTexture
        {
            get
            {
                Texture texture = (material == null ? Texture2D.whiteTexture : material.mainTexture);
                return (texture == null ? Texture2D.whiteTexture : texture);
            }
        }
        
        /// <summary>
        /// Sets the renderer to draw the strokes of a glyph.
        /// </summary>
        /// <param name="glyph">Glyph.</param>
        public void SetStrokes(Glyph glyph)
        {
            Stroke[] strokes = new Stroke[glyph.Length];
            for (int s = 0; s < strokes.Length; s++) strokes[s] = glyph[s];
            SetStrokes(strokes);
        }
        /// <summary>
        /// Sets the renderer to draw a set of strokes.
        /// </summary>
        /// <param name="strokes">Strokes.</param>
        public void SetStrokes(Stroke[] strokes)
        {
            this.strokes = strokes;
            SetVerticesDirty();
        }

        /// <summary>
        /// Sets the renderer to draw nothing.
        /// </summary>
        public void ClearStrokes()
        {
            strokes = null;
            SetVerticesDirty();
        }


        /// <summary>
        /// Check whether the renderer should be clear or drawing strokes.
        /// </summary>
        /// <value><c>true</c> if this renderer is clear; otherwise, <c>false</c>.</value>
        public bool IsClear
        {
            get { return strokes == null; }
        }


        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (strokes == null) return;

            scale = (transform as RectTransform).rect.size;
            width = relativeWidth * scale.magnitude;
            
            foreach (Stroke s in strokes) BuildStrokeMesh(s, vh);
        }
        
        /// <summary>
        /// Fills the vertex helper to build the stroke mesh.
        /// </summary>
        /// <param name="s">Stroke to draw.</param>
        /// <param name="vh">Vertex helper.</param>
        protected abstract void BuildStrokeMesh(Stroke s, VertexHelper vh);
             
    }
}
