using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScript : MonoBehaviour
{
    [SerializeField] GameObject _fileExplorerPrefab;
    [SerializeField] Transform _fileExplorerHolder;

    /// <summary>
    /// Called when user presses to load in image -> start fileexplorer to load in image
    /// </summary>
    public void OpenFileExplorer()
    {
        GameObject obj = Instantiate(_fileExplorerPrefab, Vector3.zero, Quaternion.identity, _fileExplorerHolder) as GameObject;
        FileExplorer.FileExplorer script = obj.GetComponent<FileExplorer.FileExplorer>();
        script.OpenFile += OpenFile;
        script.ClosedFileExplorer += ClosedFileExplorer;
    }

    /// <summary>
    /// Called when an item is selected in file explorer when looking for image
    /// </summary>
    /// <param name="name">Name of file</param>
    /// <param name="extension">Type of the file</param>
    /// <param name="filePath">Filepath to this file</param>
    void OpenFile(string name, string extension, string filePath)
    {

    }

    /// <summary>
    /// File explorer was closed without selecting an item
    /// </summary>
    void ClosedFileExplorer()
    {

    }
}
