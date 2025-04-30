

using System;
using Unity.VisualScripting;
using UnityEngine;

public class BlockEntity{
    public bool isNotNull = true;
    public int blockID;
    public ChunkEntity chunk;
    public BlockCoord blockCoordInChunk;
    public ChunkCoord chunkCoord{
        get{
            return chunk.chunkCoord;
        }
    }
    // 在世界下的坐标
    public BlockCoord worldCoord{
        get{
            return new( 
                chunkCoord.x * ChunkEntity.chunkSize + blockCoordInChunk.x,
                blockCoordInChunk.y,
                chunkCoord.z * ChunkEntity.chunkSize + blockCoordInChunk.z);
        }
    }

    public BlockEntity(ChunkEntity _chunk, BlockCoord _blockCoord, int _blockID = -1){
        chunk = _chunk;
        blockCoordInChunk = _blockCoord;
        blockID = _blockID;
    }

    public BlockEntity(ChunkEntity _chunk, BlockCoord _blockCoord, BlockEnum _blockID){
        chunk = _chunk;
        blockCoordInChunk = _blockCoord;
        blockID = (int)_blockID;
    }

    public BlockEntity(ChunkEntity _chunk, BlockCoord _blockCoord, string blockData = ""){
        chunk = _chunk;
        blockCoordInChunk = _blockCoord;
        FromData(blockData);
    }

    public BlockEntity(){
        isNotNull = false;
    }

    public string ToData()
    {
        return $"blockID={blockID};";
    }
    public void FromData(string date){
        // 去掉末尾的分号
        string[] dates = date.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach(string item in dates){
            string[] keyValue = item.Split('=');
            if (keyValue.Length == 2)
            {
                string key = keyValue[0].Trim();
                string value = keyValue[1].Trim();
                switch (key){
                    case "blockID":
                    blockID = int.Parse(value);
                    return;
                }
            }
        }
    }
    public bool IsBlock(){
        if(blockID >= 0){
            return true;
        }
        return isNotNull;
    }

    public bool IsEntity()
    {
        return isNotNull;
    }

    public virtual bool IsCanUse(){
        return false;
    }

    public virtual void UseBlock(){
        if(!IsCanUse()){
            return;
        }
    }
}

public class BlockCoord{
    public int x;
    public int y;
    public int z;
    public BlockCoord()
    {
        x = y = z = 0;
    }

    public BlockCoord(int _x, int _y, int _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }

    public bool Equals(BlockCoord other)
    {
        if (other == null)
            return false;
        else if (other.x == x && other.y == y && other.z == z)
            return true;
        else
            return false;
    }

    public Vector3 Vec3(){
        return new(x,y,z);
    }

    public Vector2 Vec2(){
        return new(x, z);
    }

    public static BlockCoord ToBlockCoord(int x, int y, int z)
    {
        int size = ChunkEntity.chunkSize;
        x = (x % size + size) % size;
        z = (z % size + size) % size;
        return new(x, y, z);
    }
    public static BlockCoord ToBlockCoord(Vector3Int pos)
    {
        int size = ChunkEntity.chunkSize;
        pos.x = (pos.x % size + size) % size;
        pos.z = (pos.z % size + size) % size;
        return new(pos.x, pos.y, pos.z);
    }

    public override string ToString(){
        return $"{x},{y},{z}";
    }
}