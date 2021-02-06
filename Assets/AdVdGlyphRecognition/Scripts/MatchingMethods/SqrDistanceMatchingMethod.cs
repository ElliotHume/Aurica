using UnityEngine;
using System;
using System.Collections.Generic;

namespace AdVd.GlyphRecognition
{
	/// <summary>
	/// Matching method using square distance as the feature distance. 
	/// Not as good as DTW but faster. Certain deformations may lead to wrong matchings. Stroke match time cost: O(n) 
	/// </summary>
    public class SqrDistanceMatchingMethod : StrokeToStrokeMatchingMethod
    {
		public SqrDistanceMatchingMethod(float threshold = defaultThreshold)
        {
            this.threshold = threshold;
        }

        public override string Name
        {
            get { return "SqrDistance"; }
        }

        protected override GlyphMatch.StrokeMatch GetStrokeMatch()
        {
            if (srcStroke.Length < 2 || tgtStroke.Length < 2) return null;
            int maxPairs = srcStroke.Length + tgtStroke.Length;

            //Direct
            int srcIndex = 0, tgtIndex = 0;
            float error = (srcStroke[srcIndex] - tgtStroke[tgtIndex]).sqrMagnitude;
            List<int> srcList = new List<int>(maxPairs), tgtList = new List<int>(maxPairs);
            srcList.Add(srcIndex); tgtList.Add(tgtIndex);
            while (srcIndex < srcStroke.Length - 1 && tgtIndex < tgtStroke.Length - 1)
            {
                //Get shortest next
                Vector2 srcNext = srcStroke[srcIndex + 1], tgtNext = tgtStroke[tgtIndex + 1];
                float doubleAdvanceSqr = (srcNext - tgtNext).sqrMagnitude;
                float thisAdvanceSqr = (srcNext - tgtStroke[tgtIndex]).sqrMagnitude;
                float otherAdvanceSqr = (srcStroke[srcIndex] - tgtNext).sqrMagnitude;

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
                error += (srcStroke[srcIndex + 1] - tgtStroke[tgtIndex]).sqrMagnitude;
                srcIndex++;
                srcList.Add(srcIndex); tgtList.Add(tgtIndex);
            }
            while (tgtIndex < tgtStroke.Length - 1)
            {
                error += (srcStroke[srcIndex] - tgtStroke[tgtIndex + 1]).sqrMagnitude;
                tgtIndex++;
                srcList.Add(srcIndex); tgtList.Add(tgtIndex);
            }

            //Inverse
            srcIndex = srcStroke.Length - 1; tgtIndex = 0;
            float invError = (srcStroke[srcIndex] - tgtStroke[tgtIndex]).sqrMagnitude;
            List<int> invSrcList = new List<int>(maxPairs), invTgtList = new List<int>(maxPairs);
            invSrcList.Add(srcIndex); invTgtList.Add(tgtIndex);
            while (srcIndex > 0 && tgtIndex < tgtStroke.Length - 1)
            {
                //Get shortest next
                Vector2 srcNext = srcStroke[srcIndex - 1], tgtNext = tgtStroke[tgtIndex + 1];
                float doubleAdvanceSqr = (srcNext - tgtNext).sqrMagnitude;
                float thisAdvanceSqr = (srcNext - tgtStroke[tgtIndex]).sqrMagnitude;
                float otherAdvanceSqr = (srcStroke[srcIndex] - tgtNext).sqrMagnitude;

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
                invError += (srcStroke[srcIndex -1] - tgtStroke[tgtIndex]).sqrMagnitude;
                srcIndex--;
                invSrcList.Add(srcIndex); invTgtList.Add(tgtIndex);
            }
            while (tgtIndex < tgtStroke.Length - 1)
            {
                invError += (srcStroke[srcIndex] - tgtStroke[tgtIndex + 1]).sqrMagnitude;
                tgtIndex++;
                invSrcList.Add(srcIndex); invTgtList.Add(tgtIndex);
            }

            if (error < invError) return new GlyphMatch.StrokeMatch(error, srcList.Count, srcStroke, tgtStroke, srcList.ToArray(), tgtList.ToArray());
            else return new GlyphMatch.StrokeMatch(invError, invSrcList.Count, srcStroke, tgtStroke, invSrcList.ToArray(), invTgtList.ToArray());
        }
    }
}

