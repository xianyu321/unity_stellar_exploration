using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
//方块
public class Chunk
{
    World world;//区块所在世界
    [Rename("区块坐标")]
    public ChunkCoord chunkCoord;//区块坐标
    GameObject chunkObject;//区块对象 
    MeshRenderer meshRenderer;//mesh渲染器
    MeshFilter meshFilter;//mesh过滤器
    MeshCollider meshCollider;//mesh碰撞器
    int vertexIndex = 0;//顶点个数
    List<Vector3> vertices = new List<Vector3>();//顶点集合
    List<int> triangles = new List<int>();//三角形面集合
    List<Vector2> uvs = new List<Vector2>();//贴图数组
    List<Vector3> normals = new List<Vector3>();
    List<int> transparentTriangles = new List<int>();//可透视贴图
    Material[] materials = new Material[2]; //材质数组
    public VoxelState[,,] voxelMap = new VoxelState[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];//判断有没有方块
    private bool _isActive;
    private bool isVoxelMapPopulated = false;
    // private bool threadLocked;
    //区块坐标
    public Vector3 position;

    public Queue<VoxelMod> modifications = new Queue<VoxelMod>();

    List<Color> colors = new List<Color>();

    //是否为实体
    public bool isActive
    {
        // get { return chunkObject.activeSelf; }
        get
        {
            return _isActive;
        }
        set
        {
            _isActive = value;
            if (chunkObject != null)
                chunkObject.SetActive(value);
        }

    }

    //是否可以调用
    public bool isEditable{
        get{
            if(!isVoxelMapPopulated){
                return false;
            }else{
                return true;
            }
        }
    }

    // public static int size = 5;

    //初始化区块
    public Chunk(World _world, ChunkCoord _chunkCoord)
    {
        world = _world;
        chunkCoord = _chunkCoord;
        isActive = true;
        Init();
    }

    public void Init()
    {
        chunkObject = new GameObject();
        //更新区块对象坐标
        chunkObject.transform.position = new Vector3(chunkCoord.x * VoxelData.chunkWidth, 0f, chunkCoord.z * VoxelData.chunkWidth);
        chunkObject.name = chunkCoord.x + ", " + chunkCoord.z;
        //把区块对象放入世界的子组件中
        chunkObject.transform.SetParent(world.transform);
        //为区块对象添加mesh
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        //为mesh加载材质
        materials[0] =world.material;
        materials[1] = world.transparentMaterial;
        meshRenderer.materials = materials;
        // meshRenderer.material = world.material;

        position = chunkObject.transform.position;
        PopulateVoxelMap();
    }

