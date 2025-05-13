using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DropManager : MonoBehaviour{
    public GameObject dropItemPrefab;
    public GameObject dropItemFather;
    void Awake()
    {
        DropItemManager.Instance.Init(dropItemPrefab, dropItemFather);
    }
}

public class DropItemManager{
    // 单例实例
    private static DropItemManager _instance;
    // 全局访问点
    public static DropItemManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new DropItemManager();
            }
            return _instance;
        }
    }
    public GameObject dropItem;
    public GameObject dropFather;
    public List<DropItemEntity> dropItems = new List<DropItemEntity>();
    public void Init(GameObject o, GameObject fa){
        dropItem = o;
        dropFather = fa;
    }
    public void GeneratorItem(int itemID, Vector3 itemPos, WorldEntity world){
        GameObject item = Object.Instantiate(dropItem);
        DropItemEntity dropCom = item.GetComponent<DropItemEntity>();
        dropItems.Add(dropCom);
        dropCom.SetData(itemID, itemPos, world);
        dropCom.SetParent(dropFather.transform);
    }

    public DropItemEntity GetDropByPlayer(Vector3 playerCenter, Vector3 size){
        foreach(DropItemEntity item in dropItems){
            if(item.center.x < playerCenter.x - size.x || item.center.x > playerCenter.x + size.x){
                continue;
            }
            if(item.center.y < playerCenter.y - size.y || item.center.y > playerCenter.y + size.y){
                continue;
            }
            if(item.center.z < playerCenter.z - size.z || item.center.z > playerCenter.z + size.z){
                continue;
            }
            return item;
        }
        return null;
    }
    public void RemoveListItem(DropItemEntity dropItem){
        Object.Destroy(dropItem.gameObject);
        dropItems.Remove(dropItem);
    }
}