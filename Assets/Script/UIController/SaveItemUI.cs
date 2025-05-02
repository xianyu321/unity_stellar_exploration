using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;

public class SaveItemUI : MonoBehaviour
{
    string _saveName;
    string saveName{
        get{
            return _saveName;
        }
        set{
            _saveName = value;
            if (nameTMP is not null)
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
            if(createTimeTMP is not null){
                createTimeTMP.text = _createTime.ToString();
            }
        }
    }
    public TextMeshProUGUI nameTMP;
    public TextMeshProUGUI createTimeTMP;
    // TextMeshProUGUI
    public void InitData(DirectoryInfo dirInfo){
        saveName = dirInfo.Name;
        createTime = dirInfo.CreationTime;
    }

    public void OnDeleteClicked(){

    }

    public void OnEditClicked(){

    }

    public void OnJoinClicked(){

    }
}
