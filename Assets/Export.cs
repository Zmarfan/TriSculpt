using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class Export : MonoBehaviour
{
    [SerializeField] Environment.SpecialFolder _startFolder;
    [SerializeField] Text _filepathText;

    string _currentFilepath;

    private void Awake()
    {
        _currentFilepath = Environment.GetFolderPath(_startFolder);
        _filepathText.text = _currentFilepath;
    }
}
