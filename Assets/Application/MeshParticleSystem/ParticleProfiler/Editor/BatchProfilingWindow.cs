﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using MeshParticleSystem.UITreeView;

namespace MeshParticleSystem.Profiler
{
    public class BatchProfilingWindow : EditorWindow
    {
        [MenuItem("Window/Particle Profiling Batcher Window", priority = 2050)]
        static private void Init()
        {
            BatchProfilingWindow window = GetWindow<BatchProfilingWindow>();
            window.titleContent = new GUIContent("Particle Profiling Batcher");
            window.position = new Rect(300, 200, 1300, 700);
            window.Show();
        }

        private const float kStatusbarPadding = 20;
        private const float kSplitterWidth = 5;
        private const string kDefaultConfigPath = "Assets/Application/MeshParticleSystem/ParticleProfiler";

        private Rect                        m_HorizontalSplitterRect;
        private bool                        m_ResizingHorizontalSplitter;
        private float                       m_HorizontalSplitterPercent;

        private Rect                        m_AssetTreeRect;
        private Rect                        m_InspectorRect;

        private BatchAssetCollection        m_Data;
        private ParticleAssetTreeView       m_ParticleAssetTreeView;
        private TreeViewState               m_ParticleAssetTreeViewState;
        private MultiColumnHeaderState      m_ParticleAssetMultiColumnHeaderState;
        private ParticleAssetTreeElement    m_SelectedTreeElement;

        private void OnEnable()
        {
            m_Data = GetDefault();
            if(m_Data == null)
            {
                CreateSetting();
                m_Data = GetDefault();
            }

            m_HorizontalSplitterPercent = 0.7f;
            InitParticleAssetTreeView();
        }

        void InitParticleAssetTreeView()
        {
            // create view state for bundle list tree view
            if (m_ParticleAssetTreeViewState == null)
            {
                m_ParticleAssetTreeViewState = new TreeViewState();
            }

            // create header state for bundle list tree view
            bool firstInit = m_ParticleAssetMultiColumnHeaderState == null;
            var headerState = ParticleAssetTreeView.CreateDefaultMultiColumnHeaderState();
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_ParticleAssetMultiColumnHeaderState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(m_ParticleAssetMultiColumnHeaderState, headerState);
            m_ParticleAssetMultiColumnHeaderState = headerState;
            ParticleAssetMultiColumnHeader multiColumnHeader = new ParticleAssetMultiColumnHeader(headerState);
            if (firstInit)
                multiColumnHeader.ResizeToFit();

            m_ParticleAssetTreeView = new ParticleAssetTreeView(m_ParticleAssetTreeViewState, multiColumnHeader, GetParticleAssetData(), this);
        }

        void UpdateParticleAssetTreeView()
        {
            m_ParticleAssetTreeView = new ParticleAssetTreeView(m_ParticleAssetTreeViewState, new ParticleAssetMultiColumnHeader(m_ParticleAssetMultiColumnHeaderState), GetParticleAssetData(), this);            
            if (m_ParticleAssetTreeViewState.selectedIDs != null)
                m_ParticleAssetTreeView.SetSelection(m_ParticleAssetTreeViewState.selectedIDs, TreeViewSelectionOptions.FireSelectionChanged);
        }

        private TreeModel<ParticleAssetTreeElement> GetParticleAssetData()
        {
            List<ParticleAssetTreeElement> treeElements = new List<ParticleAssetTreeElement>();
            treeElements.Add(new ParticleAssetTreeElement("Root", -1, 0));

            if(m_Data != null)
            {
                foreach(var file in m_Data.assetProfilerDataList)
                {
                    ParticleAssetTreeElement treeElement = new ParticleAssetTreeElement(file.filename, 0, file.assetPath.GetHashCode());
                    treeElement.isFile = true;
                    treeElement.assetProfilerData = file;
                    treeElement.directoryProfilerData = null;
                    treeElements.Add(treeElement);
                }

                foreach(var directory in m_Data.directoryProfilerDataList)
                {
                    ParticleAssetTreeElement treeElement = new ParticleAssetTreeElement(directory.directoryPath, 0, directory.directoryPath.GetHashCode());
                    treeElement.isFile = false;
                    treeElement.assetProfilerData = null;
                    treeElement.directoryProfilerData = directory;
                    // treeElement.index = -1;         // 目录节点
                    treeElements.Add(treeElement);

                    foreach(var file in directory.assetProfilerDataList)
                    {
                        ParticleAssetTreeElement e = new ParticleAssetTreeElement(file.filename, 1, file.assetPath.GetHashCode());
                        e.isFile = true;
                        e.assetProfilerData = file;
                        e.directoryProfilerData = null;
                        treeElements.Add(e);
                    }
                }
            }

            return new TreeModel<ParticleAssetTreeElement>(treeElements);
        }

        private void OnGUI()
        {
            HandleResize();

            DrawAssetTree();
            DrawInspector();
            DrawStatusbar();
        }

        private void HandleResize()
        {
            EditorGUIUtility.AddCursorRect(m_HorizontalSplitterRect, MouseCursor.ResizeHorizontal);
            if (Event.current.type == EventType.MouseDown && m_HorizontalSplitterRect.Contains(Event.current.mousePosition))
                m_ResizingHorizontalSplitter = true;

            if (Event.current.type == EventType.MouseUp)
                m_ResizingHorizontalSplitter = false;

            if (m_ResizingHorizontalSplitter)
            {
                m_HorizontalSplitterPercent = Mathf.Clamp( Event.current.mousePosition.x / position.width, 0.2f, 0.8f);
                Repaint();
            }

            m_HorizontalSplitterRect = new Rect(
                (int)(position.width * m_HorizontalSplitterPercent),
                0,
                kSplitterWidth,
                position.height - kStatusbarPadding);   
        }

