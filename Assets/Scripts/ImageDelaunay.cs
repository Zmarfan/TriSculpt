using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ImageDelaunay
{
    /// <summary>
    /// Gives a list of most interesting pixels (highest entropy) in an image from it's entropyTable
    /// </summary>
    /// <param name="entropyTable">Entropy table of a texture</param>
    /// <param name="pointAmount">Amount of interesting points to find</param>
    /// <param name="influenceLength">How far apart the points should be</param>
    /// <param name="influenceStrength">How strong the influence is</param>
    public static List<Vector2> GenerateImageDetailPointsFromEntropy(float[,] entropyTable, int pointAmount, int influenceLength, float influenceStrength)
    {
        int width = entropyTable.GetLength(0);
        int height = entropyTable.GetLength(1);

        List<Vector2> points = new List<Vector2>();

        for (int i = 0; i < pointAmount; i++)
        {
            FindMaxValueIndexes(in entropyTable, out int x, out int y);
            points.Add(new Vector2Int(x + width / 2, y + height / 2));
            //Lower the entropy of points around last max entropy pixel
            AffectNearbyEntropyLevels(x, y, influenceLength, influenceStrength, ref entropyTable);
        }

        return points;
    }

    /// <summary>
    /// Finds the indexes for the current highest entropy level in the table
    /// </summary>
    /// <param name="entropyTable">Table of entropy for an image pixels (x*y resolution)</param>
    /// <param name="outX">index x of max value</param>
    /// <param name="outY">index y of max value</param>
    static void FindMaxValueIndexes(in float[,] entropyTable, out int outX, out int outY)
    {
        float currentMax = entropyTable[0, 0];
        outX = 0;
        outY = 0;

        for (int x = 0; x < entropyTable.GetLength(0); x++)
        {
            for (int y = 0; y < entropyTable.GetLength(1); y++)
            {
                if (entropyTable[x, y] > currentMax)
                {
                    currentMax = entropyTable[x, y];
                    outX = x;
                    outY = y;
                }
            }
        }
    }

    /// <summary>
    /// Lower entropy level of surrounding pixels and set current to 0
    /// </summary>
    /// <param name="thisX">This pixel x coord</param>
    /// <param name="thisY">This pixel y coord</param>
    /// <param name="influenceLength">How far around the pixel affect entropy</param>
    /// <param name="influenceStrength">How strongly should surrounding pixels be affected</param>
    static void AffectNearbyEntropyLevels(int thisX, int thisY, int influenceLength, float influenceStrength, ref float[,] entropyTable)
    {
        int width = entropyTable.GetLength(0);
        int height = entropyTable.GetLength(1);

        for (int x = -influenceLength; x <= influenceLength; x++)
        {
            //This index is out of bounds
            if (thisX + x < 0 || thisX + x >= width)
                continue;

            for (int y = -influenceLength; y <= influenceLength; y++)
            {
                //This index is out of bounds
                if (thisY + y < 0 || thisY + y >= height)
                    continue;

                float old = entropyTable[thisX + x, thisY + y];

                float affectValue = (Mathf.Abs(x) + Mathf.Abs(y)) / (float)(influenceLength * 2);
                entropyTable[thisX + x, thisY + y] *= Mathf.Clamp(affectValue * (1 / influenceStrength), 0, 1);

                //Debug.Log("x: " + x + ", y: " + y + ", old Entropy: " + old + ", new Entropy: " + entropyTable[thisX + x, thisY + y] + ", affect value: " + affectValue);
            }
        }
    }
}
