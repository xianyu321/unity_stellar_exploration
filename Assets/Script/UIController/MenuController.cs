using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject savesUI;
    public GameObject sultiplayUI;
    public GameObject optionUI;
    public void onSingleplayerBtnClick(){
        savesUI.SetActive(true);
        return;
    }

    public void onMultiplayerBtnClick(){
        sultiplayUI.SetActive(true);
        return;
    }

    public void onOptionsBtnClick(){
        optionUI.SetActive(true);
        return;
    }

    public void onExitBtnClick(){
        Application.Quit();
        return;
    }
}
