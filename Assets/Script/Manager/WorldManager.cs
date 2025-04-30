


using System.Collections.Generic;
using UnityEngine;

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
        // GetOrCreateWorld(new());
    }

    public WorldEntity GetOrCreateWorld(WorldGenerator worldGenerator){
        string key = worldGenerator.GetType().Name;
        if(worlds.ContainsKey(key)){
            return worlds[key];
        }
        worlds[key] = new(worldGenerator);
        return worlds[key];
    }
}