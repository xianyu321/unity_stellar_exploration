using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class MenuController : MonoBehaviour
{
    public GameObject savesUI;
    public GameObject sultiplayUI;
    public GameObject optionUI;

    IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;
        ConfigManager.Instance.SaveAppConfig(ConfigManager.Instance.GetAppConfig());
    }
    
    public void onSingleplayerBtnClick()
    {
        savesUI.SetActive(true);
        return;
    }

    public void onMultiplayerBtnClick()
    {
        sultiplayUI.SetActive(true);
        return;
    }

    public void onOptionsBtnClick()
    {
        optionUI.SetActive(true);
        return;
    }

    public void onExitBtnClick()
    {
        Application.Quit();
        return;
    }
}
