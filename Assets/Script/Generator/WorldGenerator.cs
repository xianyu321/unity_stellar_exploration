
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WorldGenerator
{
    public Dictionary<int, Dictionary<int, ChunkEntity>> chunks = new Dictionary<int, Dictionary<int, ChunkEntity>>();
    public Dictionary<int, Dictionary<int, int>> flags = new Dictionary<int, Dictionary<int, int>>();
    float offset = 0;
    float scale = 0.1f;
    WorldEntity world;

    public void BindWorld(WorldEntity _world)
    {
        world = _world;
    }

    public bool IsBlock(int x, int y, int z)
    {
        BlockEntity block = GetBlock(x, y, z);
        if (!block.IsEntity())
        {
            return false;
        }
        return block.IsBlock();
    }

    public ChunkEntity GetChunkByBlockCoord(Vector3Int pos)
    {
        ChunkCoord chunkCoord = ChunkManager.ToChunkCoord(pos.x, pos.z);
        return GetChunk(chunkCoord.x, chunkCoord.z);
    }

    public ChunkEntity LoadChunk(int x, int z, ChunkEntity chunk)
    {
        object lockObject = ThreadPool.Instance.GetLock(x, z);
        lock(lockObject){
            if (chunks.ContainsKey(x))
            {
                if (chunks[x].ContainsKey(z))
                {
                    return chunks[x][z];
                }
            }
            else
            {
                chunks[x] = new Dictionary<int, ChunkEntity>(); // 初始化内层字典
            }
            LoadChunkFile(chunk);
            UpdateMesh(chunk);
            chunks[x][z] = chunk;
            return chunk;
        }
    }

    public ChunkEntity GetChunk(int x, int z)
    {
        if (chunks.ContainsKey(x))
        {
            if (chunks[x] != null && chunks[x].ContainsKey(z))
            {
                return chunks[x][z];
            }
        }
        return null;
    }

    public void LoadChunkFile(ChunkEntity chunk)
    {
        if (loadChunk(chunk))
        {
        }
        else
        {
            GenerateChunkBase(chunk);
            saveChunk(chunk);
        }
        chunk.isLoad = true;
    }

    Vector2Int[] arr = new Vector2Int[]{
        new(0, 0),
        new(0, 1),
        new(0, -1),
        new(-1, 0),
        new(1, 0),
    };

    public void UpdateMesh(ChunkEntity chunk)
    {
        foreach (Vector2Int item in arr)
        {
            int x = chunk.chunkCoord.x + item.x;
            int z = chunk.chunkCoord.z + item.y;
            object lockObject = ThreadPool.Instance.GetLock(x, z);
            lock(lockObject){
                if (!flags.ContainsKey(x))
                {
                    flags[x] = new Dictionary<int, int>();
                }
                if(flags[x].ContainsKey(z)){
                    ++flags[x][z];
                }else{
                    flags[x][z] = 1;
                }
                if (flags[x][z] == 5)
                {
                    chunk.UpdateChunk();
                }
            }

        }
    }

    public BlockEntity GetBlock(int x, int y, int z)
    {
        if (y < 0)
        {
            return new();
        }
        ChunkCoord chunkCoord = ChunkManager.ToChunkCoord(x, z);
        ChunkEntity chunk = GetChunk(chunkCoord.x, chunkCoord.z);
        if (chunk is null)
        {
            return new();
        }
        BlockCoord blockCoord = BlockCoord.ToBlockCoord(x, y, z);
        return chunk.GetBlock(blockCoord.x, blockCoord.y, blockCoord.z);
    }

    //构造基础属性
    public virtual void GenerateChunkBase(ChunkEntity chunk)
    {
        for (int x = 0; x < ChunkEntity.chunkSize; ++x)
        {
            for (int z = 0; z < ChunkEntity.chunkSize; ++z)
            {
                chunk.blocks[x, 0, z] = new(chunk, new(x, 0, z), BlockEnum.StoneCutter);
                BlockCoord worldCoord = chunk.blocks[x, 0, z].worldCoord;
                Biome biome = GenerateBiomes(worldCoord);
                chunk.biome[x, z] = biome;
                int height = biome.GetHeight(worldCoord);
                int soilDepth = biome.GetSoilDepth(worldCoord);
                for (int y = 1; y < height - soilDepth; ++y)
                {
                    chunk.blocks[x, y, z] = new(chunk, new(x, y, z), biome.GetsoilLayerBottom());
                }
                for (int y = height - soilDepth; y < height; ++y)
                {
                    chunk.blocks[x, y, z] = new(chunk, new(x, y, z), biome.GetsoilLayerMiddle());
                }
                chunk.blocks[x, height, z] = new(chunk, new(x, height, z), biome.GetsoilLayerTop());
            }
        }
    }
    //生成群系
    Biome GenerateBiomes(BlockCoord coord)
    {
        Vector2 pos = coord.Vec2();
        float biomeWidget = Noise.Get2DPerlin(pos, offset, scale);
        if (biomeWidget < 0.5)
        {
            return new Biome();
        }
        else if (biomeWidget < 0.65)
        {
            return new ForestBiome();
        }
        else
        {
            return new ForestBiome();
        }
    }
    //判断生成洞穴
    void GenerateCave()
    {

    }
    public void saveChunk(ChunkEntity chunk)
    {
        string chunksPath = WorldManager.Instance.GetChunksPath(this);
        SaveChunkToBinary(chunk, chunksPath);
    }
    public bool loadChunk(ChunkEntity chunk)
    {
        string chunksPath = WorldManager.Instance.GetChunksPath(this);
        return LoadChunkFromBinary(chunk, chunksPath);
    }

    public void SaveChunkToBinary(ChunkEntity chunk, string chunksPath)
    {
        string chunkFileNmae = $"{chunk.chunkCoord.x},{chunk.chunkCoord.x}.dat";
        string chunkFilePath = Path.Combine(chunksPath, chunkFileNmae);
        BlockEntity[,,] blocks = chunk.blocks;
        using (var writer = new BinaryWriter(File.Open(chunkFilePath, FileMode.Create)))
        {
            chunk.ExecuteAll((block) =>
            {
                if (block is not null)
                {
                    writer.Write(block.blockCoordInChunk.x);
                    writer.Write(block.blockCoordInChunk.y);
                    writer.Write(block.blockCoordInChunk.z);
                    writer.Write(block.ToData());
                }
            });
        }
    }

    public bool LoadChunkFromBinary(ChunkEntity chunk, string chunksPath)
    {
        string chunkFileNmae = $"{chunk.chunkCoord.x},{chunk.chunkCoord.x}.dat";
        string chunkFilePath = Path.Combine(chunksPath, chunkFileNmae);
        if (File.Exists(chunkFilePath))
        {
            using (var reader = new BinaryReader(File.Open(chunkFilePath, FileMode.Open)))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    int x = reader.ReadInt32(); // 假设文件存储的是整数
                    int y = reader.ReadInt32();
                    int z = reader.ReadInt32();
                    string value = reader.ReadString();
                    chunk.blocks[x, y, z] = new BlockEntity(chunk, new(x, y, z), value);
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}

