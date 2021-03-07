using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ImageDelaunay
{
    public static List<Vector2> GenerateBorderPoints(int amount, int width, int height)
    {
        List<Vector2> pointList = new List<Vector2>()
        {
            //The corners
            // 3 2
            // 0 1
            new Vector2(0, 0),
            new Vector2(width, 0),
            new Vector2(width, height),
            new Vector2(0, height)
        };

        int heightAmount = amount;
        int widthAmount = (int)((width / (float)height) * amount);

        pointList.AddRange(GenerateLinePoints(widthAmount, pointList[0], pointList[1]));
        pointList.AddRange(GenerateLinePoints(heightAmount, pointList[1], pointList[2]));
        pointList.AddRange(GenerateLinePoints(widthAmount, pointList[2], pointList[3]));
        pointList.AddRange(GenerateLinePoints(heightAmount,  pointList[3], pointList[0]));

        return pointList;
    }

    static List<Vector2> GenerateLinePoints(int amount, Vector2 start, Vector2 end)
    {
        //To account for corners
        amount += 2;

        List<Vector2> points = new List<Vector2>();
        Vector2 direction = (end - start);
        float interval = direction.magnitude / (amount - 1);
        direction = direction.normalized;

        for (int i = 1; i < amount - 1; i++)
            points.Add(direction * interval * i + start);

        return points;
    }


    /// <summary>
    /// Gives a list of most interesting pixels (highest entropy) in an image from it's entropyTable
    /// </summary>
    /// <param name="entropyTable">Entropy table of a texture</param>
    /// <param name="pointAmount">Amount of interesting points to find</param>
    /// <param name="influenceLength">How far apart the points should be</param>
    /// <param name="influenceStrength">How strong the influence is</param>
    public static List<Vector2> GenerateImageDetailPointsFromEntropy(int width, int height, float[] entropyTable, int pointAmount, int influenceLength, float influenceStrength)
    {
        List<Vector2> points = new List<Vector2>();

        for (int i = 0; i < pointAmount; i++)
        {
            FindMaxValueIndexes(in entropyTable, width, out int x, out int y);
            points.Add(new Vector2Int(x, y));
            //Lower the entropy of points around last max entropy pixel
            AffectNearbyEntropyLevels(x, y, width, height, influenceLength, influenceStrength, ref entropyTable);
        }

        return points;
    }

    /// <summary>
    /// Finds the indexes for the current highest entropy level in the table
    /// </summary>
    /// <param name="entropyTable">Table of entropy for an image pixels (x*y resolution)</param>
    /// <param name="outX">index x of max value</param>
    /// <param name="outY">index y of max value</param>
    static void FindMaxValueIndexes(in float[] entropyTable, int width, out int outX, out int outY)
    {
        float currentMax = entropyTable[0];
        outX = 0;
        outY = 0;

        for (int i = 0; i < entropyTable.Length; i++)
        {
            if (entropyTable[i] > currentMax)
            {
                currentMax = entropyTable[i];
                outY = Mathf.FloorToInt(i / width);
                outX = i - outY * width;
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
    static void AffectNearbyEntropyLevels(int thisX, int thisY, int width, int height, int influenceLength, float influenceStrength, ref float[] entropyTable)
    {
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

                float old = entropyTable[thisX + x + (thisY + y) * width];

                float affectValue = (Mathf.Abs(x) + Mathf.Abs(y)) / (float)(influenceLength * 2);
                entropyTable[thisX + x + (thisY + y) * width] *= Mathf.Clamp(affectValue * (1 / influenceStrength), 0, old);
            }
        }
    }
}
