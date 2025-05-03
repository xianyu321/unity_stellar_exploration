using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    AppConfig config;
    public GameObject zhObject;
    public GameObject enObject;
    public Scrollbar soundScroll;
    public TextMeshProUGUI soundTMP;
    public Scrollbar mouseScroll;
    public TextMeshProUGUI mouseTMP;
    void OnEnable()
    {
        config = ConfigManager.Instance.GetAppConfig();
        UpdateLangUI();
        soundScroll.onValueChanged.AddListener(OnScrollValueChange);
        soundScroll.value = config.soundVolume / 100;
        mouseScroll.onValueChanged.AddListener(OnSoundValueChange);
        mouseScroll.value = config.mouseSensitivity / 1000;
    }
    public void OnSwitchLanguageClicked()
    {
        if (config.languageType == "en")
        {
            config.languageType = "zh";
        }
        else
        {
            config.languageType = "en";
        }
        UpdateLangUI();
    }

    void UpdateLangUI()
    {
        if (config.languageType == "en")
        {
            zhObject.SetActive(false);
            enObject.SetActive(true);
        }
        else
        {
            zhObject.SetActive(true);
            enObject.SetActive(false);
        }
    }
    void OnScrollValueChange(float value){
        config.soundVolume = value * 100;
        soundTMP.text = Mathf.FloorToInt(config.soundVolume).ToString();
    }
    void OnSoundValueChange(float value){
        config.mouseSensitivity = value * 1000;
        mouseTMP.text = Mathf.FloorToInt(config.mouseSensitivity).ToString();
    }
    public void OnSaveOptionClicked()
    {
        ConfigManager.Instance.SaveAppConfig(config);
    }

    public void OnExitClicked()
    {
        this.gameObject.SetActive(false);
    }
}
