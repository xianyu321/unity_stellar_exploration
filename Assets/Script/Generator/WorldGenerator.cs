
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UIElements;

public class WorldGenerator
{
    public Dictionary<int, Dictionary<int, ChunkEntity>> chunks;
    float offset = 0;
    float scale = 0.1f;

    public virtual void GenerateChunk(ChunkEntity chunk){
        GeneratorChunk(chunk.chunkCoord, chunk);
        GenerateChunkBase(chunk);
    }

    //构造基础属性
    public virtual void GenerateChunkBase(ChunkEntity chunk){
        for(int x = 0; x < ChunkEntity.chunkSize; ++x){
            for(int z = 0; z < ChunkEntity.chunkSize; ++z){
                chunk.blocks[x, 0, z] = new(chunk, new(x, 0, z), BlockEnum.StoneCutter);
                BlockCoord worldCoord = chunk.blocks[x, 0, z].worldCoord;
                Biome biome = GenerateBiomes(worldCoord);
                chunk.biome[x, z] = biome;
                int height = biome.GetHeight(worldCoord);
                int soilDepth = biome.GetSoilDepth(worldCoord);
                for(int y = 1; y < height - soilDepth; ++y){
                    chunk.blocks[x, y, z] = new(chunk, new(x, y, z), biome.GetsoilLayerBottom());
                }
                for(int y = height - soilDepth; y < height; ++y){
                    chunk.blocks[x, y, z] = new(chunk, new(x, y, z), biome.GetsoilLayerMiddle());
                }
                chunk.blocks[x, height, z] = new(chunk, new(x, height, z), biome.GetsoilLayerTop());
            }
        }
    }

    public bool IsBlock(int x, int y, int z){
        BlockEntity block = GetBlock(x, y, z);
        if(!block.IsEntity()){
            return false;
        }
        return block.IsBlock();
    }

    public BlockEntity GetBlock(int x, int y, int z){
        if(y < 0){
            return new();
        }
        ChunkCoord chunkCoord = ChunkManager.ToChunkCoord(x, z);
        ChunkEntity chunk = GetChunk(chunkCoord);
        if(chunk is null){
            return new();
        }
        BlockCoord blockCoord = BlockCoord.ToBlockCoord(x, y, z);
        return chunk.GetBlock(blockCoord.x, blockCoord.y, blockCoord.z);
    }

    public ChunkEntity GetChunkByBlockCoord(int x, int z){
        ChunkCoord chunkCoord = ChunkManager.ToChunkCoord(x, z);
        return GetChunk(chunkCoord);
    }

    public ChunkEntity GetChunkByBlockCoord(Vector3Int pos){
        ChunkCoord chunkCoord = ChunkManager.ToChunkCoord(pos.x, pos.z);
        return GetChunk(chunkCoord);
    }

    public void GeneratorChunk(ChunkCoord coord, ChunkEntity chunk){
        if (!chunks.ContainsKey(coord.x))
        {
            chunks[coord.x] = new Dictionary<int, ChunkEntity>(); // 初始化内层字典
        }
        chunks[coord.x][coord.z] = chunk; // 初始化内层字典
    }

    public ChunkEntity GetChunk(ChunkCoord chunkCoord){
        if (chunks.ContainsKey(chunkCoord.x))
        {
            // Debug.Log(chunkCoord.x);
            if(chunks[chunkCoord.x].ContainsKey(chunkCoord.z)){
                return chunks[chunkCoord.x][chunkCoord.z];
            }
        }
        return null;
    }

    //生成群系
    Biome GenerateBiomes(BlockCoord coord){
        Vector2 pos = coord.Vec2();
        float biomeWidget = Noise.Get2DPerlin(pos, offset, scale);
        // return new ForestBiome();

        if (biomeWidget < 0.5){
            return new Biome();
        }else if(biomeWidget < 0.65){
            return new ForestBiome();
        }else{
            return new ForestBiome();
        }
    }
    //判断生成洞穴
    void GenerateCave(){

    }
}