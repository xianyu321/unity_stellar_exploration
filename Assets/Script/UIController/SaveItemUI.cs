using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveItemUI : MonoBehaviour
{
    string _saveName;

    public string saveName{
        get{
            return _saveName;
        }
        set{
            _saveName = value;
            if (nameTMP != null)
            {
                nameTMP.text = _saveName;
            }
        }
    }
    DateTime _createTime;
    public DateTime createTime{
        get{
            return _createTime;
        }
        set{
            _createTime = value;
            if(createTimeTMP != null)
            {
                createTimeTMP.text = _createTime.ToString();
            }
        }
    }
    public TextMeshProUGUI nameTMP;
    public TextMeshProUGUI createTimeTMP;
    string dirName;
    WorldSettingUI worldSettingUI;
    public void InitData(DirectoryInfo dirInfo, WorldSettingUI _worldSettingUI){
        worldSettingUI = _worldSettingUI;
        dirName = dirInfo.Name;
        SaveConfig config = ConfigManager.Instance.GetSaveConfig(dirInfo.Name);
        saveName = config.name;
        createTime = dirInfo.CreationTime;
    }

    public void OnDeleteClicked(){
        string folderPath = Path.Combine(PathLoader.GetSavesPath(), dirName);
        try
        {
            Directory.Delete(folderPath, true);
            Debug.Log("存档删除成功: " + folderPath);
            Destroy(this.gameObject);
        }
        catch (Exception e)
        {
            Debug.LogError("存档删除失败: " + e.Message);
        }
    }

    public void OnEditClicked(){
        worldSettingUI.gameObject.SetActive(true);
        worldSettingUI.InitData(dirName, false);
    }

    public void OnJoinClicked(){
        SceneManager.LoadScene("DebugScene");
    }
}
