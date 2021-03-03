using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [Header("Main Settings")]

    [SerializeField] int _seed;
    [SerializeField] private int _pointAmount;
    [SerializeField] private Vector2 _bounds;

    [SerializeField] Texture2D _texture;
    [SerializeField, Range(1, 256)] int _colorDepth = 256;
    [SerializeField, Range(1, 256)] int _sampleArea = 4;
    [SerializeField, Range(1, 256)] int _influenceLength = 15;
    [SerializeField, Range(0, 100)] float _influenceStrength = 1.0f;

    [Header("Display Settings")]

    [SerializeField] bool _displayTextureState = true;
    [SerializeField] bool _useBlackWhite = true;
    [SerializeField] bool _useColorDepth = true;
    [SerializeField] bool _useEntropy = true;

    DisplayDelaunayTriangulation _displayDelaunayTriangulation;
    DisplayTexture _displayTexture;

    private void Awake()
    {
        _displayDelaunayTriangulation = GetComponentInChildren<DisplayDelaunayTriangulation>();
        _displayTexture = GetComponentInChildren<DisplayTexture>();
    }

    private void Start()
    {
        Generate();
    }

    List<Triangle> triangulation;

    public void Generate()
    {

        Random.InitState(_seed);
        Texture2D texture = _texture;
        float[,] entropyTable = new float[texture.width, texture.height];

        if (_useBlackWhite)
            texture = ImageProperties.ConvertToBlackAndWhite(texture);
        if (_useColorDepth)
            texture = ImageProperties.TextureColorDepth(texture, _colorDepth);
        if (_useEntropy)
            texture = ImageProperties.GetEntropyImage(texture, _sampleArea, out entropyTable);

        //Points in texture coordinates
        List<Vector2> imageDetailPoints = ImageDelaunay.GenerateImageDetailPointsFromEntropy(entropyTable, _pointAmount, _influenceLength, _influenceStrength);

        Vector2 textureBounds = new Vector2(texture.width * 2 + 1, texture.height * 2 + 1);
        triangulation = DelaunayTriangulationGenerator.GenerateDelaunayTriangulationWithPoints(imageDetailPoints, textureBounds);
        _displayDelaunayTriangulation.Display(triangulation, textureBounds);

        _displayTexture.transform.position = new Vector3(textureBounds.x / 2, textureBounds.y / 2);
        if (_displayTextureState)
            _displayTexture.Display(texture);

        //List<Triangle> triangulation = DelaunayTriangulationGenerator.GenerateDelaunayTriangulatedGraph(_pointAmount, _bounds);
        //DelaunayTriangulationGenerator.RemoveTrianglesOutsideOfBounds(_bounds, ref triangulation);
        //_displayDelaunayTriangulation.Display(triangulation, _bounds);
    }
}
