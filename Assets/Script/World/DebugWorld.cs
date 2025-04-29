using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugWorld : World
{
    public override int GetVoxel(Vector3 pos)
    {
        if (pos.y == 50)
        {
            return BlockID.GrassBlock;
        }
        return BlockID.Air;
    }
}
