using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[System.Serializable]
public class Settings
{
    [Header("玩家设置")]
    public int viewDistance = 4;//可视区块距离
    [Range(1f,10f)]
    public float mouseSensitivity = 3;//鼠标灵敏度
    [Header("系统设置")]
    public bool enableThreading = false;
    [Header("世界参数设置")]
    public int seed;
}
