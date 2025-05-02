using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    public GameObject dontDestroyPrefab;
    void Awake()
    {
        if(NetworkManager.Singleton is null){
            GameObject gameObject = Instantiate(dontDestroyPrefab);
            DontDestroyOnLoad(gameObject);
        }
    }
}
