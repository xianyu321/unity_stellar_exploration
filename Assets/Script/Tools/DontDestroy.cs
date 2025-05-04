using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    public GameObject dontDestroyPrefab;
    public static SynchronizationContext MainThreadSyncContext { get; private set; }
    void Awake()
    {
        if(NetworkManager.Singleton is null){
            MainThreadSyncContext = SynchronizationContext.Current;
            GameObject gameObject = Instantiate(dontDestroyPrefab);
            DontDestroyOnLoad(gameObject);
        }
    }
}
