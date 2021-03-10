using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;

public class Algorithm : MonoBehaviour
{
    static readonly int DETAIL_ACCURACY_MIN = 16;

    [Header("Settings")]

    [SerializeField] ComputeShader _computeShader;
    [SerializeField, Range(1, 256)] int _sampleArea = 4;
    [SerializeField] CameraScript _cameraScript;

    DisplayDelaunayTriangulation _displayDelaunayTriangulation;
    MeshFilter _meshFilter;

    //Parameters for Mesh Generation (odd init values to make sure a full generation is done first time)
    int _colorDepth = int.MinValue;
    int _pointAmount;
    int _amountBorderPoints;
    int _influenceLength = 15;
    float _influenceStrength = 1.0f;
    float _gradientRadiusModifier = 1.0f;

    //True if _colorDepth isn't being changed after first Generation -> no need to calculate entropy again if true
    bool _hasEntropyData = false;
    //True if _pointAmount, _amountBorderPoints, _influenceLength/Strength isn't being changed && _hasEntropyData == true
    bool _hasPointPlacementData = false;

    float[] _storedEntropy;
    List<Triangle> _triangulation;

    private void Awake()
    {
        _displayDelaunayTriangulation = GetComponentInChildren<DisplayDelaunayTriangulation>();
        _meshFilter = GetComponentInChildren<MeshFilter>();
    }

    public void NeedNewEntropy()
    {
        _hasEntropyData = false;
    }

    public void NeedNewPoints()
    {
        _hasPointPlacementData = false;
    }

    /// <summary>
    /// Set parameters based of user input to calculate mesh by correct settings
    /// </summary>
    /// <param name="detailAccuracy">How precise the algorithm can detect points of interest in the image. 0 - 64 where 64 = highest possible</param>
    /// <param name="amountOfPoints">Amount of points to make up the triangulation mesh</param>
    /// <param name="borderPointAmount">How many points per side in the border</param>
    /// <param name="pointInfluenceLength">How much influence a placed point has to having points placed close to it.</param>
    /// <param name="pointInfluenceStrength">How strong that influence is</param>
    /// <param name="cornerColorSamplePoint">How the colors should be sampled for each triangle</param>
    public void SetParameters(float detailAccuracy, float amountOfPoints, float borderPointAmount, float pointInfluenceLength, float pointInfluenceStrength, float cornerColorSamplePoint)
    {
        //Convert to 0-16 range where 0 -> high detail
        _colorDepth = DETAIL_ACCURACY_MIN - (int)detailAccuracy;
        _pointAmount = (int)amountOfPoints;
        _amountBorderPoints = (int)borderPointAmount;
        _influenceLength = (int)pointInfluenceLength;
        _influenceStrength = pointInfluenceStrength;
        _gradientRadiusModifier = cornerColorSamplePoint;
    }

    /// <summary>
    /// Generate a texture of a mesh from current input
    /// </summary>
    /// <returns></returns>
    public Texture2D Generate(Texture2D modTexture)
    {
        int width = modTexture.width;
        int height = modTexture.height;

        if (!_hasEntropyData)
            CalculateEntropyForTexture(modTexture);
        if (!_hasPointPlacementData)
            CalculateTriangulationFromEntropy(width, height);

        //If nothing has been done up until this point -> only _gradientRadiusModifier was changed
        Texture2D finalImage = CalculateFinalMeshTexture(modTexture, width, height);

        //Everything is now stored
        _hasEntropyData = true;
        _hasPointPlacementData = true;

        return finalImage;
    }

    /// <summary>
    /// Calculate Entropy for certain texture
    /// </summary>
    /// <param name="modTexture"></param>
    void CalculateEntropyForTexture(Texture2D texture)
    {
        //Modify color depth of texture to bypass compression noise
        Texture2D modTexture = texture;
        modTexture = ImageProperties.TextureColorDepth(modTexture, _colorDepth);

        _storedEntropy = new float[modTexture.width * modTexture.height];
        ComputeBuffer entropyBuffer = new ComputeBuffer(_storedEntropy.Length, sizeof(float));
        entropyBuffer.SetData(_storedEntropy);

        //Buffer compute shader writes to
        _computeShader.SetBuffer(0, "pixelEntropies", entropyBuffer);
        //Relevant variables compute shader needs

        _computeShader.SetTexture(0, "inputTexture", modTexture);
        _computeShader.SetInt("width", modTexture.width);
        _computeShader.SetInt("height", modTexture.height);
        _computeShader.SetInt("sampleArea", _sampleArea);

        //Start shader
        _computeShader.Dispatch(0, (_storedEntropy.Length + 255) / 256, 1, 1);

        //Read in computated data and release buffer
        entropyBuffer.GetData(_storedEntropy);
        entropyBuffer.Release();
    }

    /// <summary>
    /// Calculate the triangulation for final mesh from entropy data
    /// </summary>
    void CalculateTriangulationFromEntropy(int width, int height)
    {
        //High detail points from entropy
        List<Vector2> imageDetailPoints = ImageDelaunay.GenerateImageDetailPointsFromEntropy(width, height, _storedEntropy, _pointAmount, _influenceLength, _influenceStrength);
        //Border around these ^ points
        List<Vector2> border = ImageDelaunay.GenerateBorderPoints(_amountBorderPoints, width, height);
        imageDetailPoints.AddRange(border);

        //Create triangulation from these points
        Vector2 textureBounds = new Vector2(width, height);
        _triangulation = DelaunayTriangulationGenerator.GenerateDelaunayTriangulationWithPoints(imageDetailPoints, textureBounds);
    }

    Texture2D CalculateFinalMeshTexture(Texture2D originalTexture, int width, int height)
    {
        Mesh mesh = MeshGenerator.GenerateMeshFromTriangulation(_triangulation, originalTexture, _gradientRadiusModifier);
        _meshFilter.mesh = mesh;

        return _cameraScript.GetCameraTextureOfMesh(width, height);
    }
}
