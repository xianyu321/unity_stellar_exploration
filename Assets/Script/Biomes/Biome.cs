using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//默认群系
public class Biome
{
    protected static string biomeName = "BaseBiome";
    // 高度相关生成参数
    protected int _height = 0;
    // 基础高度
    protected static int heightBase = 50;
    // 高度起伏度
    protected static int heightFloat = 16;
    //偏移量
    protected static int heightOffset = 10000;
    //起伏频率
    protected static float heightScale = .2f;
    //土层参数
    protected int _soilDepth = 0;
    protected static BlockEnum soilLayerTop = BlockEnum.Grass;
    protected static BlockEnum soilLayerMiddle = BlockEnum.Dirt;
    protected static BlockEnum soilLayerBottom = BlockEnum.Stone;
    //土层厚度参数
    protected static int soilDepthBase = 2;
    protected static int soilDepthFloat = 4;
    protected static int soilDepthOffset = 1000;
    protected static int soilDepthScale = 4;

    public virtual int GetHeight(BlockCoord coord){
        Vector2 pos = coord.Vec2();
        if(_height > 0){
            return _height;
        }
        float noise = Noise.Get2DPerlin(pos, heightOffset, heightScale);
        _height = Mathf.FloorToInt(heightBase + heightFloat * noise);
        return _height;
    }

    public virtual int GetSoilDepth(BlockCoord coord){
        Vector2 pos = coord.Vec2();
        if(_soilDepth > 0){
            return _soilDepth;
        }
        float noise = Noise.Get2DPerlin(pos, soilDepthOffset, soilDepthScale);
        _soilDepth = Mathf.FloorToInt(soilDepthBase + soilDepthFloat * noise);
        return _soilDepth;
    }
    public virtual string GetName(){
        return biomeName;
    }
    public virtual BlockEnum GetsoilLayerTop(){
        return soilLayerTop;
    }
    public virtual BlockEnum GetsoilLayerMiddle(){
        return soilLayerMiddle;
    }
    public virtual BlockEnum GetsoilLayerBottom(){
        return soilLayerBottom;
    }
    
}