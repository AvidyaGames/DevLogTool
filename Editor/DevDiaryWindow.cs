using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEditor;
using Directory = System.IO.Directory;
using File = UnityEngine.Windows.File;


public class DevDiaryWindow : EditorWindow
{
    //assigned in editor!
    public GUISkin _defaultSkin;

    private Vector2 _scrollPosition = Vector2.zero;
    private Vector2 _promptAPosition = Vector2.zero;
    private Vector2 _promptBPosition = Vector2.zero;
    private static DataEntry _activeEntry;
    private RitualProgress _progress;
    private RitualQuestions _questions;
    private GUIStyle previewStyle; 
    private GUIStyle previewDoStyle;
    private GUIStyle resetButton; 
    
    
    public SaveFile memory; 
    public string parentFolder = "Assets"; 
    public string saveFolder = "DevDiary";
    public string saveFile = "entries"; 
    //private DataEntry _activeEntry; 

    
    
    [MenuItem("Avidya/Open Devlogger")]
    public static void ShowWindow()
    {
        
        EditorWindow wnd = GetWindow<DevDiaryWindow>();
        DevDiaryWindow logger = wnd as DevDiaryWindow;
        logger._progress = default; 
        _activeEntry = startTemplate;
        logger.previewStyle = logger._defaultSkin.GetStyle("previewText");
        logger.previewDoStyle = logger._defaultSkin.GetStyle("previewDoText");
        logger.resetButton = logger._defaultSkin.GetStyle("resetButton"); 
        //Replace with loading and were good; 
        logger.LoadFile();
        wnd.titleContent = new GUIContent("Dev Diary"); 
    }



    private void OnEnable()
    {
        _activeEntry = startTemplate; 
    }


