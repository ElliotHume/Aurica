using UnityEngine;

namespace AdVd.GlyphRecognition
{
	/// <summary>
	/// Stroke to stroke base matching method. GetStrokeMatch must be implemented. 
	/// </summary>
    public abstract class StrokeToStrokeMatchingMethod : MatchingMethod
    {
		public const float defaultThreshold = 0.09f;

		/// <summary>
		/// Try to match the specified glyphs. Returns null if the match fails.
		/// </summary>
		/// <param name="src">Source.</param>
		/// <param name="tgt">Target.</param>
        public override GlyphMatch Match(Glyph src, Glyph tgt)
        {
//            Debug.Log("GetDifference: " + src.name + " ~ " + tgt.name);
            int[] indexMatch = MatchStrokes(src, tgt);

            if (indexMatch == null) return null;//failed
            GlyphMatch.StrokeMatch[] matches = new GlyphMatch.StrokeMatch[indexMatch.Length];
            for (int i = 0; i < indexMatch.Length; i++)
            {
                int j = indexMatch[i];
                matches[i] = matchMatrix[i, j];
            }
            float costSum = 0, weight = 0;
            foreach (GlyphMatch.StrokeMatch sm in matches) { costSum += sm.cost; weight += sm.weight; }
			GlyphMatch result = new GlyphMatch(src, tgt, matches, Mathf.Sqrt(costSum / weight), threshold);
            src = null; tgt = null; //matches = null;
            matchMatrix = null; error = null;
            return result;
        }

        protected float[,] error;
        protected GlyphMatch.StrokeMatch[,] matchMatrix;
        protected virtual int[] MatchStrokes(Glyph src, Glyph tgt)
        {
            if (src.Length != tgt.Length) return null;//Failed
            error = new float[src.Length, tgt.Length];
            matchMatrix = new GlyphMatch.StrokeMatch[src.Length, tgt.Length];

            for (int i = 0; i < src.Length; i++)
            {
                for (int j = 0; j < tgt.Length; j++)
                {
                    srcStroke = src[i]; tgtStroke = tgt[j];
                    matchMatrix[i, j] = GetStrokeMatch();
                    if (matchMatrix[i, j] == null) return null;
                    error[i, j] = matchMatrix[i, j].cost;
//                    float meanCost = Mathf.Sqrt(matchMatrix[i, j].cost / matchMatrix[i, j].weight);
//                    Debug.Log("Error(" + i + "," + j + "): " + matchMatrix[i, j].cost + " / " + matchMatrix[i, j].weight + " (" + meanCost + ")");
                }
            }

            srcStroke = null; tgtStroke = null;

            return HungarianMethod(error);
        }

        protected Stroke srcStroke = null, tgtStroke = null;
        protected abstract GlyphMatch.StrokeMatch GetStrokeMatch();


        
    }
}
