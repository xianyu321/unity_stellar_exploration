using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//体素类
public static class VoxelData
{
    public static readonly int chunkHeight = 128;//一个区块的高度
    public static readonly int chunkWidth = 16;//一个区块的宽度
    public static readonly int worldSizeInChunks = 20;//世界总区块大小
    
    public static readonly int textureAtlasSizeInBlocks = 16;//像素图片宽度
    public static int worldSizeInVoxels //世界总大小
	{
		get { return worldSizeInChunks * chunkWidth; }
	}
    public static float normalizedBlockTextureSize//归一化方块材质列表
	{
		get { return 1f / (float)textureAtlasSizeInBlocks; }
	}

    public static float minLightLevel = 0.1f;
    public static float maxLightLevel = 0.9f;
    public static float lightFalloff = 0.1f;
    //voxel:体素;vert:顶点
    //正方体的八个顶点的坐标点
    public static readonly Vector3[] voxelVerts = new Vector3[8] {
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f),
    };

    //重心偏移量
    public static readonly Vector3 barycenterOffset = new Vector3(0.5f,0.5f,0.5f);

    //重心坐标距离点
    public static readonly Vector3[] voxelBarycenterVerts = new Vector3[8] {
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, 0.5f, -0.5f),
        new Vector3(-0.5f, 0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, 0.5f),
        new Vector3(0.5f, -0.5f, 0.5f),
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(-0.5f, 0.5f, 0.5f),
    };

    //体素六面方向上的节点
    public static readonly Vector3[] faceChecks = new Vector3[6] {
        new Vector3(0.0f, 0.0f, -1.0f), //后
        new Vector3(0.0f, 0.0f, 1.0f), //前
        new Vector3(0.0f, 1.0f, 0.0f), //上
        new Vector3(0.0f, -1.0f, 0.0f), //下
        new Vector3(-1.0f, 0.0f, 0.0f), //左
        new Vector3(1.0f, 0.0f, 0.0f), //右
    };

    //体素的六个面的顶点索引
    public static readonly int[,] voxelTris = new int[6, 4] {
        //正方形上三角形贴图，两个三角形顶点排序方式为 0 1 2 2 1 3
		{0, 3, 1, 2}, // Back Face
		{5, 6, 4, 7}, // Front Face
		{3, 7, 2, 6}, // Top Face
		{1, 5, 0, 4}, // Bottom Face
		{4, 7, 0, 3}, // Left Face
		{1, 2, 5, 6} // Right Face
	};
    //正方形像素点的四个点
    public static readonly Vector2[] voxelUvs = new Vector2[4] {
        new Vector2 (0.0f, 0.0f),
		new Vector2 (0.0f, 1.0f),
		new Vector2 (1.0f, 0.0f),
		new Vector2 (1.0f, 1.0f)
    };

    public static readonly int[,] triangleHash = new int[2,3]{
        {0,1,2},
        {2,1,3}
    };
}
