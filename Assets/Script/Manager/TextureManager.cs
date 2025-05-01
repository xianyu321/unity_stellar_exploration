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
        tex.filterMode = FilterMode.Point;//设置为非线性插值像素风
        textureRols = tex.width / blockTextureSize;
        blockMaterial = new Material(Shader.Find("Standard")); // 使用标准着色器
        // 将 Texture2D 设置为材质的主纹理
        blockMaterial.SetTexture("_MainTex", tex);
    }

    void InitBlockIcon(){
        Texture2D tex = FileLoader.GetBlockIconFile();
        int rols = tex.width / blockIconSize;
        foreach(int y in Enumerable.Range(0, rols)){
            foreach(int x in Enumerable.Range(0, rols)){

                float startY = tex.height - (y + 1) * blockIconSize;
                Sprite sprite = Sprite.Create(
                    tex,
                    new Rect(x * blockIconSize, startY, blockIconSize, blockIconSize),
                    new Vector2(0.5f, 0.5f)
                );
                blockIcon.Add(sprite);
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
