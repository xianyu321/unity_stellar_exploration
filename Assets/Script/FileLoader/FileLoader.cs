using System;
using System.IO;
using UnityEngine;

public static class FileLoader
{
    /// <summary>
    /// 从指定路径读取 JSON 文件内容
    /// </summary>
    /// <param name="relativePath">相对于 Assets 文件夹的相对路径</param>
    /// <returns>JSON 文件内容字符串，如果文件不存在则返回 null</returns>
    public static string LoadJsonFile(string path)
    {
        // 检查文件是否存在
        if (!File.Exists(path))
        {
            Debug.LogError("JSON 文件未找到: " + path); // 修改为字符串拼接
            return null;
        }

        try
        {
            // 读取文件内容
            string jsonContent = File.ReadAllText(path);
            Debug.Log("成功读取 JSON 文件内容: " + jsonContent); // 修改为字符串拼接
            return jsonContent;
        }
        catch (Exception ex)
        {
            Debug.LogError("读取 JSON 文件时出错: " + ex.Message); // 修改为字符串拼接
            return null;
        }
    }
    public static Texture2D LoadImageFile(string path){
        byte[] imageData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2); // 初始大小可以随意设置
        if (!texture.LoadImage(imageData))
        {
            Debug.LogError("图片加载失败！");
            return null;
        }
        return texture;
    }

    public static string GetBlocksJsonFile(){
        string jsonPath =  PathLoader.GetBlocksPath();
        jsonPath = Path.Combine(jsonPath, "blocks.json");
        return LoadJsonFile(jsonPath);
    }

    public static Texture2D GetBlockTextureFile(){
        string texPath = PathLoader.GetBlocksPath();
        texPath = Path.Combine(texPath, "texture.png");
        return LoadImageFile(texPath);
    }

    public static Texture2D GetBlockIconFile(){
        string iconPath = PathLoader.GetBlocksPath();
        iconPath = Path.Combine(iconPath, "icon.png");
        return LoadImageFile(iconPath);
    }
}