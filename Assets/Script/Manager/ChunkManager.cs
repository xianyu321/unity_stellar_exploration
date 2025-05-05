using UnityEngine;
//方块
public class ChunkManager
{
    public ChunkEntity GetChunk(){
        return null;
    }

    public static ChunkCoord ToChunkCoord(Vector3 pos){
        int x = Mathf.FloorToInt(pos.x / ChunkEntity.chunkSize);
        int z = Mathf.FloorToInt(pos.z / ChunkEntity.chunkSize);
        return new(x, z);
    }

    public static ChunkCoord ToChunkCoord(float _x,float _z){
        int x = Mathf.FloorToInt(_x / ChunkEntity.chunkSize);
        int z = Mathf.FloorToInt(_z / ChunkEntity.chunkSize);
        return new(x, z);
    }

    public static int GetChunkDis(ChunkCoord chunk1, ChunkCoord chunk2){
        return Mathf.Abs(chunk1.x - chunk2.x) + Mathf.Abs(chunk1.z - chunk2.z);
    }
}