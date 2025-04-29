using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreativeInventiory : MonoBehaviour
{
    public GameObject slotPrefab;
    public GameObject inventioryUp;
    public GameObject inventioryMiddle;
    public GameObject inventioryDown;

    World world;

    List<ItemSlot> slots = new List<ItemSlot>();

    private void Start(){
        world = GameObject.Find("World").GetComponent<World>();

        for(int i = 1; i < world.blockTypes.Length; ++i){
            GameObject newSlot = Instantiate(slotPrefab, inventioryMiddle.transform);

            ItemStack stack = new ItemStack(i, 999);

            ItemSlot slot = new ItemSlot(newSlot.GetComponent<UIItemSlot>(), stack);

            slot.isCreative = true;
        }
    }


}
