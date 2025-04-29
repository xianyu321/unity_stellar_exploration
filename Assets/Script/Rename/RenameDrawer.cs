using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(RenameAttribute))] //用到RenameAttribute的地方都会被重绘
public class RenameDrawer : PropertyDrawer //相对于Editor类可以修改MonoBehaviour的外观，我们可以简单的理解PropertyDrawer为修改struct/class的外观的Editor类
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //替换属性名称
        RenameAttribute rename = (RenameAttribute)attribute;
        label.text = rename.name;
 
        //重绘GUI
        EditorGUI.PropertyField(position, property, label);
    }
 
}