using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace AdVd.GlyphRecognition
{
    [Serializable]
    public class Glyph : ScriptableObject, IEnumerable
    {
		[SerializeField]
        Stroke[] strokes;

		/// <summary>
		/// Creates a normalized glyph.
		/// </summary>
		/// <returns>The glyph.</returns>
		/// <param name="strokes">Strokes.</param>
        static public Glyph CreateGlyph(Stroke[] strokes = null)
        {
			Glyph glyph = CreateInstance<Glyph>();
            glyph.strokes = (strokes != null ? glyph.strokes = (Stroke[])strokes.Clone() : glyph.strokes = new Stroke[0]);
            glyph.Normalize();
			return glyph;
        }
		/// <summary>
		/// Creates a glyph and resamples its strokes.
		/// </summary>
		/// <returns>The glyph.</returns>
		/// <param name="strokes">Strokes.</param>
		/// <param name="sampleDistance">Sample distance.</param>
		static public Glyph CreateGlyph(Stroke[] strokes, float sampleDistance){
			Glyph glyph = CreateGlyph(strokes);
			glyph.Resample(sampleDistance);
			return glyph;
		}


        public int Length
        {
            get { return strokes.Length; }
        }

        public Stroke this[int index]
        {
            get
            {
                return strokes[index];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return strokes.GetEnumerator();
        }
		/// <summary>
		/// Draws the glyph using gizmos.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="scale">Scale.</param>
		public void DrawGlyph(Vector2 position, Vector2 scale)
		{
			foreach (Stroke stroke in strokes) stroke.DrawStroke(position, scale);
		}

		/// <summary>
		/// Normalize this glyph.
		/// </summary>
        public void Normalize()
        {
            if (strokes.Length == 0) return;
			// Using center of gravity
            //Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity), max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            
//			Vector2 center = Vector2.zero;
//            float length = 0;
//            foreach (Stroke s in strokes)
//            {
//                if (s.Length<2) continue;
//                length += s.strokeLength;
//                center += s.centerOfGravity * s.strokeLength;
//            }
//            center /= length;
			// using center of bounds
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
			Vector2 center=0.5f*(min+max);
			foreach(Stroke s in strokes) s.Translate(-center);//Stroke data is reset here
			min -= center; max -= center;
            Vector2 scale = Vector2.one / (Math.Max(Math.Max(max.x, -min.x), Math.Max(max.y, -min.y)) * 2);
            if (!float.IsNaN(scale.x) && !float.IsNaN(scale.y)) foreach (Stroke s in strokes)
            {
                s.Scale(scale);
            }

        }
		/// <summary>
		/// Resample this glyph with the specified sampleDistance. A sample distance sorter than 1e-3 does nothing.
		/// </summary>
		/// <param name="sampleDistance">Sample distance.</param>
		public void Resample(float sampleDistance = 0.05f){
			if (sampleDistance<Stroke.minSampleDistance) return;
			foreach (Stroke s in strokes)
			{
				s.Resample(sampleDistance);
			}
		}

        public override string ToString()
        {
            return name;
        }

        public static bool operator ==(Glyph a, Glyph b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;

            // Return true if the fields match:
            if (a.Length != b.Length) return false;
            for (int index = 0; index < a.Length; index++) if (a[index] != b[index]) return false;
            return true;
        }

        public static bool operator !=(Glyph a, Glyph b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            Glyph g = obj as Glyph;
            return this == g;
        }

        public override int GetHashCode()
        {
            return strokes.Length;
        }
    }
}
