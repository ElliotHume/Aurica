using UnityEngine;

namespace AdVd.GlyphRecognition
{
	/// <summary>
	/// Base matching method class with standard MultiMatch method and HungarianMethod implemented. 
	/// </summary>
    public abstract class MatchingMethod
    {
		/// <summary>
		/// Try to match the specified glyphs. Returns null if the match fails.
		/// </summary>
		/// <param name="src">Source.</param>
		/// <param name="tgt">Target.</param>
        public abstract GlyphMatch Match(Glyph src, Glyph tgt);

		/// <summary>
        /// Try to match a glyph with multiple targets and get the best match.
        /// </summary>
        /// <returns>The index of the best match, or -1 if there is no match.</returns>
        /// <param name="src">Source.</param>
        /// <param name="targets">Targets.</param>
        /// <param name="bestMatch">Best match info, or null if there is no match.</param>
        public virtual int MultiMatch(Glyph src, Glyph[] targets, out GlyphMatch bestMatch)
        {
            float bestDiff = float.PositiveInfinity;
            int bestIndex = -1;
            bestMatch = null;
            for (int targetIndex = 0; targetIndex < targets.Length; targetIndex++)
            {
                Glyph target = targets[targetIndex];
				if (target==null) continue;
                GlyphMatch match = Match(src, target);

                if (match != null && match.Cost < bestDiff)
                {
                    bestDiff = match.Cost;
                    bestIndex = targetIndex;
                    bestMatch = match;
                }
            }
            return bestIndex;
        }

		/// <summary>
		/// The max cost threshold of a valid match. 
		/// </summary>
		public float threshold;
        
		/// <summary>
		/// Gets the name of the method.
		/// </summary>
		/// <value>The name of the method.</value>
        public abstract string Name { get; }

		/// <summary>
		/// Perform the Hungarian method with a square cost matrix.
		/// </summary>
		/// <returns>The best match.</returns>
		/// <param name="costMatrix">Cost matrix.</param>
        static public int[] HungarianMethod(float[,] costMatrix)
        {
            int dim = costMatrix.GetLength(0);
            int[] rowZeroCount = new int[dim], columnZeroCount = new int[dim];
            //Get Labels
            float[] iLabel = new float[dim], jLabel = new float[dim];
            for (int i = 0; i < dim; i++)
            {
                iLabel[i] = costMatrix[i, 0];
                for (int j = 1; j < dim; j++)
                {
                    if (costMatrix[i, j] < iLabel[i]) iLabel[i] = costMatrix[i, j];
                }
            }
            for (int j = 0; j < dim; j++)
            {
                jLabel[j] = costMatrix[0, j] - iLabel[0];
                for (int i = 1; i < dim; i++)
                {
                    float cost = costMatrix[i, j] - iLabel[i];
                    if (cost < jLabel[j]) jLabel[j] = cost;
                }
                for (int i = 0; i < dim; i++)
                {
                    if (costMatrix[i, j] == 0)
                    {
                        rowZeroCount[i]++; columnZeroCount[j]++;
                    }
                }
            }

            //Greedy Assignment
            int[] matchedRowByColumn = new int[dim], matchedColumnByRow = new int[dim];
            for (int index = 0; index < dim; index++) { matchedColumnByRow[index] = -1; matchedRowByColumn[index] = -1; }//index can be i or j
            int firstUnassignedRow = -1;
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    if (costMatrix[i, j] == iLabel[i] + jLabel[j] && matchedColumnByRow[i] == -1 && matchedRowByColumn[j] == -1)
                    {
                        matchedColumnByRow[i] = j; matchedRowByColumn[j] = i;
                    }
                }
                if (matchedColumnByRow[i] == -1 && firstUnassignedRow == -1) firstUnassignedRow = i;
            }

