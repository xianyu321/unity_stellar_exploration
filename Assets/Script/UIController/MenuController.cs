using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject savesUI;
    public GameObject sultiplayUI;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onSingleplayerBtnClick(){
        savesUI.SetActive(true);
        Debug.Log("onSingleplayerBtn");
        SceneManager.LoadScene("DebugScene");
        return;
    }

    public void onMultiplayerBtnClick(){
        sultiplayUI.SetActive(true);
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
