using UnityEngine;
//方块
public class ChunkManager
{
    static int chunkSize = 16;
    public ChunkEntity GetChunk(){
        return null;
    }

    public static ChunkCoord ToChunkCoord(Vector3 pos){
        int x = Mathf.FloorToInt(pos.x / ChunkEntity.chunkSize);
        int y = Mathf.FloorToInt(pos.y / ChunkEntity.chunkSize);
        return new(x, y);
    }

    public static ChunkCoord ToChunkCoord(float _x,float _z){
        int x = Mathf.FloorToInt(_x / ChunkEntity.chunkSize);
        int z = Mathf.FloorToInt(_z / ChunkEntity.chunkSize);
        return new(x, z);
    }
}