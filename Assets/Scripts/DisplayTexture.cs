using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayTexture : MonoBehaviour
{
    [SerializeField] SpriteRenderer _spriteRenderer;

    public void Display(Texture2D texture)
    {
        _spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}
