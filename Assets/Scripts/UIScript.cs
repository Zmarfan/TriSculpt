using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;

public class UIScript : MonoBehaviour
{
    [Header("Settings")]

    [SerializeField] bool _changeResolutionPoint = true;
    [SerializeField] int _maxResolutionDimension = 350; //No need for too many pixels to loop over, details aren't that important for triangles
    [SerializeField] List<string> _validImageTypes = new List<string>() { ".png", ".jpg" };

    [Header("Sliders")]

    [SerializeField] Slider _detailAccuracySlider;
    [SerializeField] Slider _amountOfPointsSlider;
    [SerializeField] Slider _borderPointAmountSlider;
    [SerializeField] Slider _pointInfluenceLengthSlider;
    [SerializeField] Slider _pointInfluenceStrengthSlider;
    [SerializeField] Slider _cornerColorSamplePointSlider;

    [Header("Drop")]

    [SerializeField] Image _originalTextureImage;
    [SerializeField] Image _displayCurrentMeshImage;
    [SerializeField] TMP_Text _filePathText;
    [SerializeField] Algorithm _algorithmScript;
    [SerializeField] GameObject _hideSettingsObject;
    [SerializeField] GameObject _hideExportObject;
    [SerializeField] GameObject _fileExplorerPrefab;
    [SerializeField] Transform _fileExplorerHolder;

    HashSet<string> _validImageTypeHashSet = new HashSet<string>();
    FileExplorer.FileExplorer _currentFileExplorer;

    Texture2D _inputTexture;
    Texture2D _generatedTexture;

    private void Awake()
    {
        foreach (string type in _validImageTypes)
            _validImageTypeHashSet.Add(type);
    }
    
    public Texture2D GetTexture()
    {
        return _generatedTexture;
    }

    public void HardGenerate()
    {
        _algorithmScript.NeedNewEntropy();
        NormalGenerate();
    }

    /// <summary>
    /// Generate which doesn't need to recalculate entropy
    /// </summary>
    public void NormalGenerate()
    {
        _algorithmScript.NeedNewPoints();
        Generate();
    }

    /// <summary>
    /// Only called directly from color sample slider since it doesn't need to recalculate entropy or points
    /// </summary>
    public void Generate()
    {
        _algorithmScript.SetParameters(_detailAccuracySlider.value, _amountOfPointsSlider.value,
                                        _borderPointAmountSlider.value, _pointInfluenceLengthSlider.value,
                                        _pointInfluenceStrengthSlider.value, _cornerColorSamplePointSlider.value);

        _generatedTexture = _algorithmScript.Generate(_inputTexture);
        _displayCurrentMeshImage.sprite = SpriteFromTexture(_generatedTexture);
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
            _filePathText.text = filePath;
            _hideSettingsObject.SetActive(false);
            _hideExportObject.SetActive(false);
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
        _originalTextureImage.sprite = SpriteFromTexture(texture);
        _inputTexture = ResizeTexture(texture);
        HardGenerate();
    }

    /// <summary>
    /// Creates a Sprite from a 2DTexture
    /// </summary>
    Sprite SpriteFromTexture(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// Resizes texture if it is to big as to cut down on calculation time. No loss in detail is lost for triangels
    /// </summary>
    Texture2D ResizeTexture(Texture2D texture)
    {
        Texture2D modTexture = new Texture2D(texture.width, texture.height);
        modTexture.SetPixels(texture.GetPixels());
        modTexture.name = texture.name;
        modTexture.Apply();

        if (NeedToResizeTexture(modTexture, out int newWidth, out int newHeight))
        {
            if (_changeResolutionPoint)
                ImageProperties.ResizeTexturePoint(newWidth, newHeight, ref modTexture);
            else
                ImageProperties.ResizeTextureBilinear(newWidth, newHeight, ref modTexture);
        }

        return modTexture;
    }

    /// <summary>
    /// Checks if input texture is too big or not for the algorithm. If it is, calculate appropriate image size 
    /// </summary>
    /// <param name="texture">Input texture</param>
    /// <param name="newWidth">New width if it is too big</param>
    /// <param name="newHeight">New Height if it is too big</param>
    bool NeedToResizeTexture(Texture2D texture, out int newWidth, out int newHeight)
    {
        newWidth = texture.width;
        newHeight = texture.height;

        //Good size already
        if (texture.width < _maxResolutionDimension && texture.height < _maxResolutionDimension)
            return false;
        //What resolution should it resize to?

        float ratioX = _maxResolutionDimension / (float)texture.width;
        float ratioY = _maxResolutionDimension / (float)texture.height;
        float ratio = ratioX < ratioY ? ratioX : ratioY;

        newWidth = (int)(texture.width * ratio);
        newHeight = (int)(texture.height * ratio);
        return true;
    }
}
