using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] int _minTextureDimension = 600;

    Camera _thisCamera;

    private void Awake()
    {
        _thisCamera = GetComponent<Camera>();
    }

    public Texture2D SetCameraToTexture(float width, float height)
    {
        SetCameraSizeAndPosition(width, height);

        if (_thisCamera.targetTexture != null)
            _thisCamera.targetTexture.Release();

        int sizeModifier = CalculateSizeModifier(width, height);

        _thisCamera.targetTexture = new RenderTexture((int)width * sizeModifier, (int)height * sizeModifier, 24);
        _thisCamera.targetTexture.filterMode = FilterMode.Point;
        _thisCamera.Render();

        Texture2D texture = new Texture2D((int)width * sizeModifier, (int)height * sizeModifier, TextureFormat.RGB24, false);
        RenderTexture.active = _thisCamera.targetTexture;
        texture.ReadPixels(new Rect(0, 0, _thisCamera.targetTexture.width, _thisCamera.targetTexture.height), 0, 0);
        texture.Apply();
        return texture;
    }

    /// <summary>
    /// Make the camera look directly at mesh with these resolutions
    /// </summary>
    void SetCameraSizeAndPosition(float width, float height)
    {
        _thisCamera.orthographicSize = height / 2f;
        _thisCamera.aspect = width / height;

        transform.position = new Vector3(width / 2, height / 2, transform.position.z);
    }

    /// <summary>
    /// Calculate the size modifier to make sure picture resolution is crisp even if input texture is low res
    /// </summary>
    /// <param name="width">Input texture resolution width</param>
    /// <param name="height">Input texture resolution height</param>
    /// <returns>Modifier to scale render texture</returns>
    int CalculateSizeModifier(float width, float height)
    {
        int mod = 1;
        while (width * mod < _minTextureDimension || height * mod < _minTextureDimension)
        {
            mod++;
        }
        return mod;
    }
}
