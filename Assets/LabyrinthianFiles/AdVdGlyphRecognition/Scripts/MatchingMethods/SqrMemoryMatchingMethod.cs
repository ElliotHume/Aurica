using UnityEngine;
using System;
using System.Collections.Generic;

namespace AdVd.GlyphRecognition
{
	/// <summary>
	/// Matching method using  a special feature distance that "forgives" previous errors. 
	/// Not as good as DTW but faster. Slightly better than its square distance counterpart. Stroke match time cost: O(n) 
	/// </summary>
    public class SqrMemoryMatchingMethod : StrokeToStrokeMatchingMethod
    {

        protected float half1PlusSqrAlpha, half4TimesAlpha;
        public SqrMemoryMatchingMethod(float alpha, float threshold = defaultThreshold)
        {
            this.threshold = threshold;
            half1PlusSqrAlpha = 0.5f * (1 + alpha * alpha);
            half4TimesAlpha = 2 * alpha;
        }

        public override string Name
        {
            get { return "SqrMemory"; }
        }

        //(a-alpha·b).sqrMag + (b-alpha·a).sqrMag = [(1+alpha^2)·(a.sqrMag+b.sqrMag) - 4·alpha(a·b)] / 2;
        protected float FeatureDistance(Vector2 a, Vector2 b)
        {
            return half1PlusSqrAlpha * (a.sqrMagnitude + b.sqrMagnitude) - half4TimesAlpha * (Vector2.Dot(a, b));
        }

        protected override GlyphMatch.StrokeMatch GetStrokeMatch()
        {
            if (srcStroke.Length < 2 || tgtStroke.Length < 2) return null;
            int maxPairs = srcStroke.Length + tgtStroke.Length;

            //Direct
            int srcIndex = 0, tgtIndex = 0;
            float error = 0.5f * ((srcStroke[0] - tgtStroke[0]).sqrMagnitude + (srcStroke[srcStroke.Length - 1] - tgtStroke[tgtStroke.Length - 1]).sqrMagnitude);
            List<int> srcList = new List<int>(maxPairs), tgtList = new List<int>(maxPairs);
            srcList.Add(srcIndex); tgtList.Add(tgtIndex);
            while (srcIndex < srcStroke.Length - 1 && tgtIndex < tgtStroke.Length - 1)
            {
                //Get shortest next
                Vector2 currDiff = srcStroke[srcIndex] - tgtStroke[tgtIndex];
                Vector2 srcNext = srcStroke[srcIndex + 1], tgtNext = tgtStroke[tgtIndex + 1];
                float doubleAdvanceSqr = FeatureDistance(currDiff, srcNext - tgtNext);
                float thisAdvanceSqr = FeatureDistance(currDiff, srcNext - tgtStroke[tgtIndex]);
                float otherAdvanceSqr = FeatureDistance(currDiff, srcStroke[srcIndex] - tgtNext);

                if (thisAdvanceSqr < otherAdvanceSqr)
                {
                    if (thisAdvanceSqr < doubleAdvanceSqr)
                    {
                        error += thisAdvanceSqr;
                    }
                    else
                    {
                        error += doubleAdvanceSqr;
                        tgtIndex++;
                    }
                    srcIndex++;
                }
                else
                {
                    if (otherAdvanceSqr < doubleAdvanceSqr)
                    {
                        error += otherAdvanceSqr;
                    }
                    else
                    {
                        error += doubleAdvanceSqr;
                        srcIndex++;
                    }
                    tgtIndex++;
                }
                srcList.Add(srcIndex); tgtList.Add(tgtIndex);
            }
            //if *Index<*Length continiue
            while (srcIndex < srcStroke.Length - 1)
            {
                error += FeatureDistance(srcStroke[srcIndex] - tgtStroke[tgtIndex], srcStroke[srcIndex + 1] - tgtStroke[tgtIndex]);
                srcIndex++;
                srcList.Add(srcIndex); tgtList.Add(tgtIndex);
            }
            while (tgtIndex < tgtStroke.Length - 1)
            {
                error += FeatureDistance(srcStroke[srcIndex] - tgtStroke[tgtIndex], srcStroke[srcIndex] - tgtStroke[tgtIndex + 1]);
                tgtIndex++;
                srcList.Add(srcIndex); tgtList.Add(tgtIndex);
            }

            //Inverse
            srcIndex = srcStroke.Length - 1; tgtIndex = 0;
            float invError = 0.5f * ((srcStroke[srcStroke.Length - 1] - tgtStroke[0]).sqrMagnitude + (srcStroke[0] - tgtStroke[tgtStroke.Length - 1]).sqrMagnitude);
            List<int> invSrcList = new List<int>(maxPairs), invTgtList = new List<int>(maxPairs);
            invSrcList.Add(srcIndex); invTgtList.Add(tgtIndex);
            while (srcIndex > 0 && tgtIndex < tgtStroke.Length - 1)
            {
                //Get shortest next
                Vector2 currDiff = srcStroke[srcIndex] - tgtStroke[tgtIndex];
                Vector2 srcNext = srcStroke[srcIndex - 1], tgtNext = tgtStroke[tgtIndex + 1];
                float doubleAdvanceSqr = FeatureDistance(currDiff, srcNext - tgtNext);
                float thisAdvanceSqr = FeatureDistance(currDiff, srcNext - tgtStroke[tgtIndex]);
                float otherAdvanceSqr = FeatureDistance(currDiff, srcStroke[srcIndex] - tgtNext);

                if (thisAdvanceSqr < otherAdvanceSqr)
                {
                    if (thisAdvanceSqr < doubleAdvanceSqr)
                    {
                        invError += thisAdvanceSqr;
                    }
                    else
                    {
                        invError += doubleAdvanceSqr;
                        tgtIndex++;
                    }
                    srcIndex--;
                }
                else
                {
                    if (otherAdvanceSqr < doubleAdvanceSqr)
                    {
                        invError += otherAdvanceSqr;
                    }
                    else
                    {
                        invError += doubleAdvanceSqr;
                        srcIndex--;
                    }
                    tgtIndex++;
                }
                invSrcList.Add(srcIndex); invTgtList.Add(tgtIndex);
            }
            //if *Index<*Length continiue
            while (srcIndex > 0)
            {
                invError += FeatureDistance(srcStroke[srcIndex] - tgtStroke[tgtIndex], srcStroke[srcIndex - 1] - tgtStroke[tgtIndex]);
                srcIndex--;
                invSrcList.Add(srcIndex); invTgtList.Add(tgtIndex);
            }
            while (tgtIndex < tgtStroke.Length - 1)
            {
                invError += FeatureDistance(srcStroke[srcIndex] - tgtStroke[tgtIndex], srcStroke[srcIndex] - tgtStroke[tgtIndex + 1]);
                tgtIndex++;
                invSrcList.Add(srcIndex); invTgtList.Add(tgtIndex);
            }

            if (error < invError) return new GlyphMatch.StrokeMatch(error, srcList.Count, srcStroke, tgtStroke, srcList.ToArray(), tgtList.ToArray());
            else return new GlyphMatch.StrokeMatch(invError, invSrcList.Count, srcStroke, tgtStroke, invSrcList.ToArray(), invTgtList.ToArray());
        }

    }
}
