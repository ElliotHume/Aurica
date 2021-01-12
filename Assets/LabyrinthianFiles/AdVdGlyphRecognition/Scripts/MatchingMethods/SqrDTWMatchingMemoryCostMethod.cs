using UnityEngine;
using System;
using System.Collections.Generic;

namespace AdVd.GlyphRecognition
{
	/// <summary>
	/// DTW matching method using square distance as the feature distance. 
	/// A special feature distance that "forgives" errors is used to get the cost. Stroke match time cost: O(n^2) 
	/// </summary>
    public class SqrDTWMatchingMemoryCostMethod : SqrDistanceDTWMatchingMethod
    {
        protected float half1PlusSqrAlpha, half4TimesAlpha;
		public SqrDTWMatchingMemoryCostMethod(float alpha, float threshold = defaultThreshold) : base(threshold)
        {
            half1PlusSqrAlpha = 0.5f * (1 + alpha * alpha);
            half4TimesAlpha = 2 * alpha;
        }

        public override string Name
        {
            get { return "SqrDistance DTW Matching - SqrMemory Cost"; }
        }

        //(a-alpha·b).sqrMag + (b-alpha·a).sqrMag = [(1+alpha^2)·(a.sqrMag+b.sqrMag) - 4·alpha(a·b)] / 2;
        protected float FeatureDistance(Vector2 a, Vector2 b)
        {
            return half1PlusSqrAlpha * (a.sqrMagnitude + b.sqrMagnitude) - half4TimesAlpha * (Vector2.Dot(a, b));
        }

        protected override GlyphMatch.StrokeMatch GetStrokeMatch()
        {
            if (srcStroke.Length < 2 || tgtStroke.Length < 2) return null;

            BuildDTW();

            int srcLength_1 = srcStroke.Length - 1;
            int maxPairs = srcStroke.Length + tgtStroke.Length;
            Stack<int> srcStack = new Stack<int>(maxPairs);
            Stack<int> tgtStack = new Stack<int>(maxPairs);
            int srcIndex = srcStroke.Length - 1, tgtIndex = tgtStroke.Length - 1;
            float bestCost;
            if (directDTW[srcIndex, tgtIndex] < inverseDTW[srcIndex, tgtIndex])
            {
                bestCost = 0.5f * ((srcStroke[0] - tgtStroke[0]).sqrMagnitude + (srcStroke[srcLength_1] - tgtStroke[tgtStroke.Length - 1]).sqrMagnitude);
                Vector2 currDiff = srcStroke[srcIndex] - tgtStroke[tgtIndex];
                while (srcIndex >= 0 && tgtIndex >= 0)
                {
                    srcStack.Push(srcIndex); tgtStack.Push(tgtIndex);
                    DTWNode node = directDTW[srcIndex, tgtIndex];
                    if (node.prevNode == DTWPrev.None) break;
                    if ((node.prevNode & DTWPrev.PrevI) != DTWPrev.None) srcIndex--;
                    if ((node.prevNode & DTWPrev.PrevJ) != DTWPrev.None) tgtIndex--;
                    Vector2 prevDiff = srcStroke[srcIndex] - tgtStroke[tgtIndex];
                    bestCost += FeatureDistance(currDiff, prevDiff);
                    currDiff = prevDiff;
                }
            }
            else
            {
                bestCost = 0.5f * ((srcStroke[srcLength_1] - tgtStroke[0]).sqrMagnitude + (srcStroke[0] - tgtStroke[tgtStroke.Length - 1]).sqrMagnitude);
                Vector2 currDiff = srcStroke[srcLength_1 - srcIndex] - tgtStroke[tgtIndex];
                while (srcIndex >= 0 && tgtIndex >= 0)
                {
                    srcStack.Push(srcLength_1 - srcIndex); tgtStack.Push(tgtIndex);
                    DTWNode node = inverseDTW[srcIndex, tgtIndex];
                    if (node.prevNode == DTWPrev.None) break;
                    if ((node.prevNode & DTWPrev.PrevI) != DTWPrev.None) srcIndex--;
                    if ((node.prevNode & DTWPrev.PrevJ) != DTWPrev.None) tgtIndex--;
                    Vector2 prevDiff = srcStroke[srcLength_1 - srcIndex] - tgtStroke[tgtIndex];
                    bestCost += FeatureDistance(currDiff, prevDiff);
                    currDiff = prevDiff;
                }
            }
            directDTW = null; inverseDTW = null;

            //Find number of pairs and divide error
            int numberOfPairs = srcStack.Count;
            return new GlyphMatch.StrokeMatch(bestCost, numberOfPairs, srcStroke, tgtStroke, srcStack.ToArray(), tgtStack.ToArray());
        }
    }
}
