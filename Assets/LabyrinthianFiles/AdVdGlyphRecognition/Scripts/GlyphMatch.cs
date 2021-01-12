using UnityEngine;
using System;
using System.Collections.Generic;

namespace AdVd.GlyphRecognition
{
    public class GlyphMatch
    {
        
        public Glyph source, target;
		private float cost, threshold;
        private StrokeMatch[] strokeMatches;
        public GlyphMatch(Glyph src, Glyph tgt, StrokeMatch[] matches, float cost, float threshold)
        {
            source = src; target = tgt;
            strokeMatches = matches;
            this.cost = cost;
			this.threshold = threshold;
        }

		/// <summary>
		/// Gets the cost of the match.
		/// </summary>
		/// <value>The cost.</value>
		public float Cost { get{ return cost; } }
		/// <summary>
		/// Gets the max cost threshold for the match to be valid.
		/// </summary>
		/// <value>The threshold.</value>
		public float Threshold { get{ return threshold; } }
		/// <summary>
		/// Gets a value indicating whether this GlyphMatch is a valid match.
		/// </summary>
		/// <value><c>true</c> if valid; otherwise, <c>false</c>.</value>
		public bool Valid{ get{ return cost<=threshold; } }

		/// <summary>
		/// Gets the stroke array of a step of the morphing from the source glyph to the target glyph.
		/// </summary>
		/// <returns>The glyph strokes.</returns>
		/// <param name="t">T.</param>
		[Obsolete("Use SetLerpStrokes instead since it reuses stroke arrays when posible.")]
		public Stroke[] GetLerpStrokes(float t){
			Stroke[] strokes=new Stroke[strokeMatches.Length];
			for(int i=0;i<strokes.Length;i++) strokes[i]=strokeMatches[i].Lerp(t);
			return strokes;
		}

		/// <summary>
		/// Sets the stroke array ta a step of the morphing from the source glyph to the target glyph.
		/// </summary>
		/// <param name="t">T.</param>
		/// <param name="strokesArray">Strokes array, can be null.</param>
		public void SetLerpStrokes(float t, ref Stroke[] strokesArray) {
			if (strokesArray == null || strokesArray.Length != strokeMatches.Length) {
				strokesArray = new Stroke[strokeMatches.Length];
				for(int i = 0; i < strokesArray.Length; i++) strokesArray[i] = strokeMatches[i].Lerp(t);
			}
			else {
				for(int i = 0; i < strokesArray.Length; i++) strokesArray[i].SetToMatchLerp(strokeMatches[i], t);
			}
		}

        public class StrokeMatch
        {
            public float cost, weight;
            Stroke a, b;
            int[] aIndices;
            int[] bIndices;

            public StrokeMatch(float c, float w, Stroke a, Stroke b, int[] aIndices, int[] bIndices)
            {
                if (aIndices.Length != bIndices.Length) throw new ArgumentException("StrokeMatch: Indices arrays lengths do not match.");
                for (int i = 0; i < aIndices.Length; i++)
                {
                    if (aIndices[i] < 0 || aIndices[i] >= a.Length || bIndices[i] < 0 || bIndices[i] >= b.Length) throw new ArgumentException("StrokeMatch: Indices in arrays are out of bounds.");
                }
                this.cost = c; this.weight = w;
                this.a = a; this.b = b;
                this.aIndices = aIndices;
                this.bIndices = bIndices;
            }

            public int Length { get { return aIndices.Length; } }
            public Vector2 this[int index, float t]
            {
                get
                {
                    return Vector2.Lerp(a[aIndices[index]], b[bIndices[index]], t);
                }
            }

			/// <summary>
			/// This method creates a new array set to a lerp state of the stroke match.
			/// Use Stroke.SetToMatchLerp instead to avoid recreating the Stroke every time.
			/// </summary>
			/// <param name="t">T.</param>
            public Stroke Lerp(float t)
            {
				Vector2[] pointArray = new Vector2[Length];
                for (int index = 0; index < Length; index++) pointArray[index] = this[index, t];
                return new Stroke(pointArray);
            }

        }

    }
}
