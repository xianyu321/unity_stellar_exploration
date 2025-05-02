using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameEscMenu : MonoBehaviour
{
    public Button openLANBtn;
    public GameObject player;
    public void OnOpenLANClicked(){
        Debug.Log("OpenLan");
        if (player != null)
        {
            // 确保玩家对象有一个 NetworkObject 组件
            Destroy(player);
        }
        NetworkManager.Singleton.StartHost();
    }

    public void OnOptionClicked(){
        Debug.Log("Option");
    }

    public void OnExitClick(){
        Debug.Log("Exit");
    }
}
