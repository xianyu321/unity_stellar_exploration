using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;

public class UIItemEntity : MonoBehaviour
{
    public Image icon;
    public TMP_Text numText;
    ItemData item;

    public UIItemEntity(){
    }
    public UIItemEntity(ItemData _item){
        item = _item;
        UpdateUI();
    }

    public ItemData GetItem(){
        return item;
    }

    public void InitData(ItemData inData){
        item = inData;
        UpdateUI();
    }

    public ItemData TakeIn(ItemData inData){
        if(item is null){
            item = inData;
            UpdateUI();
            return null;
        }
        if(inData.id == item.id){
            if(inData.num + item.num > item.maxNum){
                inData.num = inData.num + item.num - item.maxNum;
                item.num = item.maxNum;
                UpdateUI();
                return inData;
            }else{
                item.num += inData.num;
                UpdateUI();
                return null;
            }
        }else{
            ItemData tempItem = item;
            item = inData;
            UpdateUI();
            return tempItem;
        }
    }

    public ItemData TakeOut(){
        ItemData tempItem = item;
        item = null;
        UpdateUI();
        return tempItem;
    }

    void UpdateUI(){
        if(item is null){
            icon.sprite = null;
            Color tempColor = icon.color;
            tempColor.a = 0;
            icon.color = tempColor;
            numText.text = "";
        }else{
            icon.sprite = TextureManager.Instance.GetIcon(item.id);
            Color tempColor = icon.color;
            tempColor.a = 1;
            icon.color = tempColor;
            numText.text = item.num.ToString();
        }   
    }
}

public class ItemData{
    public int id;
    public int num;
    public int maxNum = 999;
    public ItemData(int _id, int _num){
        id = _id;
        num = _num;
    }
}

