using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    private List<SaveItemUI> saveItems;


    public void InitData()
    {
        string directoryPath = PathLoader.GetSavesPath();
        try
        {
            // 获取该目录下的所有子目录
            string[] directories = Directory.GetDirectories(directoryPath, "*", SearchOption.TopDirectoryOnly);
            foreach (Transform item in saveContent.transform)
            {
                Destroy(item.gameObject);
            }
            foreach (string dir in directories)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(dir);
                GameObject item = Instantiate(saveItemPrefab);
                item.transform.SetParent(saveContent);
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
}
