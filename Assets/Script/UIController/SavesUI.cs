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
    }

    public GameObject saveItemPrefab;
    public Transform saveContent;
    private List<SaveItemUI> saveItems = new List<SaveItemUI>();

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
                itemUI.InitData(dirInfo);
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
        string savePath = Path.Combine(savesPath, "New World");
        savePath = GetUniqueFolderPath(savePath);
        try
        {
            Directory.CreateDirectory(savePath);
            Debug.Log("存档创建成功: " + savePath);
            InitData();
        }
        catch (Exception e)
        {
            Debug.LogError("存档创建失败: " + e.Message);
        }
    }

    private string GetUniqueFolderPath(string baseFolderPath)
    {
        string path = baseFolderPath;
        int counter = 1;
        while (Directory.Exists(path))
        {
            path = $"{baseFolderPath}_{counter}";
            counter++;
        }
        return path;
    }
}
