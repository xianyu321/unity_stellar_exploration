using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager
{
    // 单例实例
    private static BlockManager _instance;

    // 全局访问点
    public static BlockManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new BlockManager();
                _instance.Initialize(); // 初始化
            }
            return _instance;
        }
    }

    // 存储所有块的数据
    public List<BlockJsonData> Blocks { get; private set; }

    // 私有构造函数，防止外部实例化
    private BlockManager()
    {
        // 构造函数私有化，确保只能通过 Instance 访问
    }

    // 初始化方法
    private void Initialize()
    {
        // 假设我们从 JSON 文件加载数据
        string jsonContent = FileLoader.GetBlocksJsonFile(); // 你可以根据需求调整路径
        Blocks = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BlockJsonData>>(jsonContent);

        if (Blocks == null || Blocks.Count == 0)
        {
            Console.WriteLine("未能加载任何块数据！");
        }
        else
        {
            Console.WriteLine($"成功加载了 {Blocks.Count} 个块数据！");
        }
    }

    // 根据名称查找块数据
    public BlockJsonData GetBlockByName(string name)
    {
        foreach (var block in Blocks)
        {
            if (block.name == name)
            {
                return block;
            }
        }
        Console.WriteLine($"未找到名为 {name} 的块！");
        return null;
    }
}

[System.Serializable]
public class BlockDataWrapper
{
    public BlockJsonData[] blocks;
}

[System.Serializable]
public class BlockFaceData
{
    public int left;
    public int right;
    public int front;
    public int back;
    public int up;
    public int down;
    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return back;
            case 1:
                return front;
            case 2:
                return up;
            case 3:
                return down;
            case 4:
                return left;
            case 5:
                return right;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;
        }
    }
}

[System.Serializable]
public class BlockJsonData
{
    public string name;
    public BlockFaceData faces;
    public int icon;
    public override string ToString()
    {
        return $"Name: {name}, icon: {icon}";
    }
}