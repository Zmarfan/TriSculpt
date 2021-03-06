using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSampleVisualizer : MonoBehaviour
{
    [SerializeField] Slider _slider;

    [SerializeField] RectTransform _staticCorner1;
    [SerializeField] RectTransform _staticCorner2;
    [SerializeField] RectTransform _staticCorner3;

    [SerializeField] RectTransform _moveCorner1;
    [SerializeField] RectTransform _moveCorner2;
    [SerializeField] RectTransform _moveCorner3;

    private void Awake()
    {
        UpdateVisualizer();
    }

    public void UpdateVisualizer()
    {
        Vector2 center = MeshGenerator.TriangleCenter(_staticCorner1.position, _staticCorner2.position, _staticCorner3.position);
        float delta = _slider.value;

        _moveCorner1.position = Vector2.Lerp(center, _staticCorner1.position, delta);
        _moveCorner2.position = Vector2.Lerp(center, _staticCorner2.position, delta);
        _moveCorner3.position = Vector2.Lerp(center, _staticCorner3.position, delta);
    }
}
