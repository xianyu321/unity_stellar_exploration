using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//噪声对象
[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "MyMenu/Biome Attribute")]
public class BiomeAttributes : ScriptableObject
{
    [Rename("噪声名字")]
    public string biomeName;
    public int offset;
    public float scale;
    [Rename("地面初始高度")]
    public int solidGroundHeight;
    [Rename("地形最大高度")]
    public int terrainHeight;
    [Rename("地形起伏大小")]
    public float terrainScale;
    [Rename("地形变种")]
    public Lode[] lodes;

    public int surfaceBlock;
    public int subSurfaceBlock;
    public int majorFloraIndex;
    
    [Header("生成参数")]
    [Rename("生成频率")]
    public float majorFloraZoneScale = 1.3f;
    [Rename("森林区域生成阈值")]
    [Range(0.1f, 1f)]
    public float majorFloraZoneThreshold = 0.6f;
    [Rename("树木的生成频率")]
    public float majorFloraPlacementScale = 15f;
    [Rename("树木的生成阈值")]
    public float majorFloraPlacementThreshold = 0.8f;
    public bool placeMajorFlora = true;
    public int maxHeight = 9;
    public int minHeight = 5;

}

//矿脉
[System.Serializable]
public class Lode {
    [Rename("矿物名称")]
    public string nodeName;
    [Rename("矿石方块id")]
    public byte blockID;
    [Rename("生成最低高度")]
    public int minHeight;
    [Rename("生成最高高度")]
    public int maxHeight;
    [Rename("生成密度(0-1)")]
    public float scale;
    [Rename("生成概率(0-1)")]
    public float threshold;
    [Rename("随机偏移量")]
    public float noiseOffset;
}
