using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onSingleplayerBtnClick(){
        Debug.Log("onSingleplayerBtn");
        return;
    }

    public void onMultiplayerBtnClick(){
        Debug.Log("onMultiplayerBtn");
        return;

    }

    public void onOptionsBtnClick(){
        Debug.Log("onOptionsBtn");
        return;
    }

    public void onExitBtnClick(){
        Debug.Log("onExitBtn");
        return;
    }
}
