using UnityEngine;
using System.Collections.Generic;

namespace AdVd.GlyphRecognition
{
	/// <summary>
	/// A stroke matching method that uses Legendre series distance as the feature distance between strokes. 
	/// The coefficients of the targets can be precomputed and reused, saving time in the long term. Stroke match time cost: O(k)  
	/// </summary>
    public class LegendreMatchingMethod : MatchingMethod
    {
		public const float defaultThreshold = 1.6f;

        protected LegendreSeries legendreGenerator;
        protected int degree;
        public LegendreMatchingMethod(int degree, float threshold = defaultThreshold)
        {
            this.threshold = threshold;
            this.degree = degree;
            InitCoefficientsGenerator();
        }
        protected virtual void InitCoefficientsGenerator()
        {
            legendreGenerator = new LegendreSeries(degree);
            legendreGenerator.Init();
        }
        public LegendreMatchingMethod(LegendreSeries generator, float threshold = defaultThreshold)
        {
            this.threshold = threshold;
            this.degree = generator.Degree;
            legendreGenerator = generator;
            legendreGenerator.Init();
        }

        Glyph[] targets;
        Vector2[][][] targetGlyphCoefficients;// [Glyph][Stroke][k]
		/// <summary>
		/// Sets the targets. The coefficients of modified glyphs won't be updated if the instance is the same.
		/// </summary>
		/// <param name="targets">Targets.</param>
        public virtual void SetTargets(params Glyph[] targets)
        {
//            Debug.Log("Tgt coeffs:");
            Vector2[][][] newTargetGlyphCoefficients = new Vector2[targets.Length][][];
            for (int g = 0; g < targets.Length; g++)
            {
				if (targets[g]==null){
					newTargetGlyphCoefficients[g]=null;
				}
				else{
					if (this.targets!=null && g < this.targets.Length && Glyph.ReferenceEquals(targets[g],this.targets[g])){
						newTargetGlyphCoefficients[g]=this.targetGlyphCoefficients[g];//Do not re-compute
					}
					else{//Compute new coefficients
		                newTargetGlyphCoefficients[g] = new Vector2[targets[g].Length][];
		                for (int s = 0; s < targets[g].Length; s++)
		                {
		                    newTargetGlyphCoefficients[g][s] = legendreGenerator.Compute(targets[g][s]);
		                }
					}
				}
			}
			this.targets = targets;
			this.targetGlyphCoefficients=newTargetGlyphCoefficients;
        }

		/// <summary>
		/// Set targets, then try to match a glyph with them and get the best match.
		/// </summary>
		/// <returns>The index of the best match, or -1 if there is no match.</returns>
		/// <param name="src">Source.</param>
		/// <param name="targets">Targets.</param>
		/// <param name="bestMatch">Best match info, or null if there is no match.</param>
        public override int MultiMatch(Glyph src, Glyph[] targets, out GlyphMatch bestMatch)
        {
            if (this.targets != targets) SetTargets(targets);
            return MultiMatch(src, out bestMatch);
		}
		/// <summary>
		/// Try to match a glyph with the current targets and get the best match.
		/// </summary>
		/// <returns>The index of the best match, or -1 if there is no match.</returns>
		/// <param name="src">Source.</param>
		/// <param name="bestMatch">Best match info, or null if there is no match.</param>
		public virtual int MultiMatch(Glyph src, out GlyphMatch bestMatch)
        {
            bestMatch = null;
            if (targets == null || targetGlyphCoefficients == null || targetGlyphCoefficients.Length != targets.Length) return -1;

//            Debug.Log("Src coeffs:");
            Vector2[][] srcGlyphCoeffs = new Vector2[src.Length][];
            for (int s = 0; s < src.Length; s++) srcGlyphCoeffs[s] = legendreGenerator.Compute(src[s]);

            float bestDiff = float.PositiveInfinity;
            int bestIndex = -1;
            for (int targetIndex = 0; targetIndex < targets.Length; targetIndex++)
			{
				if (targetGlyphCoefficients[targetIndex]==null) continue;
                int[] indexMatch = MatchStrokes(srcGlyphCoeffs, targetGlyphCoefficients[targetIndex]);
                GlyphMatch match = FinalizeMatch(src, targets[targetIndex], indexMatch);
                if (match!=null && match.Cost < bestDiff)
                {
                    bestDiff = match.Cost;
                    bestIndex = targetIndex;
                    bestMatch = match;
                }
            }
            return bestIndex;
        }

        public override string Name
        {
            get { return (legendreGenerator != null ? legendreGenerator.ToString() : "[NullGenerator]"); }
        }

        public override GlyphMatch Match(Glyph src, Glyph tgt)
        {
//            Debug.Log("GetDifference: " + src.name + " ~ " + tgt.name);

            Vector2[][] srcGlyphCoeffs = new Vector2[src.Length][];
            for (int s = 0; s < src.Length; s++) srcGlyphCoeffs[s] = legendreGenerator.Compute(src[s]);
            Vector2[][] tgtGlyphCoeffs = new Vector2[tgt.Length][];
            for (int s = 0; s < tgt.Length; s++) tgtGlyphCoeffs[s] = legendreGenerator.Compute(tgt[s]);

            int[] indexMatch = MatchStrokes(srcGlyphCoeffs, tgtGlyphCoeffs);
            return FinalizeMatch(src, tgt, indexMatch);
        }

        protected float[,] error;
        protected bool[,] directMatch;
        
