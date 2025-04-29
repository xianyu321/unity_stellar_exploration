using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public static class UnityTools{
    static string fileDir = "/MyDatas/settings.cfg";
    static string settingsFileDir = "/MyDatas/settings.cfg";
    static string blockTypeFileDir = "/MyDatas/blocktypes.cfg";
    public static void SetBlockTypes(BlockType[] blockTypes){
        string jsonExport = JsonConvert.SerializeObject(blockTypes);
        File.WriteAllText(Application.dataPath + blockTypeFileDir, jsonExport);
    }
    
    public static BlockType[] GetBlockTypes(){
        string jsonImport = File.ReadAllText(Application.dataPath + blockTypeFileDir);
        return JsonConvert.DeserializeObject<BlockType[]>(jsonImport);
    }


    public static void SetSettingsJson(Settings settings){
        string jsonExport = JsonUtility.ToJson(settings);
        File.WriteAllText(Application.dataPath + settingsFileDir, jsonExport);
    }

    public static Settings GetSettingsJson(){
        string jsonImport = File.ReadAllText(Application.dataPath + settingsFileDir);
        return JsonUtility.FromJson<Settings>(jsonImport);
    }

    
}

public static class BlockID{
    public const int Air = 0;//空气
    public const int Bedrock = 1;//基岩
    public const int Stone = 2;//石头
    public const int GrassBlock = 3;//草方块
    public const int Sand = 4;//沙子
    public const int Dirt = 5;//泥土
    public const int Log = 6;//橡木原木
    public const int Wood = 7;//橡木木板
    public const int RoughStone = 8;//原石
    public const int Brick = 9;//砖块
    public const int Glass = 10;//玻璃
    public const int Leaves = 11;//树叶
    public const int Cactus = 12;//仙人掌
}