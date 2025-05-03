using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SavesUI : MonoBehaviour
{
    public void OnExitClicked()
    {
        this.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        InitData();
        EventManager.On("UpdateSavesUI", InitData);
    }
    void OnDisable(){
        EventManager.Off("UpdateSavesUI", InitData);
    }

    public GameObject saveItemPrefab;
    public Transform saveContent;
    private List<SaveItemUI> saveItems = new List<SaveItemUI>();
    public WorldSettingUI worldSettingUI;
    public void InitData()
    {
        string directoryPath = PathLoader.GetSavesPath();
        try
        {
            // 获取该目录下的所有子目录
            string[] directories = Directory.GetDirectories(directoryPath, "*", SearchOption.TopDirectoryOnly);
            var sortedDirectories = directories.OrderBy(dir => new DirectoryInfo(dir).CreationTime);
            foreach (Transform item in saveContent.transform)
            {
                Destroy(item.gameObject);
            }
            foreach (string dir in sortedDirectories)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(dir);
                GameObject item = Instantiate(saveItemPrefab);
                item.transform.SetParent(saveContent, false);
                SaveItemUI itemUI = item.GetComponent<SaveItemUI>();
                itemUI.InitData(dirInfo, worldSettingUI);
                saveItems.Add(itemUI);
            }
        }
        catch (Exception e)
        {
            Debug.Log("读取文件夹时发生错误： " + e.Message);
        }
    }

    public void OnCreateWorldClicked(){
        string savesPath = PathLoader.GetSavesPath();
        string savePath = Path.Combine(savesPath, "Save");
        string saveName = GetUniqueFolderPath(savePath);
        worldSettingUI.gameObject.SetActive(true);
        worldSettingUI.InitData(saveName);
    }

    private string GetUniqueFolderPath(string baseFolderPath)
    {
        int counter = 0;
        string path = $"{baseFolderPath}_{counter}";
        while (Directory.Exists(path))
        {
            counter++;
            path = $"{baseFolderPath}_{counter}";
        }
        return $"Save_{counter}";
    }
}
