using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItemSlot : MonoBehaviour
{
    public bool isLinked = false;
    public ItemSlot itemSlot;
    public Image slotIcon;
    public TMP_Text slotAmount;
    World world;

    private void Awake(){
        // world = GameObject.Find("World").GetComponent<World>();
    }

    public bool hasItem{
        get{
            if(itemSlot == null){
                return false;
            }
            return itemSlot.hasItem;
        }
    }

    public void Link(ItemSlot _itemSlot){
        itemSlot = _itemSlot;
        isLinked = true;
        itemSlot.LinkUISlot(this);
        UpdateSlot();
    }

    public void UnLink(){
        itemSlot.UnLinkUISlot();
        itemSlot = null;
        UpdateSlot();
    }

    public void UpdateSlot(){
        if(itemSlot != null && itemSlot.hasItem){
            slotIcon.sprite = TextureManager.Instance.blockIcon[itemSlot.stack.id];
            // slotIcon.sprite = BlockManager.Instance.
            // slotIcon.sprite = world.blockTypes[itemSlot.stack.id].icon;
            slotAmount.text = itemSlot.stack.amount.ToString();
            slotIcon.enabled = true;
            slotAmount.enabled = true;
        }else{
            Clear();
        }
    }

    public void Clear(){
        slotIcon.sprite = null;
        slotAmount.text = "";
        slotIcon.enabled = false;
        slotAmount.enabled = false;
    }

    private void OnDestroy(){
        if(isLinked){
            itemSlot.UnLinkUISlot();
        }
    }

    public int GetItemID(){
        if(itemSlot == null){
            Debug.Log("获取空物品");
            return 0;
        }
        return itemSlot.GetItemID();
    }

    public int Take(int amt){
        if(itemSlot == null){
            Debug.Log("Take空物品");
            return 0;
        }
        return itemSlot.Take(amt);
    }

    public ItemStack TakeAll(){
        if(itemSlot == null){
            Debug.Log("TakeAll空物品");
            return null;
        }
        return itemSlot.TakeAll();
    }
}


