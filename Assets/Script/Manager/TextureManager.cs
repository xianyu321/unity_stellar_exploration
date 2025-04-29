using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

//静态对象读取文件
public class TextureManager{
    private static TextureManager _instance;
    public static readonly int blockTextureSize = 16;
    public static readonly int blockIconSize = 64;
    public Material blockMaterial;
    List<Sprite> blockTexture = new List<Sprite>();
    public List<Sprite> blockIcon = new List<Sprite>();
    public int textureRols = 0;
    // 全局访问点
    public static TextureManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new TextureManager();
                _instance.Initialize(); // 初始化
            }
            return _instance;
        }
    }
    private void Initialize()
    {
        // InitBlockTexture();
        InitBlockMaterial();
        InitBlockIcon();
    }
    //加载面图片为材质
    void InitBlockMaterial(){
        Texture2D tex = FileLoader.GetBlockTextureFile();
        textureRols = tex.width / blockTextureSize;
        blockMaterial = new Material(Shader.Find("Standard")); // 使用标准着色器
        // 将 Texture2D 设置为材质的主纹理
        blockMaterial.SetTexture("_MainTex", tex);
    }

    void InitBlockIcon(){
        Texture2D tex = FileLoader.GetBlockIconFile();
        int rols = tex.width / blockIconSize;
        foreach(int x in Enumerable.Range(0, rols)){
            foreach(int y in Enumerable.Range(0, rols)){
                Sprite sprite = Sprite.Create(
                    tex,
                    new Rect(x * blockIconSize, y * blockIconSize, blockIconSize, blockIconSize),
                    new Vector2(0.5f, 0.5f)
                );
                blockIcon.Add(sprite);
            }
        }
    }

    //加载面图片文件为贴图 （不适用）
    void InitBlockTexture(){
        Texture2D tex = FileLoader.GetBlockTextureFile();
        int rols = tex.width / blockTextureSize;

        foreach(int x in Enumerable.Range(0, rols)){
            foreach(int y in Enumerable.Range(0, rols)){
                Sprite sprite = Sprite.Create(
                    tex,
                    new Rect(x * blockTextureSize, y * blockTextureSize, blockTextureSize, blockTextureSize),
                    new Vector2(0.5f, 0.5f)
                );
                blockTexture.Add(sprite);
            }
        }
    }

    public Sprite GetIcon(int id)
    {
        if(id < 1000){
            return blockIcon[id];
        }
        return null;
    }
}
