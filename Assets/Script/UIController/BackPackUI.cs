using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackPackUI : MonoBehaviour
{
    public PlayerEntity player;
    public Transform itemFather;
    List<UIItemEntity> UIItems = new List<UIItemEntity>();
    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform item in itemFather){
            UIItems.Add(item.GetComponent<UIItemEntity>());
        }
        for(int i = 0; i < UIItems.Count; ++i){
            UIItems[i].InitData(player.items[i]);
        }
    }
}
