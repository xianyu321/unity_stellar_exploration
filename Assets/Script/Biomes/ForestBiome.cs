using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//默认群系
public class ForestBiome:Biome
{
    protected new static string biomeName = "Forest";
    protected new static int heightFloat = 20;
    protected new static int heightOffset = 20000;

    public override int GetHeight(BlockCoord coord){
        Vector2 pos = coord.Vec2();
        if(_height > 0){
            return _height;
        }
        float noise = Noise.Get2DPerlin(pos, heightOffset, heightScale);
        _height = Mathf.FloorToInt(heightBase + heightFloat * noise);
        return _height;
    }
}