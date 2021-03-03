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

    [Header("Display Settings")]

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

    public void Generate()
    {
        Random.InitState(_seed);
        Texture2D texture = _texture;
        if (_useBlackWhite)
            texture = ImageProperties.ConvertToBlackAndWhite(texture);
        if (_useColorDepth)
            texture = ImageProperties.TextureColorDepth(texture, _colorDepth);
        if (_useEntropy)
            texture = ImageProperties.GetEntropyImage(texture, _sampleArea);
        _displayTexture.Display(texture);

        //List<Triangle> triangulation = DelaunayTriangulationGenerator.GenerateDelaunayTriangulatedGraph(_pointAmount, _bounds);
        //DelaunayTriangulationGenerator.RemoveTrianglesOutsideOfBounds(_bounds, ref triangulation);
        //_displayDelaunayTriangulation.Display(triangulation, _bounds);
    }
}
