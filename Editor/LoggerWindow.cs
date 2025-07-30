using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LoggerWindow : EditorWindow
{
    public GUISkin _defaultSkin;
    private string _defaultSkinPath = "Packages/com.avidyagames.devlogtool/Editor/DevLoggerSkin.guiskin"; 

    [MenuItem("Avidya/Open Devlogger")]
    public static void ShowWindow()
    {
        //defaultSkin = 
        EditorWindow wnd = GetWindow<LoggerWindow>();
        wnd.titleContent = new GUIContent("Devlogger Command Center"); 
    }
    
    


    public void OnGUI()
    {
        if (ReferenceEquals(_defaultSkin, null) == false)
        { 
            GUI.skin = _defaultSkin; 
        }
       
        GUILayout.Box("This box is a tester"); 
    }
}
