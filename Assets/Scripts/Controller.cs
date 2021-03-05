﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public static readonly float OFFSET_EPSILON = 0.001f;

    [Header("Main Settings")]

    [SerializeField] int _seed;
    [SerializeField, Range(0, 5000)] private int _pointAmount;
    [SerializeField, Range(0, 100)] private int _amountBorderPoints;
    [SerializeField] private Vector2 _bounds;

    [SerializeField] Texture2D _texture;
    [SerializeField, Range(1, 256)] int _colorDepth = 256;
    [SerializeField, Range(1, 256)] int _sampleArea = 4;
    [SerializeField, Range(1, 256)] int _influenceLength = 15;
    [SerializeField, Range(0, 100)] float _influenceStrength = 1.0f;
    [SerializeField, Range(0, 5)] float _gradientRadiusModifier = 1.0f;

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

    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        Random.InitState(_seed);
        Texture2D originalTexture = _texture;
        Texture2D modTexture = originalTexture;

        if (_useBlackWhite)
            modTexture = ImageProperties.ConvertToBlackAndWhite(originalTexture);
        if (_useColorDepth)
            modTexture = ImageProperties.TextureColorDepth(modTexture, _colorDepth);
        if (_useEntropy)
            GenerateEntropyTriangulation(ref modTexture, ref originalTexture);



        if (_displayTextureState)
            _displayTexture.Display(_displayModifiedTexture ? modTexture : originalTexture);

        //List<Triangle> triangulation = DelaunayTriangulationGenerator.GenerateDelaunayTriangulatedGraph(_pointAmount, _bounds);
        //DelaunayTriangulationGenerator.RemoveTrianglesOutsideOfBounds(_bounds, ref triangulation);
        //_displayDelaunayTriangulation.Display(triangulation, _bounds);
    }

    void GenerateEntropyTriangulation(ref Texture2D modTexture, ref Texture2D texture)
    {
        modTexture = ImageProperties.GetEntropyImage(modTexture, _sampleArea, out float[,] entropyTable);

        List<Vector2> imageDetailPoints = ImageDelaunay.GenerateImageDetailPointsFromEntropy(entropyTable, _pointAmount, _influenceLength, _influenceStrength);

        List<Vector2> border = ImageDelaunay.GenerateBorderPoints(_amountBorderPoints, modTexture.width, modTexture.height);
        imageDetailPoints.AddRange(border);

        Vector2 textureBounds = new Vector2(modTexture.width * 2 + 1, modTexture.height * 2 + 1);
        List<Triangle> triangulation = DelaunayTriangulationGenerator.GenerateDelaunayTriangulationWithPoints(imageDetailPoints, textureBounds);

        _displayDelaunayTriangulation.Display(triangulation, textureBounds);
        _displayTexture.transform.position = new Vector3(textureBounds.x / 2, textureBounds.y / 2);


        Mesh mesh = MeshGenerator.GenerateMeshFromTriangulation(triangulation, texture, _gradientRadiusModifier);
        _displayMesh.DisplayMeshNow(mesh);
    }
}
