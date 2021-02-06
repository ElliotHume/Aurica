using UnityEngine;
using System;
using System.Collections.Generic;

namespace AdVd.GlyphRecognition
{
	/// <summary>
	/// Legendre-Sobolev series coefficients generator.
	/// </summary>
    public class LegendreSobolevSeries : LegendreSeries
    {
        float mu = 1f;
        public LegendreSobolevSeries(int degree, float mu = 1f) : base(degree)
        {
            this.mu = mu;
        }

        protected override float LegendreSqrNorm(int k)
        {
            return base.LegendreSqrNorm(k) + mu * (k + 1) * k;
        }

        protected override float PolyInnerProduct (int polyA, int polyB) //Skips half of the coefficients because they will equal 0
        {
            float baseInnerProduct = base.PolyInnerProduct(polyA, polyB);
            if (polyA == 0 || polyB == 0) return baseInnerProduct;// + mu * 0;
            float[] derivedLegendreA = new float[polyA];//polyA polynomial has polyA+1 elements
            float[] derivedLegendreB = new float[polyB];
            for (int i = (polyA - 1) % 2; i < polyA; i += 2) derivedLegendreA[i] = legendrePolynomials[polyA][i + 1] * (i + 1);//Skips 0s
            for (int i = (polyB - 1) % 2; i < polyB; i += 2) derivedLegendreB[i] = legendrePolynomials[polyB][i + 1] * (i + 1);//Skips 0s
            polyA--; polyB--;
            int productDegree = polyA + polyB;//degree of poly* = index of poly*
            int minDegreeTerm = productDegree % 2;
            int minDegreeTermOfA = polyA % 2;
            float result = 0;
            for (int index = minDegreeTerm; index <= productDegree; index += 2)
            {
                //Integration of term index
                float xToIndexInt = 2f * ((index + 1) % 2) / (index + 1); // Only these are orthogonal [-1,1]
                //float xToIndexInt = 1f / (index + 1);//Math.Pow(1, index + 1) / (index + 1);// Equals 1/(index+1) // [0,1]

                int lastBIndex = Math.Min(polyB, index - minDegreeTermOfA);
                int firstAIndex = index - lastBIndex;
                float termCoeff = 0; 
                for (int aIndex = firstAIndex, bIndex = lastBIndex; aIndex <= polyA && bIndex >= 0; aIndex += 2, bIndex -= 2)
                {
                    termCoeff += derivedLegendreA[aIndex] * derivedLegendreB[bIndex];
                }
                result += termCoeff * xToIndexInt;
                //Debug.WriteLine("{{" + index + " " + firstAIndex + " " + lastBIndex + " " + termCoeff + "*" + xToIndexInt + "}}");
            }
            return baseInnerProduct + mu * result; //Doesn't look orthogonal, but the results seem usable
        }

        protected override void GetMoments(Stroke stroke)
        {
            Reset();
            int length_1 = stroke.Length - 1;
            float step = 2f / length_1;
            float t = -1;
            foreach (Vector2 point in stroke)
            {
                float tPowers = 1; // t = numberOfPoints;
                for (int i = 0; i <= degree; i++)
                {
                    float tFactor = tPowers - mu * i * (i - 1) * tPowers / (t * t); //may cause problems if tFactor becomes NaN when t=0
                    if (float.IsNaN(tFactor)) tFactor = tPowers;
                    xMomentIntegrals[i] += tFactor * point.x;//Sum([t^i - mu·i·(i-1)·t^(i-2)] * x(t))
                    yMomentIntegrals[i] += tFactor * point.y;//Sum([t^i - mu·i·(i-1)·t^(i-2)] * y(t))
                    tPowers *= t;
                }
                t += step;
            }
            float minus1PowTMinus1 = -1f; // plus1Pow = 1f always
            for (int i = 0; i <= degree; i++)
            {
                float muTimesI = mu * i;
                xMomentIntegrals[i] += muTimesI * (stroke[length_1].x - minus1PowTMinus1 * stroke[0].x);//mu·i * [x(1)·1^(i-1) - x(-1)·(-1)^(i-1)]
                yMomentIntegrals[i] += muTimesI * (stroke[length_1].y - minus1PowTMinus1 * stroke[0].y);//mu·i * [y(1)·1^(i-1) - y(-1)·(-1)^(i-1)]
                minus1PowTMinus1 = -minus1PowTMinus1;
            }
            //Debug.WriteLine("t=" + (t - step) + "=1?");

            //string moments = "moments:";
            //for (int k = 0; k <= degree; k++) moments += " (" + xMomentIntegrals[k] + "," + yMomentIntegrals[k] + ")";
            //Debug.WriteLine("[-1,1] " + moments);
        }

        public override string ToString()
        {
            return "Legendre-Sobolev Series(" + degree + ", " + mu + ")";
        }

    }
}
