using UnityEngine;
using System;
using System.Collections.Generic;

namespace AdVd.GlyphRecognition
{
	/// <summary>
	/// Legendre series coefficients generator.
	/// </summary>
    public class LegendreSeries
    {
        protected int degree;
        protected float[][] legendrePolynomials;
        float[] legendreSqrNorms;
        public LegendreSeries(int degree)
        {
            if (degree < 2 || degree > 20) throw new ArgumentException("Legendre Series degree has been limited between 2 and 20.");
            this.degree = degree;


            legendrePolynomials = new float[degree + 1][];
            legendrePolynomials[0] = new float[] { 1f };
            legendrePolynomials[1] = new float[] { 0f, 1f };
            for (int k = 2; k <= degree; k++)
            {
                //Legendre
                legendrePolynomials[k] = new float[k + 1];
                legendrePolynomials[k][0] = (-(k - 1) * legendrePolynomials[k - 2][0]) / k;
//                string coeffs = "Coeffs[" + k + "]: " + legendrePolynomials[k][0];
                for (int i = 1; i <= k; i++)
                {
                    float Pk_2i = (i <= k - 2 ? legendrePolynomials[k - 2][i] : 0f);
                    legendrePolynomials[k][i] = ((2 * k - 1) * legendrePolynomials[k - 1][i - 1] - (k - 1) * Pk_2i) / k;
//                    coeffs += " " + legendrePolynomials[k][i];
                }
//                Debug.Log(coeffs);//Seems right
            }
        }

        protected virtual float LegendreSqrNorm(int k)
        {
            return 2.0f / (2.0f * k + 1.0f);
        }

        protected virtual float PolyInnerProduct (int polyA, int polyB) //Skips half of the coefficients because they will equal 0
        {
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
                    termCoeff += legendrePolynomials[polyA][aIndex] * legendrePolynomials[polyB][bIndex];
                }
                result += termCoeff * xToIndexInt;
                //Debug.WriteLine("{{" + index + " " + firstAIndex + " " + lastBIndex + " " + termCoeff + "*" + xToIndexInt + "}}");
            }
            return result;
        }

        protected float[] xMomentIntegrals;
        protected float[] yMomentIntegrals;
        protected void Reset()
        {
            xMomentIntegrals = new float[degree + 1];
            yMomentIntegrals = new float[degree + 1];
        }
		/// <summary>
		/// Initialize this instance. Initialize once before using Compute().
		/// </summary>
        public void Init()
        {
            Reset();

            legendreSqrNorms = new float[degree + 1];
            for (int k = 0; k <= degree; k++)
            {
                legendreSqrNorms[k] = LegendreSqrNorm(k);
                //legendreSqrNorms[k] = PolyInnerProduct(k, k);//Can't call virtuals in ctor
            }


            //Check orthogonality //Orthogonal for [-1,1]
            /*
            for (int i = 0; i <= degree; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    float pip = PolyInnerProduct(i, j);
                    if (pip > 1e-4 || pip < -1e-4) Debug.Log("<P" + i + "(x),P" + j + "(x)> = " + pip + " ");
                }
				Debug.Log("|P"+i+"(x)|^2 =~ " + legendreSqrNorms[i]);
            }
            */
        }


        protected virtual void GetMoments(Stroke stroke)
        {
            Reset();
            float step = 2f / (stroke.Length - 1);
            float t = -1;
            foreach (Vector2 point in stroke)
            {
                float tPowers = 1; // t = numberOfPoints;
                for (int i = 0; i <= degree; i++)
                {
                    xMomentIntegrals[i] += tPowers * point.x;//Sum(t^i * x(t))
                    yMomentIntegrals[i] += tPowers * point.y;//Sum(t^i * y(t))
                    tPowers *= t;
                }
                t += step;
            }
            //Debug.WriteLine("t=" + (t-step) + "=1?");

            //string moments = "moments:";
            //for (int k = 0; k <= degree; k++) moments += " (" + xMomentIntegrals[k] + "," + yMomentIntegrals[k] + ")";
            //Debug.WriteLine("[-1,1] " + moments);
        }

		/// <summary>
		/// Compute the coefficients for the specified stroke. 
		/// The coefficients of the inverse stroke can be obtained as: ICi =~ Ci·(-1)^i, i = 0, 1, ...
		/// </summary>
		/// <param name="stroke">Stroke.</param>
        public Vector2[] Compute(Stroke stroke)
        {
            GetMoments(stroke);
            Vector2[] coeffs = new Vector2[xMomentIntegrals.Length];
//            string coeffsString = "Coeffs:";
            for (int k = 0; k <= degree; k++) //Coeffs obtention (Basis transform)
            {
                for (int i = 0; i <= k; i++)
                {
                    coeffs[k] += new Vector2(xMomentIntegrals[i], yMomentIntegrals[i]) * legendrePolynomials[k][i];
                }
                coeffs[k] /= legendreSqrNorms[k]; //May also work up in interval change
//                coeffsString += " " + coeffs[k];
            }
//            Debug.Log(this + " " + coeffsString);
            return coeffs;
        }
		/// <summary>
		/// Gets the degree of the series.
		/// </summary>
		/// <value>The degree.</value>
        public int Degree { get { return degree; } }

        public override string ToString()
        {
            return "Legendre Series(" + degree + ")";
        }
    }
}