        protected virtual int[] MatchStrokes(Vector2[][] srcGlyphCoeffs, Vector2[][] tgtGlyphCoeffs)
        {
            if (srcGlyphCoeffs.Length != tgtGlyphCoeffs.Length) return null;//Failed
            error = new float[srcGlyphCoeffs.Length, tgtGlyphCoeffs.Length];
            directMatch = new bool[srcGlyphCoeffs.Length, tgtGlyphCoeffs.Length];

            for (int i = 0; i < srcGlyphCoeffs.Length; i++)
            {
                for (int j = 0; j < tgtGlyphCoeffs.Length; j++)
                {
                    float dirError = StrokeCoeffDiff(srcGlyphCoeffs[i], tgtGlyphCoeffs[j]);
                    float invError = InvStrokeCoeffDiff(srcGlyphCoeffs[i], tgtGlyphCoeffs[j]);
                    error[i, j] = (directMatch[i, j] = (dirError < invError)) ? dirError : invError;

//                    Debug.Log("Error(" + i + "," + j + "): " + error[i, j] + (directMatch[i, j] ? " direct" : " inverse"));
                }
            }

            //Get best match using hungarian method? use n! method for n<4?
            return HungarianMethod(error);
        }

        protected virtual GlyphMatch FinalizeMatch(Glyph src, Glyph tgt, int[] indexMatch)
        {
            if (indexMatch == null) return null;//failed
            GlyphMatch.StrokeMatch[] matches = new GlyphMatch.StrokeMatch[indexMatch.Length];
            for (int i = 0; i < indexMatch.Length; i++)
            {
                int j = indexMatch[i];
                srcStroke = src[i]; tgtStroke = tgt[j];
                matches[i] = GetStrokeMatch(error[i, j], directMatch[i, j]);// matchMatrix[i, j];

				if (matches[i]==null) return null;
//				if (matches[i]==null){
//					Debug.Log ("Match "+i+"-"+j+" failed.");
//					return null;
//				}
//				else{
//	                float meanCost = (float)Math.Sqrt(matches[i].cost / matches[i].weight);
//	                Debug.Log("MeanCost(" + i + "," + j + "): " + matches[i].cost + " / " + matches[i].weight + " (" + meanCost + ")");
//				}
            }
            srcStroke = null; tgtStroke = null;

            float costSum = 0, weight = 0;
			foreach (GlyphMatch.StrokeMatch sm in matches) { costSum += sm.cost; weight += sm.weight; }
			GlyphMatch result = new GlyphMatch(src, tgt, matches, Mathf.Sqrt(costSum / weight), threshold);
            src = null; tgt = null;
            directMatch = null; error = null;
            return result;
        }

        protected Stroke srcStroke = null, tgtStroke = null;
        protected GlyphMatch.StrokeMatch GetStrokeMatch(float error, bool direct)
        {
            if (srcStroke.Length < 2 || tgtStroke.Length < 2) return null;
            int maxPairs = srcStroke.Length + tgtStroke.Length;

            /*
            string coeffs = "Coeffs";
            foreach (Vector2 c in srcStroke.LegendreCoefficients) coeffs += c;
            Debug.WriteLine(coeffs);
            string invCoeffs = "InvCoeffs";
            foreach (Vector2 c in srcStroke.Inverse().LegendreCoefficients) invCoeffs += c;
            Debug.WriteLine(invCoeffs); /// ICi =~ C·(-1)^i, i = 0, 1, ...
            */
            List<int> srcList = new List<int>(maxPairs), tgtList = new List<int>(maxPairs);
            if (direct)
            {
                //Direct
                int iLength = srcStroke.Length, jLength = tgtStroke.Length;
                //int lengthsProduct = iLength * jLength;
                int step = Mathf.Min(iLength, jLength);
                int progress = 0;
                for (int i = 0, j = 0; i < srcStroke.Length && j < tgtStroke.Length; i = progress / jLength, j = progress / iLength)
                {
                    srcList.Add(i); tgtList.Add(j);
                    progress += step;
                }
                return new GlyphMatch.StrokeMatch(error, srcList.Count, srcStroke, tgtStroke, srcList.ToArray(), tgtList.ToArray());
            }
            else
            {
                //Inverse
                int iLength = srcStroke.Length, jLength = tgtStroke.Length;
                //int lengthsProduct = iLength * jLength;
                int step = Mathf.Min(iLength, jLength);
                int progress = 0;
                for (int i = 0, j = 0; i < srcStroke.Length && j < tgtStroke.Length; i = progress / jLength, j = progress / iLength)
                {
                    srcList.Add(iLength - 1 - i); tgtList.Add(j);
                    progress += step;
                }
				float matchLength=srcList.Count;
                return new GlyphMatch.StrokeMatch(error*matchLength, (degree+1)*matchLength, srcStroke, tgtStroke, srcList.ToArray(), tgtList.ToArray());
            }
        }

        protected virtual float StrokeCoeffDiff(Vector2[] aCoeffs, Vector2[] bCoeffs)
        {
            float diff = 0;
            for (int i = 0; i < aCoeffs.Length; i++)
            {
                Vector2 d = aCoeffs[i] - bCoeffs[i];
                diff += d.sqrMagnitude;
            }
            return diff;
        }

        protected virtual float InvStrokeCoeffDiff(Vector2[] aCoeffs, Vector2[] bCoeffs)
        {
            float diff = 0;
            int sign = 1;
            for (int i = 0; i < aCoeffs.Length; i++)
            {
                Vector2 d = sign * aCoeffs[i] - bCoeffs[i];
                diff += d.sqrMagnitude;
                sign = -sign;
            }
            return diff;
        }

    }
}
