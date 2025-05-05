using System.Collections.Generic;

public class WorldManager
{
    private Dictionary<string, WorldEntity> worlds;
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
    WorldManager(){
        worlds = new();
        BlockManager.Instance.Init();
    }

    public WorldEntity GetOrCreateWorld(WorldGenerator worldGenerator){
        string worldName = worldGenerator.GetType().Name;
        if(worlds.ContainsKey(worldName)){
            return worlds[worldName];
        }
        worlds[worldName] = new(worldGenerator);
        return worlds[worldName];
    }
    public WorldEntity GetWorld(string worldName){
        if(worlds.ContainsKey(worldName)){
            return worlds[worldName];
        }
        if(worldName == "WorldGenerator"){
            worlds[worldName] = new(new WorldGenerator());
        }
        return worlds[worldName];
    }

    string saveName = "testsave1";

    public void InitData(string _saveName){
        saveName = _saveName;
        worlds.Clear();
    }

    public string GetChunksPath(WorldGenerator worldGenerator){
        string worldName = worldGenerator.GetType().Name;
        string chunksPath = PathLoader.GetChunksPath(saveName, worldName);
        return chunksPath;
    }
}