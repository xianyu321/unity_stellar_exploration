using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragAndDropHandler : MonoBehaviour
{
    [SerializeField] private UIItemEntity cursorItem = null;
    [SerializeField] private GraphicRaycaster m_Raycaster =null;
    private PointerEventData m_PointerEventData;
    [SerializeField] private EventSystem m_EventSystem = null;
    [SerializeField] public PlayerEntity player;

    private void Start(){
        // world = GameObject.Find("World").GetComponent<World>();

        // cursorItemSlot = new ItemData(cursorSlot);
    }

    private void Update(){
        if(!player.inUI){
            return;
        }
        cursorItem.transform.position = Input.mousePosition;
        if(Input.GetMouseButtonDown(0)){
            HandleSlotClick(CheckForSlot());
        }
    }

    private void HandleSlotClick(UIItemEntity clickedSlot){
        if(clickedSlot == null){
            return;
        }

        if(!cursorItem.hasItem && !clickedSlot.hasItem){
            return;
        }

        if(clickedSlot.hasItem && clickedSlot.GetItem().isCreative){
            cursorItem.ClearItem();
            cursorItem.InitData(clickedSlot.GetItem());
            return;
        }
        
        if(!cursorItem.hasItem && clickedSlot.hasItem){
            cursorItem.InitData(clickedSlot.TakeOut());
            return;
        }

        if(cursorItem.hasItem && !clickedSlot.hasItem){ 
            clickedSlot.InitData(cursorItem.TakeOut());
            return;
        }

        if(cursorItem.hasItem && clickedSlot.hasItem){
            if(cursorItem.GetItemID() != clickedSlot.GetItemID()){
                ItemData tempItem = cursorItem.TakeOut();
                cursorItem.InitData(clickedSlot.TakeOut());
                clickedSlot.InitData(tempItem);
            }else{
                ItemData tempItem = clickedSlot.TakeIn(cursorItem.GetItem());
                cursorItem.InitData(tempItem);
            }
            return;
        }
    }

    private UIItemEntity CheckForSlot(){
        m_PointerEventData = new PointerEventData(m_EventSystem);
        m_PointerEventData.position = Input.mousePosition;
        
        List<RaycastResult> results = new List<RaycastResult>();

        m_Raycaster.Raycast(m_PointerEventData, results);

        foreach(RaycastResult result in results){
            if(result.gameObject.tag == "UIItemSlot"){
                return result.gameObject.GetComponent<UIItemEntity>();
            }
        }
        return null;
    }
}
