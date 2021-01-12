using UnityEngine;
using System;
using System.Collections.Generic;

namespace AdVd.GlyphRecognition
{
	/// <summary>
	/// DTW matching method using square distance as the feature distance. Stroke match time cost: O(n^2)
	/// </summary>
    public class SqrDistanceDTWMatchingMethod : StrokeToStrokeMatchingMethod // Dynamic Time Warping = Elastic Matching
    {
		public SqrDistanceDTWMatchingMethod(float threshold = defaultThreshold)//meanSqrError -> large errors weight more
        {
            this.threshold = threshold;
        }

        public override string Name
        {
            get { return "DTW SqrDistance"; }
        }

        protected DTWNode[,] directDTW, inverseDTW;
        protected virtual void BuildDTW()
        {
            int srcLength_1 = srcStroke.Length - 1;
            directDTW = new DTWNode[srcStroke.Length, tgtStroke.Length];
            inverseDTW = new DTWNode[srcStroke.Length, tgtStroke.Length];
            directDTW[0, 0] = new DTWNode((srcStroke[0] - tgtStroke[0]).sqrMagnitude, DTWPrev.None);
            inverseDTW[0, 0] = new DTWNode((srcStroke[srcLength_1] - tgtStroke[0]).sqrMagnitude, DTWPrev.None);
            for (int i = 1; i < srcStroke.Length; i++)
            {
                directDTW[i, 0] = new DTWNode(directDTW[i - 1, 0], DTWPrev.PrevI) + (srcStroke[i] - tgtStroke[0]).sqrMagnitude;
                inverseDTW[i, 0] = new DTWNode(inverseDTW[i - 1, 0], DTWPrev.PrevI) + (srcStroke[srcLength_1 - i] - tgtStroke[0]).sqrMagnitude;
            }
            for (int j = 1; j < tgtStroke.Length; j++)
            {
                directDTW[0, j] = new DTWNode(directDTW[0, j - 1], DTWPrev.PrevJ) + (srcStroke[0] - tgtStroke[j]).sqrMagnitude;
                inverseDTW[0, j] = new DTWNode(inverseDTW[0, j - 1], DTWPrev.PrevJ) + (srcStroke[srcLength_1] - tgtStroke[j]).sqrMagnitude;
                for (int i = 1; i < srcStroke.Length; i++)
                {
                    directDTW[i, j] = new DTWNode(directDTW[i - 1, j], directDTW[i, j - 1], directDTW[i - 1, j - 1]) + (srcStroke[i] - tgtStroke[j]).sqrMagnitude;
                    inverseDTW[i, j] = new DTWNode(inverseDTW[i - 1, j], inverseDTW[i, j - 1], inverseDTW[i - 1, j - 1]) + (srcStroke[srcLength_1 - i] - tgtStroke[j]).sqrMagnitude;
                }
            }
        }
        protected override GlyphMatch.StrokeMatch GetStrokeMatch()
        {
            if (srcStroke.Length < 2 || tgtStroke.Length < 2) return null;

            BuildDTW();

            int maxPairs = srcStroke.Length + tgtStroke.Length;
            Stack<int> srcStack = new Stack<int>(maxPairs);
            Stack<int> tgtStack = new Stack<int>(maxPairs);
            int srcIndex = srcStroke.Length - 1, tgtIndex = tgtStroke.Length - 1;
            float bestCost;
            if (directDTW[srcIndex, tgtIndex] < inverseDTW[srcIndex, tgtIndex])
            {
                bestCost = directDTW[srcIndex, tgtIndex];
                while (srcIndex >= 0 && tgtIndex >= 0)
                {
                    srcStack.Push(srcIndex); tgtStack.Push(tgtIndex);
                    DTWNode node = directDTW[srcIndex, tgtIndex];
                    if (node.prevNode == DTWPrev.None) break;
                    if ((node.prevNode & DTWPrev.PrevI) != DTWPrev.None) srcIndex--;
                    if ((node.prevNode & DTWPrev.PrevJ) != DTWPrev.None) tgtIndex--;
                }
            }
            else
            {
                int srcLength_1 = srcStroke.Length - 1;
                bestCost = inverseDTW[srcIndex, tgtIndex];
                while (srcIndex >= 0 && tgtIndex >= 0)
                {
                    srcStack.Push(srcLength_1 - srcIndex); tgtStack.Push(tgtIndex);
                    DTWNode node = inverseDTW[srcIndex, tgtIndex];
                    if (node.prevNode == DTWPrev.None) break;
                    if ((node.prevNode & DTWPrev.PrevI) != DTWPrev.None) srcIndex--;
                    if ((node.prevNode & DTWPrev.PrevJ) != DTWPrev.None) tgtIndex--;
                }
            }
            directDTW = null; inverseDTW = null;

            //Find number of pairs and divide error
            int numberOfPairs = srcStack.Count;
            return new GlyphMatch.StrokeMatch(bestCost, numberOfPairs, srcStroke, tgtStroke, srcStack.ToArray(), tgtStack.ToArray());
        }


        protected enum DTWPrev : byte { None = 0, PrevI, PrevJ, PrevIJ };// I(1) + J(2) = IJ(3)
        protected struct DTWNode
        {
            public float cost;
            public DTWPrev prevNode;
            static public implicit operator float(DTWNode n)
            {
                return n.cost;
            }
            static public DTWNode operator +(DTWNode node, float c)
            {
                node.cost += c;
                return node;
            }
            public DTWNode(float cost, DTWPrev prevNode = DTWPrev.None)
            {
                this.cost = cost; this.prevNode = prevNode;
            }
            public DTWNode(float costPI, float costPJ, float costPIJ)
            {
                if (costPI < costPJ)
                {
                    if (costPIJ < costPI) { this.cost = costPIJ; this.prevNode = DTWPrev.PrevIJ; }
                    else { this.cost = costPI; this.prevNode = DTWPrev.PrevI; }
                }
                else
                {
                    if (costPIJ < costPJ) { this.cost = costPIJ; this.prevNode = DTWPrev.PrevIJ; }
                    else { this.cost = costPJ; this.prevNode = DTWPrev.PrevJ; }
                }
            }
        }
    }
}
