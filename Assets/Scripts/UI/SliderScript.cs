using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderScript : MonoBehaviour
{
    [SerializeField] Slider _thisSlider;
    [SerializeField] TMP_Text _sliderValueText;

    private void Awake()
    {
        UpdateSlider();
    }

    /// <summary>
    /// Called when value of the slider has been changed by user
    /// </summary>
    public void UpdateSlider()
    {
        _sliderValueText.text = _thisSlider.value.ToString("0.##").Replace(',', '.');
    }
}
