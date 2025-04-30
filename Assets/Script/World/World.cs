using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using System.Threading;
using System.IO;
using System;
using Unity.VisualScripting;

public class World : MonoBehaviour
{
    public Settings settings;
    [Rename("噪声对象")]
    public BiomeAttributes[] biomes;//噪声对象
    [Rename("玩家对象")]
    public Transform player;//玩家对象
    // [Rename("玩家所在坐标")]
    public Vector3 spawnPosition;//玩家所在坐标
    [Rename("材质对象")]
    public Material material;//材质对象
    public Material transparentMaterial;//可透视对象
    public BlockType[] blockTypes;//方块样式
    public Chunk[,] chunks = new Chunk[VoxelData.worldSizeInChunks, VoxelData.worldSizeInChunks];//区块数组
    public List<ChunkCoord> activeChunks = new List<ChunkCoord>();//已经加载的区块列表
    public ChunkCoord playerLastChunkCoord;//玩家所在之前的区块
    public ChunkCoord playerChunkCoord;
    public List<ChunkCoord> chunksToCreate = new List<ChunkCoord>(); //需要创建的区块
    public List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();

    bool applyingModifications = false;
    [Rename("debugUI")]
    public GameObject debugScreen;
    // ConcurrentQueue<Queue<VoxelMod>> modifications = new ConcurrentQueue<Queue<VoxelMod>>();
    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();

    public GameObject creativeInventioryWindow;
    public GameObject cursorSlot;

    [Header("Performance")]
    Thread ChunkUpdateThread;
    public object ChunkUpdateThreadLock = new object();

    //是否打开ui界面
    private bool _inUI;
    public bool inUI
    {
        get
        {
            return _inUI;
        }
        set
        {
            _inUI = value;
            if (_inUI)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (creativeInventioryWindow)
                    creativeInventioryWindow.SetActive(true);
                if (cursorSlot)
                    cursorSlot.SetActive(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                if (creativeInventioryWindow)
                    creativeInventioryWindow.SetActive(false);
                if (cursorSlot)
                    cursorSlot.SetActive(false);
            }
        }
    }

    private void Start()
    {
        // UnityTools.SetSettingsJson(settings);
        // settings = UnityTools.GetSettingsJson();
        // UnityTools.SetBlockTypes(blockTypes);
        // blockTypes = UnityTools.GetBlockTypes();

        // Random.InitState(seed);

        // Shader.SetGlobalFloat("minGlobalLightLevel", VoxelData.minLightLevel);
        // Shader.SetGlobalFloat("maxGlobalLightLevel", VoxelData.maxLightLevel);

        // spawnPosition = new Vector3((VoxelData.worldSizeInChunks * VoxelData.chunkWidth) / 2f, VoxelData.chunkHeight - 50f, (VoxelData.worldSizeInChunks * VoxelData.chunkWidth) / 2f);
        // GenerateWorld();//创建世界
        // playerLastChunkCoord = GetChunkCoordFromVector3(player.transform.position);
        // SetGlobalLightValue();

    }

    void OnEnable()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        
        // 开始计时
        stopwatch.Start();

        // ChunkEntity chunk = new ChunkEntity(new ChunkCoord(0, 0));
        // chunk.chunkObject.transform.SetParent(this.transform);
        // SaveManager saveManager = new SaveManager();
        // // saveManager.LoadChunkFromBinary(chunk);
        // chunk.UpdateChunk();
        // saveManager.SaveChunkToBinary(chunk);

