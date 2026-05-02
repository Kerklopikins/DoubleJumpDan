using UnityEngine;
using System;
using System.IO;

public class MainMenuCamera: MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    DirectoryInfo directory;
    FileInfo[] files;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        directory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        files = directory.GetFiles("*IMG_2731.jpg");
    }

    void Update()
    {
        if(spriteRenderer == null)
            return;
            
        if(GameInputManager.Instance.ShootButtonDown())
        {
            spriteRenderer.sprite = LoadSprite(files[0].FullName);
        }
    }

    private Sprite LoadSprite(string path)
    {
        if(string.IsNullOrEmpty(path))
        {
            Debug.LogError("Path is null");
            return null;
        }

        if(!File.Exists(path))
        {
            Debug.LogError("File is null");
            return null;
        }

        byte[] bytes = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
        ImageConversion.LoadImage(texture, bytes, false);
        texture.filterMode = FilterMode.Point;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        //texture.Apply(false, false);

        return sprite;
    }
}