using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static Queue<VoxelMod> GenerateMajorFlora(int index,Vector3 position, int minTreeHeight, int maxTreeHeight){
        switch(index){
            case 0:
                return MakeTree(position,minTreeHeight,maxTreeHeight);
            case 1:
                return MakeCactis(position,minTreeHeight,maxTreeHeight);
        }
        return null;
    }

    public static Queue<VoxelMod> MakeTree(Vector3 position, int minTreeHeight, int maxTreeHeight){
        Queue<VoxelMod> queue = new Queue<VoxelMod>();
        int height = (int) (maxTreeHeight*Noise.Get2DPerlin(new Vector2(position.x, position.z), 250f, 3f));
        if(height < minTreeHeight){
            height = minTreeHeight;
        }

        for(int i = 1 ; i < height; ++i){
            queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), BlockID.Log));
        }
        queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + height, position.z), BlockID.Leaves));
        return queue;
    }
    public static Queue<VoxelMod> MakeCactis(Vector3 position, int minTreeHeight, int maxTreeHeight){
        Queue<VoxelMod> queue = new Queue<VoxelMod>();
        int height = (int) (maxTreeHeight*Noise.Get2DPerlin(new Vector2(position.x, position.z), 250f, 3f));
        if(height < minTreeHeight){
            height = minTreeHeight;
        }

        for(int i = 1 ; i < height; ++i){
            queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), BlockID.Cactus));
        }
        return queue;
    }
}