    //获取区块的全部方块索引
    void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.chunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.chunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    voxelMap[x, y, z] = new VoxelState(world.GetVoxel(new Vector3(x, y, z) + position));
                }
            }
        }

        isVoxelMapPopulated = true;
        lock(world.ChunkUpdateThreadLock){
            world.chunksToUpdate.Add(this);
        }
    }

    //更新整个区块
    public void UpdateChunk()
    {
        while(modifications.Count > 0){
            VoxelMod v = modifications.Dequeue();
            Vector3 pos = v.position - position;
            voxelMap[(int)pos.x, (int)pos.y, (int)pos.z].id = v.id;
        }

        ClearMeshData();

        CalculateLight();

        for (int y = 0; y < VoxelData.chunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.chunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.chunkWidth; z++)
                {
                    if (world.blockTypes[voxelMap[x, y, z].id].isSolid)
                        UpdateMeshData(new Vector3(x, y, z));
                }
            }
        }
        world.chunksToDraw.Enqueue(this);
        // CreateMesh();
    }

    //清空贴图数据
    void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        uvs.Clear();
        colors.Clear();
        normals.Clear();
    }

    //创建mesh并装载数据
    public void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        // mesh.triangles = triangles.ToArray();
        mesh.subMeshCount = 2;

        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);

        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();
        mesh.normals = normals.ToArray();
        // mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    //更新mesh数据
    void UpdateMeshData(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        int blockID = voxelMap[x, y, z].id;
        // bool renderNeighborFaces = world.blocktypes[blockID].renderNeighborFaces;

        for (int p = 0; p < 6; p++)
        {
            VoxelState neighbor = CheckVoxel(pos + VoxelData.faceChecks[p]);
            
            // if (isTransparent)
            // {
            //     AddVertice(pos, p);
            //     AddTexture(world.blocktypes[blockID].GetTextureID(p));
            //     CWTriangle(ref transparentTriangles);
            //     vertexIndex += 4;
            //     //方块内贴图
            //     // AddVertice(pos, p, 0.99f);
            //     // AddTexture(world.blocktypes[blockID].GetTextureID(p));
            //     // CCWTriangle(ref transparentTriangles);
            //     // vertexIndex += 4;
            //     continue;
            // }
            if (neighbor != null && world.blockTypes[neighbor.id].renderNeighborFaces)
            {
                for(int i = 0; i < 4; ++i){
                    vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, i]]);
                    normals.Add(VoxelData.faceChecks[p]);
                }
                AddTexture(world.blockTypes[blockID].GetTextureID(p));
                if(world.blockTypes[neighbor.id].renderNeighborFaces){

                }
                CWTriangle(ref triangles);
                vertexIndex += 4;   

                // float lightLevel = 0f;

                // int yPos = (int)pos.y + 1;
                // bool isShade = false;
                // while(yPos < VoxelData.chunkHeight){
                //     if(voxelMap[(int)pos.x,yPos,(int)pos.z].id != 0){
                //         isShade = true;
                //         break;
                //     }
                //     yPos++;
                // }

                // if(isShade){
                //     lightLevel = 0.4f;
                // }
                float lightLevel = neighbor.globalLightPercent;
                colors.Add(new Color(0,0,0,lightLevel));
                colors.Add(new Color(0,0,0,lightLevel));
                colors.Add(new Color(0,0,0,lightLevel));
                colors.Add(new Color(0,0,0,lightLevel));
            }
        }
    }

    void CalculateLight(){

        Queue<Vector3Int> litVoxels = new Queue<Vector3Int>();

        for (int x = 0; x < VoxelData.chunkWidth; x++)
        {
            for (int z = 0; z < VoxelData.chunkWidth; z++)
            {
                float lightRay = 1f;
                for(int y = VoxelData.chunkHeight - 1; y >= 0; --y){
                    VoxelState thisVoxel = voxelMap[x,y,z];
                    if(thisVoxel.id > 0){
                        lightRay *= world.blockTypes[thisVoxel.id].transparency;
                    }
                    thisVoxel.globalLightPercent = lightRay;
                    voxelMap[x,y,z] = thisVoxel;


                    if(lightRay > VoxelData.lightFalloff){
                        litVoxels.Enqueue(new Vector3Int(x,y,z));
                    }
                }
            }
        }

        while(litVoxels.Count > 0){
            Vector3Int v = litVoxels.Dequeue();
            for(int p = 0; p < 6; ++p){
                Vector3 currentVoxel = v + VoxelData.faceChecks[p];
                Vector3Int neighbor = new Vector3Int((int)currentVoxel.x,(int)currentVoxel.y,(int)currentVoxel.z);

                if(IsVoxelInChunk(neighbor.x,neighbor.y,neighbor.z)){
                    if(voxelMap[neighbor.x,neighbor.y,neighbor.z].globalLightPercent < voxelMap[v.x,v.y,v.z].globalLightPercent - VoxelData.lightFalloff){
                        voxelMap[neighbor.x,neighbor.y,neighbor.z].globalLightPercent = voxelMap[v.x,v.y,v.z].globalLightPercent - VoxelData.lightFalloff;
                        if(voxelMap[neighbor.x,neighbor.y,neighbor.z].globalLightPercent > VoxelData.lightFalloff){
                            litVoxels.Enqueue(neighbor);
                        }
                    }
                }
            }
        }
    }

    //根据重心实现内接贴图
    void AddVertice(Vector3 pos, int p, float indentation){
        pos = pos + VoxelData.barycenterOffset;
        for(int i = 0; i < 4; ++i){
            vertices.Add(pos + VoxelData.voxelBarycenterVerts[VoxelData.voxelTris[p, i]] * indentation);
        }
    }

    //顺时针三角形
    void CWTriangle(ref List<int> list){
        for(int i = 0 ; i < 2; ++i){
            for(int j = 0; j < 3; ++j){
                list.Add(vertexIndex + VoxelData.triangleHash[i,j]);
            }
        }
    }
    //逆时针三角形
    void CCWTriangle(ref List<int> list){
        for(int i = 0 ; i < 2; ++i){
            for(int j = 2; j >= 0; --j){
                list.Add(vertexIndex + VoxelData.triangleHash[i,j]);
            }
        }
    }




    //更新一个坐标的方块
    public void EditVoxel(Vector3 pos, int newId = 0)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        voxelMap[xCheck, yCheck, zCheck].id = newId;

        lock(world.ChunkUpdateThreadLock){
            world.chunksToUpdate.Insert(0, this);
            UpdateSurroundingVoxels(xCheck, yCheck, zCheck);
        }
    }

    //更新方块后其他面的贴图
    void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);
        for (int p = 0; p < 6; p++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[p];
            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                world.chunksToUpdate.Insert(0, world.GetChunkFromVector3(currentVoxel + position));
            }
        }

    }

    //判断所在位置是否为可视
    VoxelState CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        if (!IsVoxelInChunk(x, y, z))
            return world.GetVoxelState(pos + position);
        return voxelMap[x, y, z];
    }

    //是否在本区块内
    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.chunkWidth - 1 || y < 0 || y > VoxelData.chunkHeight - 1 || z < 0 || z > VoxelData.chunkWidth - 1)
            return false;
        return true;
    }

    //加入贴图
    void AddTexture(int textureID)
    {
        float y = textureID / VoxelData.textureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.textureAtlasSizeInBlocks);

        x *= VoxelData.normalizedBlockTextureSize;
        y *= VoxelData.normalizedBlockTextureSize;
        y = 1f - y - VoxelData.normalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.normalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.normalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.normalizedBlockTextureSize, y + VoxelData.normalizedBlockTextureSize));
    }



    public VoxelState GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(position.x);
        zCheck -= Mathf.FloorToInt(position.z);

        if (!IsVoxelInChunk(xCheck, yCheck, zCheck))
        {
            Debug.Log(chunkObject.transform.position.x);
            return new VoxelState(0);
        }
        return voxelMap[xCheck, yCheck, zCheck];
    }
}


public class VoxelState{
    public int id;
    public float globalLightPercent;
    public VoxelState(){
        id = 0; 
        globalLightPercent = 0f;
    }

    public VoxelState(int _id){
        id = _id; 
        globalLightPercent = 0f;
    }
}