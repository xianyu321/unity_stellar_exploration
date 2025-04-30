using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ToolBar : MonoBehaviour
{
    public UIItemEntity[] uiItems;
    public PlayerEntity player;
    public RectTransform highlight;
    public int itemSelect = 0;
    // World world;
    Transform[] item;
    public void Initialize(PlayerEntity _player){
        player = _player;
    }
    private void Start()
    {
        InitNode();
    }
    public void UpdateData(UIItemEntity[] uIItems){
        for(int i = 0; i < uIItems.Length; ++i){

        }
    }

    public UIItemEntity GetNowItem(){
        return uiItems[itemSelect];
    }
    public int GetNowItemID(){
        return GetNowItem().GetItem().id;
    }

    void SetItem(UIItemEntity slot, int index){
        uiItems[index] = slot;
    }
    
    private void InitNode(){
        item = new Transform[transform.childCount];
        uiItems = new UIItemEntity[transform.childCount];
        for(int i = 0 ; i < transform.childCount; ++ i){
            item[i] = transform.GetChild(i);
            uiItems[i] = item[i].GetComponent<UIItemEntity>();
            uiItems[i].InitData(new(i, 100));
        }
    }

    void SetSelect(int index){
        itemSelect = index;
        highlight.SetParent(item[itemSelect].transform);
        highlight.position = highlight.parent.position;
    }

    public UIItemSlot GetSlotNow(){
        return null;
    }

    public int GetItemIDNow(){
        return 0;
    }
    float scroll;
    void Update()
    {
        if(player.inUI){
            return;
        }
        scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (scroll > 0)
            {
                itemSelect--;
            }
            else
            {
                itemSelect++;
            }
            int maxSelect = item.Length - 1;
            if (itemSelect > maxSelect)
            {
                itemSelect = 0;
            }
            if (itemSelect < 0)
            {
                itemSelect = maxSelect;
            }
            SetSelect(itemSelect);
        }
    }
}