﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class UIScript : MonoBehaviour
{
    [Header("Settings")]

    [SerializeField]
    List<string> _validImageTypes = new List<string>() { ".png", ".jpg" };

    [Header("Drop")]

    [SerializeField] Controller _controllerScript;
    [SerializeField] GameObject _hideSettingsObject;
    [SerializeField] GameObject _fileExplorerPrefab;
    [SerializeField] Transform _fileExplorerHolder;

    HashSet<string> _validImageTypeHashSet = new HashSet<string>();
    FileExplorer.FileExplorer _currentFileExplorer;

    private void Awake()
    {
        foreach (string type in _validImageTypes)
            _validImageTypeHashSet.Add(type);
    }

    /// <summary>
    /// Called when user presses to load in image -> start fileexplorer to load in image
    /// </summary>
    public void OpenFileExplorer()
    {
        GameObject obj = Instantiate(_fileExplorerPrefab, Vector3.zero, Quaternion.identity, _fileExplorerHolder) as GameObject;
        _currentFileExplorer = obj.GetComponent<FileExplorer.FileExplorer>();
        _currentFileExplorer.OpenFile += OpenFile;
    }

    /// <summary>
    /// Called when an item is selected in file explorer when looking for image
    /// </summary>
    /// <param name="name">Name of file</param>
    /// <param name="extension">Type of the file</param>
    /// <param name="filePath">Filepath to this file</param>
    void OpenFile(string name, string extension, string filePath)
    {
        if (IsValidFile(extension))
        {
            _currentFileExplorer.OpenFile -= OpenFile;
            Destroy(_currentFileExplorer.gameObject);
            LoadTexture(filePath);
            _hideSettingsObject.SetActive(false);
        }
    }

    /// <summary>
    /// Can this file be turned into an image?
    /// </summary>
    /// <param name="extension">File type</param>
    bool IsValidFile(string extension)
    {
        return _validImageTypeHashSet.Contains(extension);
    }

    /// <summary>
    /// Loads in and store a texture for future computations
    /// </summary>
    /// <param name="filePath">Filepath for image</param>
    void LoadTexture(string filePath)
    {
        byte[] data = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(64, 64, TextureFormat.ARGB32, false);
        texture.LoadImage(data);
        texture.name = Path.GetFileNameWithoutExtension(filePath);
        _controllerScript.SetTexture(texture);
    }
}
