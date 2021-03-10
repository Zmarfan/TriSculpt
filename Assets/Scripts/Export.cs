using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using TMPro;

public class Export : MonoBehaviour
{
    [SerializeField] string _exportStartString = "This triangulation image will be exported as ";
    [SerializeField] string _exportMiddleString = ".png to ";
    [SerializeField] Environment.SpecialFolder _startFolder;
    [SerializeField] TMP_Text _filepathText;

    [SerializeField] UIScript _uiScript;
    [SerializeField] TMP_Text _savePromptText;
    [SerializeField] GameObject _savePrompt;
    [SerializeField] GameObject _fileExplorerPrefab;
    [SerializeField] Transform _fileExplorerHolder;

    string _currentFilepath;
    FileExplorer.FileExplorer _currentFileExplorer;

    string _currentExportFileName;

    private void Awake()
    {
        SetFilePath(Environment.GetFolderPath(_startFolder));
    }

    void SetFilePath(string filePath)
    {
        _currentFilepath = filePath;
        _filepathText.text = filePath;
    }

    /// <summary>
    /// Called from user pressing to change export directory to export images to
    /// </summary>
    public void OpenFileExplorer()
    {
        GameObject obj = Instantiate(_fileExplorerPrefab, Vector3.zero, Quaternion.identity, _fileExplorerHolder) as GameObject;
        _currentFileExplorer = obj.GetComponent<FileExplorer.FileExplorer>();
        _currentFileExplorer.OpenFile += OpenFile;
    }

    /// <summary>
    /// If file is directory it's a place that can export to
    /// </summary>
    /// <param name="name">Name of file/directory</param>
    /// <param name="extension">filetype, empty if directory</param>
    /// <param name="filePath">Filepath to this file/directory</param>
    public void OpenFile(string name, string extension, string filePath)
    {
        if (extension == string.Empty)
        {
            SetFilePath(filePath);
            _currentFileExplorer.OpenFile -= OpenFile;
            Destroy(_currentFileExplorer.gameObject);
        }
    }

    /// <summary>
    /// User pressed Export Image, load in Texture2D and set up for export
    /// </summary>
    public void StartExport()
    {
        _currentExportFileName = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

        _savePrompt.SetActive(true);
        _savePromptText.text = _exportStartString + _currentExportFileName + _exportMiddleString + _currentFilepath; 
    }

    public void DoExport()
    {
        Texture2D texture = _uiScript.GetTexture();
        byte[] data = texture.EncodeToPNG();
        File.WriteAllBytes(_currentFilepath + "/" + _currentExportFileName + ".png", data);
    }
}