    public void OnGUI()
    {
        if (ReferenceEquals(_defaultSkin, null) == false)
        { 
            GUI.skin = _defaultSkin; 
        }
        //gui not loaded or smthn
        if (resetButton == null)
        {
            ShowWindow();
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Past Entries"))
        {
            _progress.isViewingPast = true; 
        }
        if (_progress.initialEntryCompleted == false && GUILayout.Button("Current Draft"))
        {
            _progress.isViewingPast = false; 
        }
        if (GUILayout.Button("Reset Drafting",resetButton))
        {
            ShowWindow();
        }
        GUILayout.EndHorizontal();
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, true, true, GUILayout.Width(position.width), GUILayout.Height(position.height + 64)); 
        if (_progress.isViewingPast == false && _progress.initialEntryCompleted == false)
        {
            if (_progress.inited == false)
            {
                _progress = default;
                _progress.inited = true; 
            }

            GUILayout.BeginHorizontal();
            if (_activeEntry.TimeStamp == default)
            {
                GUILayout.Label("Drafting");
            }
            else
            {
                GUILayout.Label("Drafted");
            }
            
            if (GUILayout.Button("Complete Ritual"))
            {
                _progress.initialEntryCompleted = true; 
                AddEntry(_activeEntry);
                SaveEntry();
                AssetDatabase.Refresh();
            }
            
            GUILayout.EndHorizontal();
            _activeEntry.feelPrompt = GUILayout.TextField(_activeEntry.feelPrompt); 
            GUILayout.Space(4);
            _activeEntry.feelResponse = GUILayout.TextArea(_activeEntry.feelResponse);
            
            GUILayout.Space(24);
            
            _activeEntry.whatPrompt = GUILayout.TextField(_activeEntry.whatPrompt); 
            GUILayout.Space(4);
            _activeEntry.whatResponse = GUILayout.TextArea(_activeEntry.whatResponse);
            GUILayout.Space(4);

            if (_activeEntry.rating == default)
            {
                _activeEntry.showRating = GUILayout.Toggle(_activeEntry.showRating,"collect quantitative data");
                if (_activeEntry.showRating)
                {             
                    _activeEntry.ratingPrompt = GUILayout.TextField(_activeEntry.ratingPrompt); 
                    GUILayout.BeginHorizontal();
                    for (int i = 0; i < 10; i++)
                    {
                        if (GUILayout.Button(i.ToString()))
                        {
                            _activeEntry.rating = i; 
                        }
                    }
                    GUILayout.EndHorizontal();
                }    
            }
            else
            {
                GUILayout.Label($"{_activeEntry.ratingPrompt}: {_activeEntry.rating}");
            }


            GUILayout.Space(12);
            
        }
        else
        {
            GUILayout.Label("The Past");
            if (memory.entries != null)
            {
                int c = memory.entries.Length; 
                for (int i = 0; i < c ; i++)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(new DateTime(memory.entries[i].TimeStamp).ToString()))
                    {
                        _activeEntry = memory.entries[i];
                        _progress = default; 
                    }
                
                    GUILayout.Space(6);
                    string preview = "-";
                    string previewDo =  "-";
                    if (!String.IsNullOrEmpty(memory.entries[i].feelResponse))
                    {
                        preview = memory.entries[i].feelResponse.Substring(0, Mathf.Min(memory.entries[i].feelResponse.Length,120)) + "...";
                    }
                
                    if (!String.IsNullOrEmpty(memory.entries[i].feelResponse))
                    { 
                        previewDo = memory.entries[i].whatResponse.Substring(0, Mathf.Min(memory.entries[i].whatResponse.Length,120)) + "..."; 
                    }
                    GUILayout.BeginVertical();
                    GUILayout.Label(preview,previewStyle);
                    GUILayout.Label(previewDo,previewDoStyle);
                    GUILayout.EndVertical();
                
                    GUILayout.EndHorizontal();
                }   
            }
        }
        
        GUILayout.Space(12);
        GUILayout.EndScrollView();
    }


    private static DataEntry startTemplate
    {
        get
        {
            DataEntry data = default;
            data.feelPrompt = "How are you feeling?";
            data.whatPrompt = "What are you doing?";
            data.showRating = true;
            data.ratingPrompt = "Best Number"; 
            return data; 
        }
    } 
    
    
    [Serializable]
    public struct DataEntry
    {
        public long TimeStamp; 
        public string feelPrompt;
        public string feelResponse;
        
        public string whatPrompt;
        public string whatResponse;
        
        public bool showRating; 
        public string ratingPrompt; 
        public int rating;
        
        
    }
    [Serializable]
    private struct RitualProgress
    {
        public bool isViewingPast; 
        public bool inited; 
        public bool initialEntryCompleted;
        public string howFeel;
        public string whatDo; 
    }
    private struct RitualQuestions
    {
        public string howFeel;
        public string WhatDo;
    }
    
    private  bool CheckForFolder()
    {
        if (AssetDatabase.IsValidFolder(parentFolder+"/" + saveFolder+"/"))
        {
            return true; 
        }
        else
        {
            AssetDatabase.CreateFolder(parentFolder+"/",saveFolder+"/"); 
            return true; 
        }
        return false; 
    }

    public void AddEntry(DevDiaryWindow.DataEntry entry)
    {
        if (memory.entries == null)
        {
            memory.entries = new DataEntry[0]; 
        }
        int startingCount = memory.entries.Length;
        DataEntry[] cache = memory.entries; 
        memory.entries = new DataEntry[startingCount + 1];
        for (int i = 0; i < startingCount; i++)
        {
            memory.entries[i] = cache[i]; 
        }
        entry.TimeStamp = DateTime.Now.Ticks;  
        memory.entries[startingCount] = entry; 
    }
    public void SaveEntry()
    {
     using (FileStream stream = new FileStream(Application.dataPath + "/" + saveFolder+"/" + saveFile + ".txt", FileMode.Create))
     {
            var formatter = new BinaryFormatter();
            var data = memory; 
            formatter.Serialize(stream, data);
        stream.Dispose();    
     }
        
      //  writer.Dispose();
    }

    public void LoadFile()
    {
        if (File.Exists(Application.dataPath + "/" + saveFolder+"/" + saveFile + ".txt"))
        {
            using (FileStream stream = new FileStream(Application.dataPath + "/" + saveFolder+"/" + saveFile + ".txt", FileMode.Open))
            {
                var formatter = new BinaryFormatter();
                SaveFile file = formatter.Deserialize(stream) as SaveFile;
                memory = file;

                if (memory == null)
                {
                    memory = new SaveFile();
                }
            }
        }
        else
        {
            Directory.CreateDirectory(Application.dataPath + "/" + saveFolder+"/"); 
            memory = new SaveFile(); 
        }
        
        
    }

    
    int TotalLines(string filePath)
    {
        using (StreamReader r = new StreamReader(filePath))
        {
            int i = 0;
            while (r.ReadLine() != null) { i++; }
            r.Close();
            return i;
        }
    }
    [Serializable]
    public class SaveFile
    {
        public string count;
        public DevDiaryWindow.DataEntry[] entries; 
    }
    
}
