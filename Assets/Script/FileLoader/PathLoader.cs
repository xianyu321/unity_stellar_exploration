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
    public static string GetLocalPath(){
        string father = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        return SplicingAndCreatePath(father, "LocalFiles");
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
        return SplicingAndCreatePath(GetLocalPath(), "Saves");
    }
    public static string GetAppConfigPath(){
        return SplicingAndCreatePath(GetLocalPath(), "AppConfig");
    }
    public static string GetSavePath(string saveName){
        return SplicingAndCreatePath(GetSavesPath(), saveName);
    }
    public static string GetWorldsPath(string saveName){
        return SplicingAndCreatePath(GetSavePath(saveName), "worlds");
    }
    public static string GetWorldPath(string saveName, string worldName){
        return SplicingAndCreatePath(GetWorldsPath(saveName), worldName);
    }

    public static string GetPlayersPath(string saveName){
        return SplicingAndCreatePath(GetSavePath(saveName), "players");
    }

    public static string GetPlayerPath(string saveName, string playerUID){
        return SplicingAndCreatePath(GetPlayersPath(saveName), playerUID);
    }

    public static string GetChunksPath(string saveName, string worldName){
        string worldPath = SplicingAndCreatePath(GetWorldsPath(saveName), worldName);
        string chunksPath = SplicingAndCreatePath(worldPath, "chunks");
        return chunksPath;
    }
}
