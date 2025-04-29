


using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldManager
{
    
    private static WorldManager _instance;
    public static WorldManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new WorldManager();
            }
            return _instance;
        }
    }

    public void GenerateByPlayerPos(){
        WorldGenerator worldGenerator = new();
        Vector3 pos = new Vector3(0, 0, 0);
        ChunkCoord coord = ChunkManager.ToChunkCoord(pos);
        int width = 4;
        for(int x = coord.x - width; x <= coord.x + width; ++ x){
            for(int z = coord.z - width; z <= coord.z + width; ++z){
                ChunkEntity chunk = new(new(x,z), worldGenerator);
                worldGenerator.GenerateChunk(chunk);
                GameObject chunks = GameObject.Find("Chunks");
                chunk.chunkObject.transform.SetParent(chunks.transform);
                // chunk.UpdateChunk();
            }
        }
        for(int x = coord.x - width; x <= coord.x + width; ++ x){
            for(int z = coord.z - width; z <= coord.z + width; ++z){
                ChunkEntity chunk = worldGenerator.GetChunk(new(x, z));
                chunk.UpdateChunk();
            }
        }
    }
    public void GenerateChunk(){

    }
}