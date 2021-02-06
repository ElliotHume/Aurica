using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace AdVd.GlyphRecognition
{
    /// <summary>
    /// Draws glyphs and strokes repeating a texture in the U axis.
    /// </summary>
    [ExecuteInEditMode]
    public class RepeatStrokeGraphic : StrokeGraphic {
        
        protected override void BuildStrokeMesh(Stroke s, VertexHelper vh)
        {
            if (s.Length < 2) return;

            float accumLength = 0;
            Vector3 prev = Vector2.Scale(s[0], scale);
            Vector3 curr = Vector2.Scale(s[1], scale);
            Vector3 w = new Vector3(prev.y - curr.y, curr.x - prev.x, 0);
            float d = w.magnitude / width;//Relative length
            if (d > 0) w *= 0.5f / d;
            vh.AddVert(prev - w, color, new Vector2(accumLength, 0));
            vh.AddVert(prev + w, color, new Vector2(accumLength, 1));
            accumLength += d;

            int vertexCount;
            for (int p = 2; p < s.Length; p++)
            {
                Vector3 next = Vector2.Scale(s[p], scale);
                Vector3 w2 = new Vector3(curr.y - next.y, next.x - curr.x, 0);
                d = w2.magnitude / width;//Relative length
                if (d > 0) w2 *= 0.5f / d;
                w = (w + w2).normalized * width * 0.5f;
                
                vh.AddVert(curr - w, color, new Vector2(accumLength, 0));
                vh.AddVert(curr + w, color, new Vector2(accumLength, 1));
                vertexCount = vh.currentVertCount;
                vh.AddTriangle(vertexCount - 4, vertexCount - 3, vertexCount - 2);
                vh.AddTriangle(vertexCount - 2, vertexCount - 3, vertexCount - 1);
                accumLength += d;

                prev = curr; curr = next;
                w = w2;
            }

            vh.AddVert(curr - w, color, new Vector2(accumLength, 0));
            vh.AddVert(curr + w, color, new Vector2(accumLength, 1));
            vertexCount = vh.currentVertCount;
            vh.AddTriangle(vertexCount - 4, vertexCount - 3, vertexCount - 2);
            vh.AddTriangle(vertexCount - 2, vertexCount - 3, vertexCount - 1);
        }


    }
}
