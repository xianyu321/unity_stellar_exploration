using UnityEngine;

public static class Noise
{
    //position:坐标;offset:偏移量;scale:频率
    public static float Get2DPerlin(Vector2 position, float offset, float scale)
    {
        return Mathf.PerlinNoise((position.x + 0.1f) / VoxelData.chunkWidth * scale + offset, (position.y + 0.1f) / VoxelData.chunkWidth * scale + offset);
        // return Mathf.PerlinNoise((position.x + 0.1f) / VoxelData.chunkWidth * scale + offset, (position.y + 0.1f) / VoxelData.chunkWidth * scale + offset);
    }
    //threshold:门槛值
    public static bool Get3DPerlin(Vector3 position, float offset, float scale, float threshold)
    {
        return Get3DPerlinValue(position, offset, scale) > threshold;
    }

    public static float Get3DPerlinValue(Vector3 position, float offset, float scale)
    {
        float x = (position.x + offset + 0.1f) * scale;
        float y = (position.y + offset + 0.1f) * scale;
        float z = (position.z + offset + 0.1f) * scale;

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);
        return (AB + BC + AC + BA + CB + CA) / 6f;
    }
    public static float Generate3DFractalNoise(Vector3 position,float offset = 0, float scale = 1, int octaves = 4, float persistence = 0.5f)
    {
        float total = 0;
        float amplitude = 1;
        float maxAmplitude = 0;
        float freq = 1.0f;//初始频率

        for (int i = 0; i < octaves; i++)
        {
            total += Get3DPerlinValue(position, offset, freq * scale);
            maxAmplitude += amplitude;
            amplitude *= persistence;
            freq *= 2; // 每层频率翻倍
        }
        return total / maxAmplitude; 
    }
}
