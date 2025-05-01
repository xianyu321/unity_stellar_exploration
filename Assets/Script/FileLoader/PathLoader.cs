using System;
using System.IO;
using UnityEngine;

public static class PathLoader{
    // 拼接文件夹地址，如果文件夹不存在就创建文件夹
    public static string SplicingAndCreatePath(string path1, string path2){
        string splicingPath = Path.Combine(path1, path2);
        if (!Directory.Exists(splicingPath))
        {
            Directory.CreateDirectory(splicingPath);
        }
        return splicingPath;
    }
    public static string GetAssetsPath(){
        return Application.dataPath;
    }
    public static string GetDesktop(){
        return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }
    public static string GetDatePath(){
        return Path.Combine(GetAssetsPath(), "MyDatas");
    }
    public static string GetTexturesPath(){
        return "Textures";
        // return Path.Combine(GetDatePath(), "Textures");
    }
    public static string GetBlocksPath(){
        return Path.Combine(GetTexturesPath(), "blocks");
    }
    public static string GetSavesPath(){
        return SplicingAndCreatePath(GetDesktop(), "Saves");
        // return SplicingAndCreatePath(GetAssetsPath(), "Saves");
    }
    public static string GetSavePath(string saveName){
        return SplicingAndCreatePath(GetSavesPath(), saveName);
    }
    public static string GetWorldsPath(string saveName){
        return SplicingAndCreatePath(GetSavePath(saveName), "worlds");
    }



    public static string GetChunksPath(string saveName, string worldName){
        string worldPath = SplicingAndCreatePath(GetWorldsPath(saveName), worldName);
        string chunksPath = SplicingAndCreatePath(worldPath, "chunks");
        return chunksPath;
    }
}