        WorldManager.Instance.GetOrCreateWorld(new WorldGenerator());
        Debug.Log("代码执行耗时: " + stopwatch.ElapsedMilliseconds + " ms");

    }

    void ThreadedUpdate()
    {
        while (true)
        {
            if (!applyingModifications)
            {
                ApplyModifications();
            }

            if (chunksToUpdate.Count > 0)
            {
                UpdateChunks();
            }
        }
    }

    private void OnDisable()
    {
        if (settings.enableThreading)
        {
            ChunkUpdateThread.Abort();
        }

    }

    [Range(0f, 1f)]
    public float globalLightLevel = 0.4f;
    public Color day;
    public Color night;
    bool threadOnce = true;

    public void SetGlobalLightValue()
    {
        Shader.SetGlobalFloat("GlobalLightLevel", globalLightLevel);
        Camera.main.backgroundColor = Color.Lerp(night, day, globalLightLevel);
    }

    private void Update()
    {
        //玩家区块改变后重新加载世界
        return;
        playerChunkCoord = GetChunkCoordFromVector3(player.transform.position);

        if (!playerChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();

        if (chunksToCreate.Count > 0)
        {
            CreateChunk();
        }

        if (chunksToDraw.Count > 0)
        {
            if (chunksToDraw.Peek().isEditable)
            {
                chunksToDraw.Dequeue().CreateMesh();
            }
        }

        if (!settings.enableThreading)
        {

            if (!applyingModifications)
                ApplyModifications();

            if (chunksToUpdate.Count > 0)
                UpdateChunks();

        }
        else
        {
            if (threadOnce)
            {
                ChunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
                ChunkUpdateThread.Start();
            }
            threadOnce = false;
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }
    }

    private void CheckViewDistance()
    {
        //玩家所在区块
        int chunkX = Mathf.FloorToInt(player.position.x / VoxelData.chunkWidth);
        int chunkZ = Mathf.FloorToInt(player.position.z / VoxelData.chunkWidth);
        playerLastChunkCoord = new ChunkCoord(chunkX, chunkZ);
        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);
        activeChunks.Clear();
        for (int x = chunkX - settings.viewDistance / 2; x <= chunkX + settings.viewDistance / 2; x++)
        {
            for (int z = chunkZ - settings.viewDistance / 2; z <= chunkZ + settings.viewDistance / 2; z++)
            {
                // 如果区块存在
                if (IsChunkInWorld(x, z))
                {
                    ChunkCoord thisChunk = new ChunkCoord(x, z);

                    if (chunks[x, z] == null)//不存在区块加载区块
                    {
                        // Debug.Log(x + " " + z);
                        chunks[x, z] = new Chunk(this, new ChunkCoord(x, z));
                        chunksToCreate.Add(new ChunkCoord(x, z));
                    }
                    else if (!chunks[x, z].isActive)//存在区块显示区块
                    {

                        chunks[x, z].isActive = true;
                    }
                    activeChunks.Add(thisChunk);
                    // 移除在已经加载的区块
                    for (int i = previouslyActiveChunks.Count - 1; i >= 0; i--)
                    {
                        if (previouslyActiveChunks[i].x == x && previouslyActiveChunks[i].z == z)
                            previouslyActiveChunks.RemoveAt(i);
                    }
                }
            }
        }
        // 把未移除的区块变为不可视状态
        foreach (ChunkCoord coord in previouslyActiveChunks)
        {
            chunks[coord.x, coord.z].isActive = false;
        }
    }

    //区块是否在这个世界
    bool IsChunkInWorld(int x, int z)
    {
        if (x >= 0 && x < VoxelData.worldSizeInChunks && z >= 0 && z < VoxelData.worldSizeInChunks)
            return true;
        return false;
    }

    private void GenerateWorld()
    {
        for (int x = VoxelData.worldSizeInChunks / 2 - settings.viewDistance / 2; x <= VoxelData.worldSizeInChunks / 2 + settings.viewDistance / 2; x++)
        {
            for (int z = VoxelData.worldSizeInChunks / 2 - settings.viewDistance / 2; z <= VoxelData.worldSizeInChunks / 2 + settings.viewDistance / 2; z++)
            {
                if (IsChunkInWorld(x, z))
                {
                    ChunkCoord thisCoord = new ChunkCoord(x, z);
                    chunks[x, z] = new Chunk(this, thisCoord);
                    chunksToCreate.Add(thisCoord);
                }
            }
        }

        player.position = spawnPosition;
        CheckViewDistance();
    }


    void CreateChunk()
    {

        ChunkCoord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        // activeChunks.Add(c);
        chunks[c.x, c.z].Init();

    }

    void UpdateChunks()
    {

        bool updated = false;
        int index = 0;

        lock (ChunkUpdateThreadLock)
        {
            while (!updated && index < chunksToUpdate.Count)
            {
                if (chunksToUpdate[index].isEditable)
                {
                    chunksToUpdate[index].UpdateChunk();
                    if (!activeChunks.Contains(chunksToUpdate[index].chunkCoord))
                        activeChunks.Add(chunksToUpdate[index].chunkCoord);
                    chunksToUpdate.RemoveAt(index);
                    updated = true;
                }
                else
                    index++;
            }
        }


    }

    //创建区块的进程
    void ApplyModifications()
    {
        applyingModifications = true;

        while (modifications.Count > 0)
        {
            Queue<VoxelMod> queue = modifications.Dequeue();
            if (queue == null)
            {
                Debug.Log("队列出现空值");
            }
            while (queue != null && queue.Count > 0)
            {
                VoxelMod v = queue.Dequeue();
                ChunkCoord c = GetChunkCoordFromVector3(v.position);
                if (chunks[c.x, c.z] == null)
                {
                    chunks[c.x, c.z] = new Chunk(this, c);
                    chunksToCreate.Add(c);
                }
                chunks[c.x, c.z].modifications.Enqueue(v);
            }
        }
        applyingModifications = false;
    }

    public ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.chunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.chunkWidth);
        return new ChunkCoord(x, z);
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.chunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.chunkWidth);
        return chunks[x, z];
    }


    //获取该坐标的方块是什么
    public virtual int GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);
        //不存在方块返回空气
        if (!IsVoxelInWorld(pos))
            return BlockID.Air;
        //最底层基岩
        if (yPos == 0)
            return BlockID.Bedrock;
        //获取生物群系
        // int index = 0;
        // if(Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 123456, 2f) > 0.5f){
        //     index = 1;
        // }
        // BiomeAttributes biome = biomes[index];

        int solidGroundHeight = 42;
        float sumOfHeight = 0;
        int count = 0;
        float strongestWeight = 0;
        int strongestBiomeIndex = 0;
        for (int i = 0; i < biomes.Length; ++i)
        {
            float weight = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[i].offset, biomes[i].scale);
            if (weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestBiomeIndex = i;
            }
            float height = biomes[i].terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biomes[i].terrainScale) * weight;
            if (height > 0)
            {
                sumOfHeight += height;
                count++;
            }
        }
        BiomeAttributes biome = biomes[strongestBiomeIndex];
        sumOfHeight /= count;

        //获取坐标实际高度
        int terrainHeight = Mathf.FloorToInt(sumOfHeight) + solidGroundHeight;
        // int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale)) + biome.solidGroundHeight;
        //判断这个地方是什么
        int voxelValue = BlockID.Air;
        if (yPos == terrainHeight)
            voxelValue = biome.surfaceBlock;
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
            voxelValue = biome.subSurfaceBlock;
        else if (yPos > terrainHeight)
            return BlockID.Air;
        else
            voxelValue = BlockID.Stone;

        if (voxelValue == BlockID.Stone)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (yPos >= lode.minHeight && yPos <= lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                        voxelValue = lode.blockID;
                }
            }
        }

        //树木生成
        if (yPos == terrainHeight && biome.placeMajorFlora)
        {
            if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.majorFloraZoneScale) > biome.majorFloraZoneThreshold)
            {
                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.majorFloraPlacementScale) > biome.majorFloraPlacementThreshold)
                {
                    modifications.Enqueue(Structure.GenerateMajorFlora(biome.majorFloraIndex, pos, biome.minHeight, biome.maxHeight));
                }
            }
        }

        return voxelValue;
    }

    bool IsChunkInWorld(ChunkCoord coord)
    {

        if (coord.x >= 0 && coord.x < VoxelData.worldSizeInChunks && coord.z >= 0 && coord.z < VoxelData.worldSizeInChunks)
            return true;
        else
            return false;

    }

    public bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.worldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.chunkHeight && pos.z >= 0 && pos.z < VoxelData.worldSizeInVoxels)
            return true;
        else
            return false;
    }

    //判断一个浮点坐标是否在实体方块内
    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);
        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y >= VoxelData.chunkHeight)
        {
            return false;
        }
        Chunk chunkTemp = chunks[thisChunk.x, thisChunk.z];
        if (chunkTemp != null && chunkTemp.isEditable)
        {
            return blockTypes[chunkTemp.GetVoxelFromGlobalVector3(pos).id].isSolid;
        }
        return blockTypes[GetVoxel(pos)].isSolid;
    }

    //判断是否为可透视方块
    public VoxelState GetVoxelState(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);
        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y >= VoxelData.chunkHeight)
        {
            return null;
        }
        Chunk chunkTemp = chunks[thisChunk.x, thisChunk.z];
        if (chunkTemp != null && chunkTemp.isEditable)
        {
            return chunkTemp.GetVoxelFromGlobalVector3(pos);
        }
        return new VoxelState(GetVoxel(pos));
    }
}

public class VoxelMod
{
    public Vector3 position;
    public byte id;
    public VoxelMod()
    {
        position = new Vector3();
        id = 0;
    }

    public VoxelMod(Vector3 _position, byte _id)
    {
        position = _position;
        id = _id;
    }
}

