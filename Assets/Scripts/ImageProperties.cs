using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;

public class ImageProperties : MonoBehaviour
{
    public static Texture2D GetEntropyImage(Texture2D texture, int sampleArea, out float[,] returnEntropyTable)
    {
        Texture2D entropyTexture = new Texture2D(texture.width, texture.height);
        Color[] oldColors = texture.GetPixels();
        Color[] newColors = new Color[oldColors.Length];
        float[,] entropyTable = new float[texture.width, texture.height];

        LookupTable<float, float> lookupTable = new LookupTable<float, float>();
        Histogram histogram = new Histogram();

        for (int i = 0; i < oldColors.Length; i++)
        {
            int thisY = Mathf.FloorToInt(i / texture.width);
            int thisX = i - thisY * texture.width;

            //For each pixel check sample area around it to set this pixel entropy
            for (int x = Mathf.Max(thisX - sampleArea, 0); x <= thisX + sampleArea && x < texture.width; x++)
            {
                for (int y = Mathf.Max(thisY - sampleArea, 0); y <= thisY + sampleArea && y < texture.height; y++)
                {
                    histogram.AddPixel(oldColors[x + y * texture.width]);
                }
            }

            float entropy = histogram.EntropyOfHistogram(lookupTable);
            histogram.Clear();

            newColors[i] = new Color(entropy / 5f, entropy / 5f, entropy / 5f);
            entropyTable[thisX, thisY] = entropy;
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

    /// <summary>
    /// Changes color depth for single value
    /// </summary>
    /// <param name="colorValue">Current color r/g/b value</param>
    /// <param name="depth">Depth of the change</param>
    static float ChangeValueColorDepth(float colorValue, float depth)
    {
        int gDepth = (int)(colorValue * depth);
        float value = (float)gDepth / depth;
        return value;
    }


    //http://wiki.unity3d.com/index.php/TextureScale

    public class ThreadData
    {
        public int start;
        public int end;
        public ThreadData(int start, int end)
        {
            this.start = start;
            this.end = end;
        }
    }

    static Color[] _textureColors;
    static Color[] _newColors;
    static int _textureWidth;
    static int _newTextureWidth;
    static float _ratioX;
    static float _ratioY;
    static int _finishCount;
    static Mutex _mutex;

    public static void ResizeTexturePoint(int newWidth, int newHeight, ref Texture2D texture)
    {
        ThreadedScale(newWidth, newHeight, false, ref texture);
    }

    public static void ResizeTextureBilinear(int newWidth, int newHeight, ref Texture2D texture)
    {
        ThreadedScale(newWidth, newHeight, true, ref texture);
    }

    static void ThreadedScale(int newWidth, int newHeight, bool useBilinear, ref Texture2D texture)
    {
        _textureColors = texture.GetPixels();
        _newColors = new Color[newWidth * newHeight];
        if (useBilinear)
        {
            _ratioX = 1.0f / ((float)newWidth / (texture.width - 1));
            _ratioY = 1.0f / ((float)newHeight / (texture.height - 1));
        }
        //Point
        else
        {
            _ratioX = (float)texture.width / newWidth;
            _ratioY = (float)texture.height / newHeight;
        }

        _textureWidth = texture.width;
        _newTextureWidth = newWidth;
        int cores = Mathf.Min(SystemInfo.processorCount, newHeight);
        int slice = newHeight / cores;

        _finishCount = 0;
        if (_mutex == null)
            _mutex = new Mutex(false);
        if (cores > 1)
        {
            ThreadData threadData;
            int i = 0;
            for (i = 0; i < cores - 1; i++)
            {
                threadData = new ThreadData(slice * i, slice * (i + 1));
                ParameterizedThreadStart ts = useBilinear ? new ParameterizedThreadStart(BilinearScale) : new ParameterizedThreadStart(PointScale);
                Thread thread = new Thread(ts);
                thread.Start(threadData);
            }
            threadData = new ThreadData(slice * i, newHeight);
            if (useBilinear)
                BilinearScale(threadData);
            else
                PointScale(threadData);
            while (_finishCount < cores)
            {
                Thread.Sleep(1);
            }
        }
        else
        {
            ThreadData threadData = new ThreadData(0, newHeight);
            if (useBilinear)
                BilinearScale(threadData);
            else
                PointScale(threadData);
        }

        texture.Resize(newWidth, newHeight);
        texture.SetPixels(_newColors);
        texture.Apply();

        _textureColors = null;
        _newColors = null;
    }

    static void BilinearScale(object obj)
    {
        ThreadData threadData = (ThreadData)obj;
        for (int y = threadData.start; y < threadData.end; y++)
        {
            int yFloor = (int)Mathf.Floor(y * _ratioY);
            int y1 = yFloor * _textureWidth;
            int y2 = (yFloor + 1) * _textureWidth;
            int yw = y * _newTextureWidth;

            for (int x = 0; x < _newTextureWidth; x++)
            {
                int xFloor = (int)Mathf.Floor(x * _ratioX);
                float xLerp = x * _ratioX - xFloor;
                _newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(_textureColors[y1 + xFloor], _textureColors[y1 + xFloor + 1], xLerp), ColorLerpUnclamped(_textureColors[y2 + xFloor], _textureColors[y2 + xFloor + 1], xLerp), y * _ratioY - yFloor);
            }
        }

        _mutex.WaitOne();
        _finishCount++;
        _mutex.ReleaseMutex();
    }

    static void PointScale(object obj)
    {
        ThreadData threadData = (ThreadData)obj;
        for (int y = threadData.start; y < threadData.end; y++)
        {
            int thisY = (int)(_ratioY * y) * _textureWidth;
            int yw = y * _newTextureWidth;
            for (int x = 0; x < _newTextureWidth; x++)
            {
                _newColors[yw + x] = _textureColors[(int)(thisY + _ratioX * x)];
            }
        }

        _mutex.WaitOne();
        _finishCount++;
        _mutex.ReleaseMutex();
    }

    static Color ColorLerpUnclamped(Color c1, Color c2, float value)
    {
        return new Color(c1.r + (c2.r - c1.r) * value, c1.g + (c2.g - c1.g) * value, c1.b + (c2.b - c1.b) * value, c1.a + (c2.a - c1.a) * value);
    }
}
