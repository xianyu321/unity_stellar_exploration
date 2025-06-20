

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ChunkEntity
{
    public static readonly int chunkSize = 16;
    public static readonly int chunkHight = 128;
    public ChunkCoord chunkCoord;
    public BlockEntity[,,] blocks;
    public Biome[,] biome;
    public WorldEntity world;//区块所在世界
    public GameObject chunkObject;//区块对象 
    MeshRenderer meshRenderer;//mesh渲染器
    MeshFilter meshFilter;//mesh过滤器
    int vertexIndex = 0;//顶点个数
    List<Vector3> vertices;//顶点集合
    List<int> triangles;//三角形面集合
    List<Vector2> uvs;//贴图数组
    List<Vector3> normals;
    Material material; //材质
    //区块坐标
    public bool isLoad = false;
    public bool isShow = false;

    public ChunkEntity(ChunkCoord _chunkCoord, WorldEntity _world)
    {
        chunkObject = new GameObject();
        chunkCoord = _chunkCoord;
        world = _world;
        Init();
    }
    public void InitData()
    {
        blocks = new BlockEntity[chunkSize, chunkHight, chunkSize];
        biome = new Biome[chunkSize, chunkSize];
        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();
        normals = new List<Vector3>();
    }

    public void Init()
    {
        //更新区块对象坐标
        chunkObject.transform.position = new Vector3(chunkCoord.x * VoxelData.chunkWidth, 0f, chunkCoord.z * VoxelData.chunkWidth);
        chunkObject.name = chunkCoord.x + ", " + chunkCoord.z;
        //为区块对象添加mesh
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        material = TextureManager.Instance.blockMaterial;
        meshRenderer.material = material;
    }
    //更新整个区块
    public void UpdateChunk()
    {
        ClearMeshData();
        for (int y = 0; y < chunkHight; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    UpdateMeshData(new Vector3(x, y, z));
                }
            }
        }
        TaskRunner.Instance.TaskRunMain(() =>
        {
            CreateMesh();
            isShow = true;
        });
    }
    static Vector2Int[] arr = new Vector2Int[]{
        new(0, 1),
        new(0, -1),
        new(-1, 0),
        new(1, 0),
    };
    public void BrokenBlock(BlockCoord blockCoord)
    {
        if (IsVoxelInChunk(blockCoord))
        {
            BlockEntity block = GetBlock(blockCoord.x, blockCoord.y, blockCoord.z);
            if (block.IsEntity() && block.OnBroken(0))
            {
                blocks[blockCoord.x, blockCoord.y, blockCoord.z] = null;
                foreach (var item in arr)
                {
                    int x = blockCoord.x + item.x;
                    int z = blockCoord.z + item.y;
                    if (!IsVoxelInChunk(x, 0, z))
                    {
                        ChunkEntity chunk = world.GetChunk(chunkCoord.x + item.x, chunkCoord.z + item.y);
                        if (chunk != null && chunk.isShow)
                        {
                            TaskRunner.Instance.RunTask(() =>
                            {
                                chunk.UpdateChunk();
                            });
                        }
                    }
                }
            }
        }

    }
    public bool PlaceBlock(BlockCoord blockCoord, int blockId)
    {
        if (IsVoxelInChunk(blockCoord))
        {
            if (GetBlock(blockCoord.x, blockCoord.y, blockCoord.z).IsEntity())
            {
                return false;
            }
            blocks[blockCoord.x, blockCoord.y, blockCoord.z] = new(this, blockCoord, blockId);
            UpdateChunk();
            return true;
        }
        return false;
    }

    //清空贴图数据
    public void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        normals.Clear();
    }
    Mesh mesh = new Mesh();
    //创建mesh并装载数据
    public void CreateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.subMeshCount = 2;
        mesh.uv = uvs.ToArray();
        mesh.normals = normals.ToArray();
        meshFilter.mesh = mesh;
    }

    //更新mesh数据
    void UpdateMeshData(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        int blockID = GetBlockID(x, y, z);
        if (blockID < 0)
        {
            return;
        }
        for (int p = 0; p < 6; p++)
        {
            if (CheckVoxel(pos + VoxelData.faceChecks[p]))
            {
                int texIndex = BlockManager.Instance.Blocks[blockID].faces.GetTextureID(p);
                for (int i = 0; i < 4; ++i)
                {
                    vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, i]]);
                    normals.Add(VoxelData.faceChecks[p]);
                }
                AddTexture(texIndex);
                CWTriangle(ref triangles);
                vertexIndex += 4;
            }
        }
    }

    //顺时针三角形
    void CWTriangle(ref List<int> list)
    {
        for (int i = 0; i < 2; ++i)
        {
            for (int j = 0; j < 3; ++j)
            {
                list.Add(vertexIndex + VoxelData.triangleHash[i, j]);
            }
        }
    }
    //判断所在位置是否为可视
    bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        if (!IsVoxelInChunk(x, y, z))
            return !world.IsBlock(x + chunkCoord.x * chunkSize, y, z + chunkCoord.z * chunkSize);
        return blocks[x, y, z] is null;
    }
    //是否在本区块内
    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > chunkSize - 1 || y < 0 || y > chunkHight - 1 || z < 0 || z > chunkSize - 1)
            return false;
        return true;
    }
    bool IsVoxelInChunk(BlockCoord coord)
    {
        if (coord.x < 0 || coord.x > chunkSize - 1 || coord.y < 0 || coord.y > chunkHight - 1 || coord.z < 0 || coord.z > chunkSize - 1)
            return false;
        return true;
    }

    //加入贴图
    void AddTexture(int textureID)
    {
        int rols = TextureManager.Instance.textureRols;
        float y = textureID / rols;
        float x = textureID - (y * rols);
        float texSize = 1f / rols;

        x *= texSize;
        y *= texSize;
        y = 1f - y - texSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + texSize));
        uvs.Add(new Vector2(x + texSize, y));
        uvs.Add(new Vector2(x + texSize, y + texSize));
    }

    //y是高度
    public BlockEntity GetBlock(int x, int y, int z)
    {
        if (x >= 0 && x < blocks.GetLength(0) &&
            y >= 0 && y < blocks.GetLength(1) &&
            z >= 0 && z < blocks.GetLength(2))
        {
            if (blocks[x, y, z] is null)
            {
                return new();
            }
            return blocks[x, y, z];
        }
        return new();
    }

    int GetBlockID(int x, int y, int z)
    {
        BlockEntity block = GetBlock(x, y, z);
        if (!block.IsEntity())
        {
            return -1;
        }
        return block.blockID;
    }
    public void SetBlock(int x, int y, int z, BlockEntity block)
    {
        if (x >= 0 && x < blocks.GetLength(0) &&
            y >= 0 && y < blocks.GetLength(1) &&
            z >= 0 && z < blocks.GetLength(2))
        {
            blocks[x, y, z] = block;
        }
    }

    public void ExecuteAll(Action<BlockEntity> action)
    {
        for (int x = 0; x < chunkSize; ++x)
        {
            for (int y = 0; y < chunkHight; ++y)
            {
                for (int z = 0; z < chunkSize; ++z)
                {
                    action(blocks[x, y, z]);
                }
            }
        }
    }

    public void ClearMesh()
    {

    }
}

//区块坐标对象
public class ChunkCoord
{
    public int x;
    public int z;

    public ChunkCoord()
    {
        x = z = 0;
    }

    public ChunkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public ChunkCoord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);
        if (xCheck >= 0)
            x = xCheck / VoxelData.chunkWidth;
        else
            x = (xCheck + 1) / VoxelData.chunkWidth - 1;
        if (zCheck >= 0)
            z = zCheck / VoxelData.chunkWidth;
        else
            x = (zCheck + 1) / VoxelData.chunkWidth - 1;
    }

    public bool Equals(ChunkCoord other)
    {
        if (other == null)
            return false;
        else if (other.x == x && other.z == z)
            return true;
        else
            return false;
    }
}