        private void DrawAssetTree()
        {
            m_AssetTreeRect = new Rect(
                0,
                0,
                (int)(position.width * m_HorizontalSplitterPercent),
                position.height - kStatusbarPadding);

            GUILayout.BeginArea(m_AssetTreeRect, GUI.skin.GetStyle("HelpBox"));

            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    if(GUILayout.Button("Add"))
                    {
                        if(ValidGameObject())
                        {
                            m_Data.AddFile(AssetDatabase.GetAssetPath(Selection.activeGameObject));

                            EditorUtility.SetDirty(m_Data);
                            AssetDatabase.SaveAssets();

                            UpdateParticleAssetTreeView();
                        }
                        else if(ValidFolder())
                        {
                            m_Data.AddDirectory(AssetDatabase.GetAssetPath(Selection.activeObject));
                            
                            EditorUtility.SetDirty(m_Data);
                            AssetDatabase.SaveAssets();

                            UpdateParticleAssetTreeView();
                        }
                        else
                        {
                            Debug.LogWarning("plz select Valid Object");
                        }
                    }

                    if(GUILayout.Button("Remove"))
                    {
                        if(m_SelectedTreeElement != null)
                        {
                            if(m_SelectedTreeElement.assetProfilerData != null && m_SelectedTreeElement.depth == 0)
                            {
                                m_Data.RemoveFile(m_SelectedTreeElement.assetProfilerData.assetPath);

                                EditorUtility.SetDirty(m_Data);
                                AssetDatabase.SaveAssets();

                                UpdateParticleAssetTreeView();
                            }
                            else if(m_SelectedTreeElement.directoryProfilerData != null && m_SelectedTreeElement.depth == 0)
                            {
                                m_Data.RemoveDirectory(m_SelectedTreeElement.directoryProfilerData.directoryPath);

                                EditorUtility.SetDirty(m_Data);
                                AssetDatabase.SaveAssets();

                                UpdateParticleAssetTreeView();
                            }
                        }
                    }
                    if(GUILayout.Button("Refresh"))
                    {

                    }
                }
                GUILayout.EndHorizontal();

                if(GUILayout.Button("Test"))
                {

                }
            }
            GUILayout.EndVertical();

            int padding = 48;
            Rect rc = new Rect(0, padding, m_AssetTreeRect.width, m_AssetTreeRect.height - padding);
            m_ParticleAssetTreeView.OnGUI(rc);

            GUILayout.EndArea();
        }

        private void DrawInspector()
        {
            m_InspectorRect = new Rect(
                m_AssetTreeRect.width + kSplitterWidth,
                0,
                position.width - kSplitterWidth - m_AssetTreeRect.width,
                position.height - kStatusbarPadding);
        }

        private void DrawStatusbar()
        {
            Rect rc = new Rect(0, position.height - kStatusbarPadding, position.width, kStatusbarPadding);
            GUILayout.BeginArea(rc);
            GUILayout.Label("Hello world");
            GUILayout.EndArea();
        }

        private bool ValidParticle(GameObject prefab)
        {
            if(prefab.GetComponentsInChildren<ParticleSystem>(true).Length == 0 &&
               prefab.GetComponentsInChildren<FX_Component>(true).Length == 0)
            {
                return false;
            }
            return true;
        }

        private bool ValidGameObject()
        {
            if(Selection.activeGameObject == null || !ValidParticle(Selection.activeGameObject))
                return false;
            return true;
        }

        private bool ValidFolder()
        {
            if(Selection.activeObject == null)
                return false;
            return AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.activeObject));
        }

        internal void OnPostAssetListSelection(ParticleAssetTreeElement treeElement)
        {
            m_SelectedTreeElement = treeElement;
        }

        static private void CreateSetting()
        {
            BatchAssetCollection asset = GetDefault();
            if (asset != null)
                Selection.activeObject = asset;
            AssetDatabase.SaveAssets();
        }

        static public BatchAssetCollection GetDefault()
        {
            return GetOrCreateEditorConfigObject<BatchAssetCollection>(kDefaultConfigPath);
        }

        static private T GetOrCreateEditorConfigObject<T>(string directoryPath) where T : UnityEngine.ScriptableObject
        {
            //name of config data object
            string stringName = "com.myproject." + typeof(T).Name;

            //path to Config Object and asset name
            string stringPath = string.Format("{0}{1}{2}{3}", directoryPath.TrimEnd(new char[] { '/' }), "/", typeof(T).Name, ".asset");

            //used to hold config data
            T data = null;

            //if a config data object exists with the same name, return its config data
            if (EditorBuildSettings.TryGetConfigObject<T>(stringName, out data))
                return data;

            //If the asset file already exists, store existing config data
            if (System.IO.File.Exists(stringPath))
                data = AssetDatabase.LoadAssetAtPath<T>(stringPath);

            //if no previous config data exists
            if (data == null)
            {
                //initialise config data object
                data = ScriptableObject.CreateInstance<T>();
                //create new asset from data at specified path
                //asset MUST be saved with the AssetDatabase before adding to EditorBuildSettings
                AssetDatabase.CreateAsset(data, stringPath);
            }

            //add the new or loaded config object to EditorBuildSettings
            EditorBuildSettings.AddConfigObject(stringName, data, true);

            return data;
        }
    }
}