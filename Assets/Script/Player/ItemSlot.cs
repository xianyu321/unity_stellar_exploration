using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSlot{
    public ItemStack stack = null;
    private UIItemSlot uiItemSlot = null;
    public bool isCreative = false;
    public ItemSlot(UIItemSlot _uiItemSlot){
        stack = null;
        uiItemSlot = _uiItemSlot;
        uiItemSlot.Link(this);
    }

    public ItemSlot(UIItemSlot _uiItemSlot, ItemStack _stack){
        stack = _stack;
        uiItemSlot = _uiItemSlot;
        uiItemSlot.Link(this);
    }


    public void LinkUISlot(UIItemSlot _uIItemSlot){
        uiItemSlot = _uIItemSlot;
    }

    public void UnLinkUISlot(){
        uiItemSlot = null;
    }

    public void EmptySlot(){
        stack = null;
        if(uiItemSlot!=null){
            uiItemSlot.UpdateSlot();
        }
    }

    public int Take(int amt){
        if(amt > stack.amount){
            int res = stack.amount;
            EmptySlot();
            return res;
        }else if(amt < stack.amount){
            stack.amount -= amt;
            uiItemSlot.UpdateSlot();
            return amt;
        }else{
            EmptySlot();
            return amt;
        }
    }

    public ItemStack TakeAll(){
        ItemStack handOver = new ItemStack(stack.id, stack.amount);
        EmptySlot();
        return handOver;
    }

    public void InsertStack(ItemStack _stack){
        stack = _stack;
        uiItemSlot.UpdateSlot();
    }

    public bool hasItem{
        get{
            if(stack == null){
                return false;
            }
            return true;
        }
    }

    public int GetItemID(){
        if(stack == null){
            Debug.Log("获取空物品");
            return 0;
        }
        return stack.id;
    }
}