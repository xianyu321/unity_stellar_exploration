using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldSettingUI : MonoBehaviour
{
    SaveConfig config;
    string pathName;
    bool isCreate;
    public TMP_InputField nameInput;
    public TMP_InputField seedInput;
    public GameObject normalObject;
    public GameObject hardObject;
    public Scrollbar HPScroll;
    public TextMeshProUGUI HPTMP;
    public void InitData(string _pathName, bool _isCreate = true)
    {
        pathName = _pathName;
        isCreate = _isCreate;
        config = ConfigManager.Instance.GetSaveConfig(pathName);
        HPScroll.onValueChanged.AddListener(OnScrollValueChange);
        nameInput.text = config.name;
        seedInput.text = config.seed.ToString();
        HPScroll.value = (float)(config.PlayerHP - 10) / 90;
        UpdateUI();
        if (!isCreate)
        {
            seedInput.interactable = false;
        }else{
            seedInput.interactable = true;
        }
    }

    void UpdateUI()
    {
        UpdateDiffUI();
    }
    public void OnSwitchLanguageClicked()
    {
        if (config.difficulty == "hard")
        {
            config.difficulty = "normal";
        }
        else
        {
            config.difficulty = "hard";
        }
        UpdateDiffUI();
    }

    void UpdateDiffUI()
    {
        if (config.difficulty == "hard")
        {
            normalObject.SetActive(false);
            hardObject.SetActive(true);
        }
        else
        {
            normalObject.SetActive(true);
            hardObject.SetActive(false);
        }
    }
    void OnScrollValueChange(float value)
    {
        config.PlayerHP = 10 + Mathf.FloorToInt(value * 18) * 5;
        HPTMP.text = config.PlayerHP.ToString();
    }
    public void OnSaveClicked()
    {
        config.seed = int.Parse(seedInput.text);
        config.name = nameInput.text;
        ConfigManager.Instance.SaveSaveConfig(config, pathName);
        EventManager.Send("UpdateSavesUI");
        gameObject.SetActive(false);
    }

    public void OnExitClicked()
    {
        gameObject.SetActive(false);
        if (isCreate)
        {
            string folderPath = Path.Combine(PathLoader.GetSavesPath(), pathName);
            try
            {
                Directory.Delete(folderPath, true);
                Debug.Log("存档删除成功: " + folderPath);
            }
            catch (Exception e)
            {
                Debug.LogError("存档删除失败: " + e.Message);
            }
        }
    }
}
