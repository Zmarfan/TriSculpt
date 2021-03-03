using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ImageProperties : MonoBehaviour
{
    public static Texture2D GetEntropyImage(Texture2D texture, int sampleArea, out float[,] returnEntropyTable)
    {
        Texture2D entropyTexture = new Texture2D(texture.width, texture.height);
        Color[] oldColors = texture.GetPixels();
        Color[] newColors = new Color[oldColors.Length];
        float[,] entropyTable = new float[texture.width, texture.height];

        Histogram histogram = new Histogram();

        for (int i = 0; i < oldColors.Length; i++)
        {
            int thisY = Mathf.FloorToInt(i / texture.width);
            int thisX = i - thisY * texture.width;

            //For each pixel check sample area around it to set this pixel entropy
            for (int x = -sampleArea; x <= sampleArea; x++)
            {
                //This pixel is out of bounds
                if (IsPixelOutOfBounds(thisX + x, texture.width))
                    continue;
                for (int y = -sampleArea; y <= sampleArea; y++)
                {
                    //This pixel is out of bounds
                    if (IsPixelOutOfBounds(thisY + y, texture.height))
                        continue;

                    histogram.AddPixel(oldColors[thisX + x + (thisY + y) * texture.width]);
                }
            }

            float entropy = histogram.EntropyOfHistogram();

            newColors[i] = new Color(entropy / 5f, entropy / 5f, entropy / 5f);
            entropyTable[thisX, thisY] = entropy;

            histogram.Clear();
        }

        entropyTexture.filterMode = FilterMode.Point;
        entropyTexture.SetPixels(newColors);
        entropyTexture.Apply();

        returnEntropyTable = entropyTable;
        return entropyTexture;
    }

    /// <summary>
    /// Returns true if pixel is outside of bounds (smaller than 0 or bigger/equal to maxDimension)
    /// </summary>
    /// <param name="position">pixel position in x/y</param>
    /// <param name="maxDimension">width/height</param>
    public static bool IsPixelOutOfBounds(int position, int maxDimension)
    {
        return position < 0 || position >= maxDimension;
    }

    /// <summary>
    /// Create a black and white version of the texture 
    /// </summary>
    /// <param name="texture">Original texture</param>
    public static Texture2D ConvertToBlackAndWhite(Texture2D texture)
    {
        Texture2D blackWhiteTexture = new Texture2D(texture.width, texture.height);
        Color[] oldColors = texture.GetPixels();
        Color[] newColors = new Color[oldColors.Length];

        for (int i = 0; i < oldColors.Length; i++)
        {
            float g = oldColors[i].grayscale;
            newColors[i] = new Color(g, g, g);
        }

        blackWhiteTexture.filterMode = FilterMode.Point;
        blackWhiteTexture.SetPixels(newColors);
        blackWhiteTexture.Apply();

        return blackWhiteTexture;
    }

    /// <summary>
    /// Change color depth for each pixel based on depth
    /// </summary>
    /// <param name="texture">Original texture</param>
    /// <param name="depth">1-255 how much color depth the picture should have (1 => binary, 255 => unchanged)</param>
    /// <returns>New texture with changed color depth from original texture</returns>
    public static Texture2D TextureColorDepth(Texture2D texture, int depth)
    {
        Texture2D modTexture = new Texture2D(texture.width, texture.height);
        Color[] oldColors = texture.GetPixels();
        Color[] newColors = new Color[oldColors.Length];

        //Limit each pixel precision for each r, g, b value based on depth
        for (int i = 0; i < oldColors.Length; i++)
            newColors[i] = new Color(ChangeValueColorDepth(oldColors[i].r, depth), ChangeValueColorDepth(oldColors[i].g, depth), ChangeValueColorDepth(oldColors[i].b, depth));

        modTexture.filterMode = FilterMode.Point;
        modTexture.SetPixels(newColors);
        modTexture.Apply();

        return modTexture;
    }

    static float ChangeValueColorDepth(float colorValue, float depth)
    {
        int gDepth = (int)(colorValue * depth);
        float value = (float)gDepth / depth;
        return value;
    }
}


/*

    for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                newColors[x + y * texture.width] = Color.blue;
            }
        }

*/
