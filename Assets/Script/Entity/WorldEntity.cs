

using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldEntity{
    public WorldGenerator worldGenerator;
    public Dictionary<int, Dictionary<int, ChunkEntity>> chunks = new Dictionary<int, Dictionary<int, ChunkEntity>>();

    public WorldEntity(WorldGenerator _worldGenerator){
        worldGenerator = _worldGenerator;
        worldGenerator.chunks = chunks;
        TextInit();
    }

    void TextInit(){
        Vector3 pos = new Vector3(0, 0, 0);
        ChunkCoord coord = ChunkManager.ToChunkCoord(pos);
        int width = 4;
        for(int x = coord.x - width; x <= coord.x + width; ++ x){
            for(int z = coord.z - width; z <= coord.z + width; ++z){
                ChunkEntity chunk = new(new(x,z), this);
                worldGenerator.GenerateChunk(chunk);
                GameObject chunks = GameObject.Find("Chunks");
                chunk.chunkObject.transform.SetParent(chunks.transform);
                // chunk.UpdateChunk();
            }
        }
        for(int x = coord.x - width; x <= coord.x + width; ++ x){
            for(int z = coord.z - width; z <= coord.z + width; ++z){
                ChunkEntity chunk = worldGenerator.GetChunk(new(x, z));
                chunk.UpdateChunk();
            }
        }
    }

    public BlockEntity GetBlock(int x, int y, int z){
        return worldGenerator.GetBlock(x, y, z);
    }

    public bool IsBlock(int x, int y, int z){
        return worldGenerator.IsBlock(x, y, z);
    }

    public BlockEntity GetBlock(Vector3 vec3){
        return GetBlock(VectorTools.Vct3ToVec3Int(vec3));
    }

    public BlockEntity GetBlock(Vector3Int pos){
        return worldGenerator.GetBlock(pos.x, pos.y, pos.z);
    }

    public void BrokenBlock(Vector3Int brokenBlockPos)
    {
        BlockEntity block = GetBlock(brokenBlockPos);
        if(block.IsEntity()){
            block.chunk.BrokenBlock(block.blockCoordInChunk);
            block.chunk.UpdateChunk();
        }
    }

    public bool PlacedBlock(Vector3Int placedBlockPos, int blockId)
    {
        if(blockId < 1000){
            ChunkEntity chunk = worldGenerator.GetChunkByBlockCoord(placedBlockPos);
            BlockCoord blockCoord = BlockCoord.ToBlockCoord(placedBlockPos);
            return chunk.PlaceBlock(blockCoord, blockId);
        }
        return false;
    }
}