            //Assign unassigned rows
            while (firstUnassignedRow != -1)
            {
                float[] minSlackValues = new float[dim];
                int[] minSlackRows = new int[dim];
                bool[] commitedRows = new bool[dim];
                int[] commitedColumnsParentRow = new int[dim];
                for (int j = 0; j < dim; j++)
                {
                    minSlackValues[j] = costMatrix[firstUnassignedRow, j] - iLabel[firstUnassignedRow] - jLabel[j];
                    minSlackRows[j] = firstUnassignedRow; commitedColumnsParentRow[j] = -1;//Not commited
                }
                for (int it = 0; it < dim; it++)//Limit loop
                {
                    //Get min slack
                    int minSlackRow = -1, minSlackColumn = -1;
                    float minSlackValue = float.PositiveInfinity;
                    for (int j = 0; j < dim; j++)
                    {
                        if (commitedColumnsParentRow[j] == -1 && minSlackValues[j] < minSlackValue)
                        {
                            minSlackValue = minSlackValues[j]; minSlackRow = minSlackRows[j]; minSlackColumn = j;
                        }
                    }
					if (minSlackColumn==-1) return null;//All minSlackValues are +Inf? return no solution
                    if (minSlackValue > 0)//Create zero
                    {
                        for (int index = 0; index < dim; index++)//index can be i or j
                        {
                            if (commitedRows[index]) iLabel[index] += minSlackValue;//Row labels increased (cost decreased)
                            if (commitedColumnsParentRow[index] != -1) jLabel[index] -= minSlackValue;//Column labels decreased
                            else minSlackValues[index] -= minSlackValue;
                        }
                    }
                    commitedColumnsParentRow[minSlackColumn] = minSlackRow;//Column commited
                    if (matchedRowByColumn[minSlackColumn] != -1)//Column already matched. Commit row match and update slacks.
                    {
                        //Debug.WriteLine(it+" Column already matched: " + minSlackColumn);
                        int matchedRow = matchedRowByColumn[minSlackColumn]; commitedRows[matchedRow] = true;
                        for (int j = 0; j < dim; j++)
                        {
                            if (commitedColumnsParentRow[j] == -1)//Not commited. Update slack
                            {
                                float slack = costMatrix[matchedRow, j] - iLabel[matchedRow] - jLabel[j];
                                if (slack < minSlackValues[j]) { minSlackValues[j] = slack; minSlackRows[j] = matchedRow; }
                            }
                        }
                    }
                    else//Re-match all commited. Number of matches should be increased (by 1?)
                    {
                        //Debug.WriteLine(it + " start rematching: " + minSlackColumn);
                        int commitedColumn = minSlackColumn;
                        int parentRow = commitedColumnsParentRow[commitedColumn];
                        for (int it2 = 0; it2 < dim; it2++)//Limit loop
                        {
                            int temp = matchedColumnByRow[parentRow];
                            matchedColumnByRow[parentRow] = commitedColumn; matchedRowByColumn[commitedColumn] = parentRow;
                            //Debug.WriteLine("  " + it2 + " rematching " + parentRow + "-" + commitedColumn + " next=" + temp);
                            commitedColumn = temp;
                            if (commitedColumn == -1) break;
                            parentRow = commitedColumnsParentRow[commitedColumn];
                        }
                        break;
                    }
                    //Debug.WriteLine("unassigned row: " + firstUnassignedRow + " it " + it);
                }
                //Get first unassigned
                int previousRow = firstUnassignedRow;
                firstUnassignedRow = -1;
                for (int i = 0; i < dim; i++)
                {
                    if (matchedColumnByRow[i] == -1) { firstUnassignedRow = i; break; }
                }
                if (firstUnassignedRow <= previousRow) break;//Either firstUnassignedRow did not increase (exit inf loop) or -1 (end of loop)
            }
//            string result = "";
//            for (int i = 0; i < dim; i++) result += "(" + i + "," + matchedColumnByRow[i] + ") ";
//            Debug.Log("Matches: " + result);
            if (firstUnassignedRow != -1) return null;//Didn't end well.
            return matchedColumnByRow;
        }
    }
}
