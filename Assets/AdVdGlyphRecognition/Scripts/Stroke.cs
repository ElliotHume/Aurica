using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace AdVd.GlyphRecognition
{
    [Serializable]
    public class Stroke : IEnumerable
    {
		[SerializeField]
        Vector2[] points = new Vector2[0];

        public int Length
        {
            get { return points.Length; }
        }

        public Vector2 this[int index] {
            get {
                return points[index];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return points.GetEnumerator();
        }

        public Stroke(Vector2[] points)
        {
            this.points = (points != null ? this.points = (Vector2[]) points.Clone() : this.points = new Vector2[0]);
        }

		public Stroke(int pointCount = 0) {
			this.points = new Vector2[pointCount];
		}

		/// <summary>
		/// Draws the stroke using gizmos.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="scale">Scale.</param>
		public void DrawStroke(Vector2 position, Vector2 scale)
		{
			if (points.Length>1){
				Vector2 prevPoint, currPoint=Vector2.Scale(points[0],scale)+position;
				for (int i = 1; i < points.Length; i++){
					prevPoint=currPoint; currPoint=Vector2.Scale(points[i],scale)+position;
					Gizmos.DrawLine(prevPoint, currPoint);
				}
			}
		}
        

        /// <summary>
        /// Inverse of this stroke.
        /// </summary>
        internal Stroke Inverse()
        {
            Vector2[] iPoints = new Vector2[this.Length];
            for (int i = 0; i < this.Length; i++) iPoints[i] = points[this.Length - 1 - i];
            return new Stroke(iPoints);
        }

		/// <summary>
		/// Gets the bounds of the stroke.
		/// </summary>
		/// <value>The bounds.</value>
		public Rect Bounds{
			get{
				if (points.Length==0) return new Rect(0,0,0,0);
				float minX=points[0].x, minY=points[0].y, maxX=points[0].x, maxY=points[0].y;
				for (int i = 1; i < points.Length; i++)
				{
					if (points[i].x < minX) minX = points[i].x;
					else if (points[i].x > maxX) maxX = points[i].x;
					if (points[i].y < minY) minY = points[i].y;
					else if (points[i].y > maxY) maxY = points[i].y;
				}
				return new Rect(minX, minY, maxX-minX, maxY-minY);
			}
		}
		/// <summary>
		/// Translate this stroke to the specified position.
		/// </summary>
		/// <param name="position">Position.</param>
		public void Translate(Vector2 position)
		{
			for (int i = 0; i < Length; i++)
			{
				points[i] += position;
			}
		}
		/// <summary>
		/// Scale this stroke by specified value.
		/// </summary>
		/// <param name="scale">Scale.</param>
		public void Scale(Vector2 scale)
        {
            for (int i = 0; i < Length; i++)
            {
                points[i].Scale(scale);
            }
        }

		/// <summary>
		/// Sets this stroke to a lerp state of a stroke match.
		/// </summary>
		/// <param name="strokeMatch">Stroke Match.</param>
		/// <param name="t">T.</param>
		public void SetToMatchLerp(GlyphMatch.StrokeMatch strokeMatch, float t) {
			if (points.Length != strokeMatch.Length) {
				points = new Vector2[strokeMatch.Length];//Resize points array
			}
			for (int index = 0; index < Length; index++) points[index] = strokeMatch[index, t];
		}

		public const float minSampleDistance = 1e-3f;
		/// <summary>
		/// Resample this stroke by the specified sampleDistance. A sample distance sorter than 1e-3 does nothing.
		/// </summary>
		/// <param name="sampleDistance">Sample distance.</param>
        public void Resample(float sampleDistance)
        {
			if (sampleDistance<minSampleDistance) return;
            float totalLength=0;
            float[] lengths=new float[points.Length-1];
            for (int i = 0; i < lengths.Length; i++)
            {
                totalLength += (lengths[i] = Vector2.Distance(points[i], points[i + 1]));
            }
            int numberOfSamples = (int)Math.Floor(totalLength / sampleDistance);
            sampleDistance = totalLength / numberOfSamples;
            Vector2[] aux = new Vector2[numberOfSamples+1];
            int auxIndex = 0;
            float progress = 0f;
            for (int i = 0; i < lengths.Length; i++)
            {
                float segLength = lengths[i];
                while (progress < segLength && auxIndex < numberOfSamples)
                {
                    aux[auxIndex] = Vector2.Lerp(points[i], points[i + 1], progress / segLength);
                    auxIndex++;
                    progress += sampleDistance;
                }
                progress -= segLength;
            }
            aux[numberOfSamples] = points[Length - 1];
            points = aux;
        }

        public static bool operator ==(Stroke a, Stroke b)
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

        public static bool operator !=(Stroke a, Stroke b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            Stroke s = obj as Stroke;
            return this == s;
        }

        public override int GetHashCode()
        {
            return points.Length;
        }

    }
}
