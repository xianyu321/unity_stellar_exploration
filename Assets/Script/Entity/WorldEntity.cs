

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class WorldEntity
{
    public WorldGenerator worldGenerator;

    public WorldEntity(WorldGenerator _worldGenerator)
    {
        worldGenerator = _worldGenerator;
        worldGenerator.BindWorld(this);
        // TextInit();
    }

    public void TextInit()
    {
        Vector3 pos = new Vector3(0, 0, 0);
        ChunkCoord coord = ChunkManager.ToChunkCoord(pos);
        int width = 10;
        for (int x = coord.x - width; x <= coord.x + width; ++x)
        {
            for (int z = coord.z - width; z <= coord.z + width; ++z)
            {
                LoadChunk(x, z);
            }
        }
    }

    public Dictionary<int, Dictionary<int, int>> loads = new Dictionary<int, Dictionary<int, int>>();
    public Dictionary<int, Dictionary<int, int>> shows = new Dictionary<int, Dictionary<int, int>>();
    public void AddLoad(int x, int z)
    {
        if (!loads.ContainsKey(x))
        {
            loads[x] = new Dictionary<int, int>();
            shows[x] = new Dictionary<int, int>();
        }
        if (!loads[x].ContainsKey(z))
        {
            loads[x][z] = 1;
            shows[x][z] = 0;
            LoadChunk(x, z);
        }
        else
        {
            if (shows[x][z] == 0)
            {
                worldGenerator.GetChunk(x, z).UpdateChunk();
            }
        }
        ++shows[x][z];
    }
    public void DelShow(int x, int z)
    {
        --shows[x][z];
        if (shows[x][z] == 0)
        {
            GetChunk(x, z).ClearMeshData();
        }
    }

    public async Task LoadChunk(int x, int z)
    {
        ChunkEntity chunk = new ChunkEntity(new(x, z), this);
        GameObject chunks = GameObject.Find("Chunks");
        chunk.chunkObject.transform.SetParent(chunks.transform);
        // Task.Run(() =>
        // {
        //     worldGenerator.LoadChunk(x, z, chunk);
        // });
        try
        {
            var task = Task.Run(() =>
            {
                worldGenerator.LoadChunk(x, z, chunk);
            });
            await task;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Caught an exception: {ex.Message}");
        }
        // ThreadPool.Instance.QueueTask(() =>
        // {
        //     worldGenerator.LoadChunk(x, z, chunk);
        // });
    }

    public ChunkEntity GetChunk(int x, int z)
    {
        return worldGenerator.GetChunk(x, z);
    }

    public ChunkEntity GetChunkByBlockCoord(int x, int z)
    {
        return worldGenerator.GetChunkByBlockCoord(x, z);
    }
    public BlockEntity GetBlock(int x, int y, int z)
    {
        return worldGenerator.GetBlock(x, y, z);
    }

    public bool IsBlock(int x, int y, int z)
    {
        return worldGenerator.IsBlock(x, y, z);
    }

    public BlockEntity GetBlock(Vector3 vec3)
    {
        return GetBlock(VectorTools.Vct3ToVec3Int(vec3));
    }

    public BlockEntity GetBlock(Vector3Int pos)
    {
        return worldGenerator.GetBlock(pos.x, pos.y, pos.z);
    }

    public void BrokenBlock(Vector3Int brokenBlockPos)
    {
        BlockEntity block = GetBlock(brokenBlockPos);
        if (block.IsEntity())
        {
            block.chunk.BrokenBlock(block.blockCoordInChunk);
            block.chunk.UpdateChunk();
        }
    }

    public bool PlacedBlock(Vector3Int placedBlockPos, int blockId)
    {
        if (blockId < 1000)
        {
            ChunkEntity chunk = worldGenerator.GetChunkByBlockCoord(placedBlockPos);
            BlockCoord blockCoord = BlockCoord.ToBlockCoord(placedBlockPos);
            return chunk.PlaceBlock(blockCoord, blockId);
        }
        return false;
    }
}