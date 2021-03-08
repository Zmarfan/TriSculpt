﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;

public class Controller : MonoBehaviour
{
    [Header("Shader")]

    [SerializeField] ComputeShader _computeShader;

    [Header("Main Settings")]

    [SerializeField, Range(0, 5000)] private int _pointAmount;
    [SerializeField, Range(0, 100)] private int _amountBorderPoints;
    [SerializeField, Range(1, 256)] int _colorDepth = 256;
    [SerializeField, Range(1, 256)] int _influenceLength = 15;
    [SerializeField, Range(0, 100)] float _influenceStrength = 1.0f;
    [SerializeField, Range(0, 1)] float _gradientRadiusModifier = 1.0f;

    [Header("No user interference needed")]

    [SerializeField, Range(1, 256)] int _sampleArea = 4;
    [SerializeField] CameraScript _cameraScript;

    [Header("Display Settings")]

    [SerializeField] bool _displayTextureState = true;
    [SerializeField] bool _displayModifiedTexture = true;
    [SerializeField] bool _useBlackWhite = true;
    [SerializeField] bool _useColorDepth = true;
    [SerializeField] bool _useEntropy = true;

    DisplayDelaunayTriangulation _displayDelaunayTriangulation;
    DisplayTexture _displayTexture;
    DisplayMesh _displayMesh;

    private void Awake()
    {
        _displayDelaunayTriangulation = GetComponentInChildren<DisplayDelaunayTriangulation>();
        _displayTexture = GetComponentInChildren<DisplayTexture>();
        _displayMesh = GetComponentInChildren<DisplayMesh>();
    }


    /// <summary>
    /// Generate a texture of a mesh from current input
    /// </summary>
    /// <returns></returns>
    public Texture2D Generate(Texture2D texture)
    {
        Texture2D originalTexture = texture;
        Texture2D modTexture = originalTexture;

        modTexture = ImageProperties.TextureColorDepth(modTexture, _colorDepth);

        float[] pixelEntropies = new float[modTexture.width * modTexture.height];

        ComputeBuffer entropyBuffer = new ComputeBuffer(pixelEntropies.Length, sizeof(float));
        entropyBuffer.SetData(pixelEntropies);

        //Buffer compute shader writes to
        _computeShader.SetBuffer(0, "pixelEntropies", entropyBuffer);
        //Relevant variables compute shader needs

        _computeShader.SetTexture(0, "inputTexture", modTexture);
        _computeShader.SetInt("width", modTexture.width);
        _computeShader.SetInt("height", modTexture.height);
        _computeShader.SetInt("sampleArea", _sampleArea);

        //Start shader
        _computeShader.Dispatch(0, (pixelEntropies.Length + 255) / 256, 1, 1);

        //Read in computated data and release buffer
        entropyBuffer.GetData(pixelEntropies);
        entropyBuffer.Release();

        List<Vector2> imageDetailPoints = ImageDelaunay.GenerateImageDetailPointsFromEntropy(modTexture.width, modTexture.height, pixelEntropies, _pointAmount, _influenceLength, _influenceStrength);


        List<Vector2> border = ImageDelaunay.GenerateBorderPoints(_amountBorderPoints, modTexture.width, modTexture.height);
        imageDetailPoints.AddRange(border);
        //imageDetailPoints = border;

        Vector2 textureBounds = new Vector2(modTexture.width, modTexture.height);
        List<Triangle> triangulation = DelaunayTriangulationGenerator.GenerateDelaunayTriangulationWithPoints(imageDetailPoints, textureBounds);

        _displayDelaunayTriangulation.Display(triangulation, textureBounds);
        _displayTexture.transform.position = new Vector3(textureBounds.x / 2, textureBounds.y / 2);


        Mesh mesh = MeshGenerator.GenerateMeshFromTriangulation(triangulation, originalTexture, _gradientRadiusModifier);
        _displayMesh.DisplayMeshNow(mesh);

        Texture2D meshTexture = _cameraScript.SetCameraToTexture(textureBounds.x, textureBounds.y);

        return meshTexture;
    }
}
