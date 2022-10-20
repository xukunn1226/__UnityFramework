using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Application.Editor
{
    public class ExtractAnimWindow : UnityEditor.EditorWindow
    {
        static private string s_DstDirectoryKey = "Cache_Dst_Directory_3dcadcdfe3454dccgfd43";
        public string m_SrcDirectory;
        public string m_DstDirectory;
        public string[] m_PendingFbxs;
        public List<bool> m_PendingFbxsSelected;
        private Vector2 m_ScrollPosition;

        static public void Init(string srcDirectory)
        {
            ExtractAnimWindow window = GetWindow<ExtractAnimWindow>();
            window.Show();
            window.titleContent = new GUIContent("ExtractAnim Window", EditorGUIUtility.FindTexture("SettingsIcon"));
            window.position = new Rect(600, 120, 800, 600);
            window.m_SrcDirectory = string.IsNullOrEmpty(srcDirectory) ? "Assets/" : srcDirectory;

            string cachedDstDirectory = PlayerPrefs.GetString(s_DstDirectoryKey);            
            window.m_DstDirectory = (string.IsNullOrEmpty(cachedDstDirectory) || !AssetDatabase.IsValidFolder(cachedDstDirectory)) ? srcDirectory : cachedDstDirectory;

            window.m_PendingFbxs = AssetDatabase.FindAssets("t:model", new string[] { window.m_SrcDirectory });
            window.m_PendingFbxsSelected = new List<bool>(window.m_PendingFbxs.Length);
            for(int i = 0; i < window.m_PendingFbxs.Length; i++)
            {
                window.m_PendingFbxsSelected.Add(true);
            }
        }

        private void OnGUI()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
            EditorGUILayout.Space();            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("当前目录：", EditorStyles.label);
            if(GUILayout.Button(m_SrcDirectory, GUILayout.Width(600)))
            {
                string srcDirectory = EditorUtility.OpenFolderPanel("选择当前目录", m_SrcDirectory, m_SrcDirectory);
                if(!string.IsNullOrEmpty(srcDirectory))
                {
                    m_SrcDirectory = Framework.Core.Utility.GetProjectPath(srcDirectory);
                }
            }
            if(GUILayout.Button("UP", GUILayout.Width(30)))
            {
                m_SrcDirectory = m_SrcDirectory.Substring(0, m_SrcDirectory.LastIndexOf("/"));
                m_SrcDirectory = Framework.Core.Utility.GetProjectPath(m_SrcDirectory);
            }
            EditorGUILayout.EndHorizontal();

            // display all clips in current folder
            for(int i = 0; i < m_PendingFbxs.Length; ++i)
            {
                m_PendingFbxsSelected[i] = EditorGUILayout.ToggleLeft(System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(m_PendingFbxs[i])), m_PendingFbxsSelected[i]);
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("目标目录：", EditorStyles.label);
            if(GUILayout.Button(m_DstDirectory, GUILayout.Width(600)))
            {
                string dstDirectory = EditorUtility.OpenFolderPanel("选择目标目录", m_DstDirectory, m_DstDirectory);
                if(!string.IsNullOrEmpty(dstDirectory))
                {
                    m_DstDirectory = Framework.Core.Utility.GetProjectPath(dstDirectory);
                    PlayerPrefs.SetString(s_DstDirectoryKey, m_DstDirectory);
                }
            }
            if(GUILayout.Button("UP", GUILayout.Width(30)))
            {
                m_DstDirectory = m_DstDirectory.Substring(0, m_DstDirectory.LastIndexOf("/"));
                m_DstDirectory = Framework.Core.Utility.GetProjectPath(m_DstDirectory);
                PlayerPrefs.SetString(s_DstDirectoryKey, m_DstDirectory);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            if(GUILayout.Button("Extract Anim"))
            {
                //AssetPostprocessorHelper.DoExtractAnimClipBatch(m_SrcDirectory, m_DstDirectory);
                for(int i = 0; i < m_PendingFbxsSelected.Count; ++i)
                {
                    if (!m_PendingFbxsSelected[i])
                        continue;
                    AssetPostprocessorHelper.ExtractAnimClip(AssetDatabase.GUIDToAssetPath(m_PendingFbxs[i]), m_DstDirectory);
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }
}