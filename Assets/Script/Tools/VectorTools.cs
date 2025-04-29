

using UnityEngine;

public static class VectorTools{
    public static Vector3Int Vct3ToVec3Int(Vector3 vec3){
        int x = Mathf.FloorToInt(vec3.x);
        int y = Mathf.FloorToInt(vec3.y);
        int z = Mathf.FloorToInt(vec3.z);
        return new(x, y, z);
    }
}
