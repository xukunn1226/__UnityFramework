using System.Collections;
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
            window.position = new Rect(300, 200, 800, 600);
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

        private void OnEnable()
        {
            m_Data = GetDefault();
            if(m_Data == null)
            {
                CreateSetting();
                m_Data = GetDefault();
            }

            m_HorizontalSplitterPercent = 0.4f;
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

        private TreeModel<ParticleAssetTreeElement> GetParticleAssetData()
        {
            List<ParticleAssetTreeElement> treeElements = new List<ParticleAssetTreeElement>();
            treeElements.Add(new ParticleAssetTreeElement("Root", -1, 0));

            if(m_Data != null)
            {

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
                    // GUILayout.Space(2);
                    if(GUILayout.Button("Add"))
                    {

                    }
                    if(GUILayout.Button("Remove"))
                    {

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