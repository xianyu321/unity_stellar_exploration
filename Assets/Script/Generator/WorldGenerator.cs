
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WorldGenerator
{
    public ConcurrentDictionary<int, ConcurrentDictionary<int, ChunkEntity>> chunks = new ConcurrentDictionary<int, ConcurrentDictionary<int, ChunkEntity>>();
    public ConcurrentDictionary<int, ConcurrentDictionary<int, int>> flags = new ConcurrentDictionary<int, ConcurrentDictionary<int, int>>();
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

    public ChunkEntity GetChunkByBlockCoord(int x, int z)
    {
        ChunkCoord chunkCoord = ChunkManager.ToChunkCoord(x, z);
        return GetChunk(chunkCoord.x, chunkCoord.z);
    }
    public ChunkEntity LoadChunk(int x, int z, ChunkEntity chunk)
    {
        var innerDict = chunks.GetOrAdd(x, _ => new ConcurrentDictionary<int, ChunkEntity>());
        if (innerDict.TryGetValue(z, out var existingChunk))
        {
            return existingChunk;
        }
        LoadChunkFile(chunk);
        innerDict.TryAdd(z, chunk);
        UpdateMesh(chunk);
        return chunk;
    }

    public ChunkEntity GetChunk(int x, int z)
    {
        if (chunks.TryGetValue(x, out var existingChunk))
        {
            if (existingChunk.TryGetValue(z, out var chunk))
            {
                return chunk;
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

    static Vector2Int[] arr = new Vector2Int[]{
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
            var innerDict = flags.GetOrAdd(x, _ => new ConcurrentDictionary<int, int>());

            int count = innerDict.AddOrUpdate(z, 1, (key, oldValue) => oldValue + 1);

            if (count == 5)
            {
                var temp = GetChunk(x, z);
                if (temp != null)
                {
                    temp.UpdateChunk();
                }else{
                }
                // Debug.Log(x + " " + z);
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

    private readonly object fileLock = new object();

    public void SaveChunkToBinary(ChunkEntity chunk, string chunksPath)
    {
        string chunkFileNmae = $"{chunk.chunkCoord.x},{chunk.chunkCoord.z}.dat";
        string chunkFilePath = Path.Combine(chunksPath, chunkFileNmae);
        // lock(fileLock){
        using (var writer = new BinaryWriter(
            new FileStream(
                chunkFilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read))) // 允许其他线程读取该文件
        {
            BlockEntity[,,] blocks = chunk.blocks;

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
        // }
    }

    public bool LoadChunkFromBinary(ChunkEntity chunk, string chunksPath)
    {
        string chunkFileNmae = $"{chunk.chunkCoord.x},{chunk.chunkCoord.z}.dat";
        string chunkFilePath = Path.Combine(chunksPath, chunkFileNmae);
        // lock(fileLock){
        if (File.Exists(chunkFilePath))
        {
            using (var stream = new FileStream(
                chunkFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete)) // 允许多个读取者
            {
                using (var reader = new BinaryReader(stream))
                {
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        int x = reader.ReadInt32();
                        int y = reader.ReadInt32();
                        int z = reader.ReadInt32();
                        string value = reader.ReadString();

                        // 如果 blocks[,] 是共享资源，需加锁保护
                        lock (chunk) // 假设 chunk 本身是一个锁对象
                        {
                            chunk.blocks[x, y, z] = new BlockEntity(chunk, new(x, y, z), value);
                        }
                    }
                }
            }
            return true;
        }
        else
        {
            return false;
        }
        // }

    }
}

