using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItemEntity : MonoBehaviour
{
    // Start is called before the first frame update
    Material material;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    List<Vector3> normals = new List<Vector3>();
    Mesh mesh;
    public int blockID;
    Transform parent;
    WorldEntity world;

    private void Start()
    {
        transform.SetParent(parent);
        mesh = new Mesh();
        Init();
        InitMesh();
    }
    public ItemData data;

    public void SetData(int _blockID, Vector3 position, WorldEntity _world){
        data = new(_blockID, 1);
        transform.position = position;
        blockID = _blockID;
        world = _world;
    }

    public void SetData(ItemData _data){
        data = _data;
    }


    public CollisionEntity collisionEntity;
    public Vector3 center{
        get{
            return collisionEntity.pos + collisionEntity.size / 2;
        }
    }
    void FixedUpdate()
    {
        float y = collisionEntity.ChunkDownSpeed(world, -2 * Time.fixedDeltaTime);
        transform.Translate(Vector3.up * y, Space.World);
    }

    void Init()
    {
        //为区块对象添加mesh
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        material = TextureManager.Instance.blockMaterial;
        meshRenderer.material = material;
    }
    public void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        normals.Clear();
    }
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

    void InitMesh()
    {
        for (int p = 0; p < 6; p++)
        {
            int texIndex = BlockManager.Instance.Blocks[data.id].faces.GetTextureID(p);
            for (int i = 0; i < 4; ++i)
            {
                vertices.Add(Vector3.zero + VoxelData.voxelVerts[VoxelData.voxelTris[p, i]]);
                normals.Add(VoxelData.faceChecks[p]);
            }
            AddTexture(texIndex);
            CWTriangle(ref triangles);
            vertexIndex += 4;
        }
        CreateMesh();
    }
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

    public void SetParent(Transform transform)
    {
        parent = transform;
    }
}
