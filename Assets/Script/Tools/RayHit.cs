using UnityEngine;

public static class RayHit{
    public static bool IntersectRayWithVoxel(Ray ray, Vector3Int voxelPos, out RaycastHit hitInfo, float maxRayDistance = 10f)
    {
        // 定义包围盒 min/max
        Vector3 min = new Vector3(voxelPos.x, voxelPos.y, voxelPos.z);
        Vector3 max = min + Vector3.one;

        Vector3 dirfrac = new Vector3(
            ray.direction.x == 0 ? float.MaxValue : 1.0f / ray.direction.x,
            ray.direction.y == 0 ? float.MaxValue : 1.0f / ray.direction.y,
            ray.direction.z == 0 ? float.MaxValue : 1.0f / ray.direction.z
        );

        // 计算 t 值
        float t1 = (min.x - ray.origin.x) * dirfrac.x;
        float t2 = (max.x - ray.origin.x) * dirfrac.x;
        float t3 = (min.y - ray.origin.y) * dirfrac.y;
        float t4 = (max.y - ray.origin.y) * dirfrac.y;
        float t5 = (min.z - ray.origin.z) * dirfrac.z;
        float t6 = (max.z - ray.origin.z) * dirfrac.z;

        float tmin = Mathf.Max(Mathf.Max(Mathf.Min(t1, t2), Mathf.Min(t3, t4)), Mathf.Min(t5, t6));
        float tmax = Mathf.Min(Mathf.Min(Mathf.Max(t1, t2), Mathf.Max(t3, t4)), Mathf.Max(t5, t6));

        // 如果 tmax < 0，则射线朝向反方向
        if (tmax < 0)
        {
            hitInfo = new RaycastHit();
            return false;
        }

        // 如果没有交集（tmin > tmax）
        if (tmin > tmax)
        {
            hitInfo = new RaycastHit();
            return false;
        }

        // 距离小于最大交互距离（例如10个单位）
        float hitDistance = tmin;
        if (hitDistance > maxRayDistance)
        {
            hitInfo = new RaycastHit();
            return false;
        }

        // 计算命中点
        Vector3 hitPoint = ray.origin + ray.direction * hitDistance;

        // 判断命中的是哪一面
        int hitFace = GetHitFace(ray.direction, hitPoint, voxelPos);

        hitInfo = new RaycastHit()
        {
            point = hitPoint,
            distance = hitDistance,
            normal = GetNormalFromFace(hitFace),
        };

        return true;
    }

    private static int GetHitFace(Vector3 rayDirection, Vector3 hitPoint, Vector3Int voxelPos)
    {
        // 检查哪个坐标最接近该面
        float eps = 0.01f;
        if (Mathf.Abs(hitPoint.x - voxelPos.x) < eps)
            return 4; // 左
        else if (Mathf.Abs(hitPoint.x - (voxelPos.x + 1)) < eps)
            return 5; // 右

        if (Mathf.Abs(hitPoint.y - voxelPos.y) < eps)
            return 3; // 下
        else if (Mathf.Abs(hitPoint.y - (voxelPos.y + 1)) < eps)
            return 2; // 上

        if (Mathf.Abs(hitPoint.z - voxelPos.z) < eps)
            return 0; // 后
        else if (Mathf.Abs(hitPoint.z - (voxelPos.z + 1)) < eps)
            return 1; // 前

        return -1;
    }

    private static Vector3 GetNormalFromFace(int face)
    {
        switch (face)
        {
            case 0: return Vector3.back;   // 后
            case 1: return Vector3.forward; // 前
            case 2: return Vector3.up;     // 上
            case 3: return Vector3.down;   // 下
            case 4: return Vector3.left;   // 左
            case 5: return Vector3.right;  // 右
            default: return Vector3.zero;
        }
    }
}