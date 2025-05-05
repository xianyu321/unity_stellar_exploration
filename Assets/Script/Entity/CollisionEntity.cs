

using UnityEngine;
using UnityEngine.UI;

public class CollisionEntity : MonoBehaviour
{
    //物品属性
    public Vector3 pos{
        get{
            return transform.position;
        }
    }
    public Vector3 size = new(0.5f, 1.8f, 0.5f);
    public float TOP
    {
        get
        {
            return pos.y + size.y;
        }
    }
    public float BOTTOM
    {
        get
        {
            return pos.y;
        }
    }

    public static Vector3[] downPoint = new Vector3[4]{
        new(0, 0, 0),
        new(0, 0, 1),
        new(1, 0, 0),
        new(1, 0, 1),
    };

    public static Vector3[] upPoint = new Vector3[4]{
        new(0, 1, 0),
        new(0, 1, 1),
        new(1, 1, 0),
        new(1, 1, 1),
    };

    public float ChunkUpSpeed(WorldEntity world, float speed)
    {
        float res = speed;
        foreach (Vector3 point in upPoint)
        {
            Vector3Int coord = VectorTools.Vct3ToVec3Int(pos + Vector3.Scale(size, point));
            for (int i = 1; i < speed + 1; ++i)
            {
                coord.y += 1;
                BlockEntity block = world.GetBlock(coord);
                if (block is not null && block.IsEntity())
                {
                    float dis = coord.y - TOP;
                    if (dis < res)
                    {
                        res = dis;
                    }
                    break;
                }
            }
        }
        return res;
    }

    public float ChunkDownSpeed(WorldEntity world, float speed)
    {
        float res = speed;
        foreach (Vector3 point in downPoint)
        {
            Vector3Int coord = VectorTools.Vct3ToVec3Int(pos + Vector3.Scale(size, point));
            if(world.GetChunkByBlockCoord(coord.x, coord.z) == null){
                return 0;
            }
            for (int i = 1; i < -speed + 1; ++i)
            {
                coord.y -= 1;
                BlockEntity block = world.GetBlock(coord);
                if (block is not null && block.IsEntity())
                {
                    float dis = coord.y + 1 - BOTTOM;
                    if (dis > res)
                    {
                        res = dis;
                    }
                    break;
                }
            }
        }
        return res;
    }
    //前后左右能否移动
    float playerHeightEps = 0.5f;
    public static int[,] moveFront = new int[2, 2]{
        {0, 1},
        {1, 1},
    };
    public static int[,] moveBack = new int[2, 2]{
        {0, 0},
        {1, 0}
    };
    public static int[,] moveLeft = new int[2, 2]{
        {0, 0},
        {0, 1}
    };
    public static int[,] moveRight = new int[2, 2]{
        {1, 0},
        {1, 1}
    };
    public bool ChunkMoveXZ(WorldEntity world ,int[,] dxz, Vector3 move){
        Vector3 nextMove = new Vector3(pos.x + move.x, pos.y, pos.z + move.z);
        for (int i = 0; i < 2; ++i)
        {
            if (world.GetBlock(new Vector3(
                nextMove.x + size.x * dxz[i, 0],
                nextMove.y + size.y,
                nextMove.z + size.z * dxz[i, 1]
            )).IsEntity())
            {
                return true;
            }
            for (float h = 0; h < size.y; h += playerHeightEps)
            {
                if (world.GetBlock(new Vector3(
                    nextMove.x + size.x * dxz[i, 0],
                    nextMove.y + h,
                    nextMove.z + size.z * dxz[i, 1]
                )).IsEntity())
                {
                    return true;
                }
            }
        }
        return false;
    }
}