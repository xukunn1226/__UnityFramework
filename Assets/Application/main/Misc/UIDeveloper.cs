using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Application.Runtime
{
    public class UIDeveloper : MonoBehaviour
    {
        #if UNITY_EDITOR
        public void OpenUI(string id)
        {
            IStaticMethod start = CodeLoader.GetStaticMethod("Application.Logic.UIManager", "Get", 0);
            System.Object inst = start.Exec();

            IMemberMethod method = CodeLoader.GetMemberMethod("Application.Logic.UIManager", "Open", 2);
            method.Exec(inst, id, null);
        }

		public void CloseUI(string id)
        {
            IStaticMethod start = CodeLoader.GetStaticMethod("Application.Logic.UIManager", "Get", 0);
            System.Object inst = start.Exec();

            IMemberMethod method = CodeLoader.GetMemberMethod("Application.Logic.UIManager", "Close", 1);
            method.Exec(inst, id);
        }

        public void CloseTopUI()
        {
            IStaticMethod start = CodeLoader.GetStaticMethod("Application.Logic.UIManager", "Get", 0);
            System.Object inst = start.Exec();

            IMemberMethod method = CodeLoader.GetMemberMethod("Application.Logic.UIManager", "CloseTop", 0);
            method.Exec(inst);
        }

		public string[] GetIds()
		{
			IStaticMethod method = CodeLoader.GetStaticMethod("Application.Logic.UIDefines", "GetIds", 0);
			List<string> ids = (List<string>)method.Exec();
			return ids.ToArray();
		}

        public string[] GetStackPanelInfo()
        {
            IStaticMethod method = CodeLoader.GetStaticMethod("Application.Logic.UIManager", "GetStackPanelInfo", 0);
            List<string> infos = (List<string>)method.Exec();
            return infos.ToArray();
        }

        public string[] GetUpdatePanelInfo()
        {
            IStaticMethod method = CodeLoader.GetStaticMethod("Application.Logic.UIManager", "GetUpdatePanelInfo", 0);
            List<string> infos = (List<string>)method.Exec();
            return infos.ToArray();
        }

        public string[] GetPersistentPoolInfo()
        {
            IStaticMethod method = CodeLoader.GetStaticMethod("Application.Logic.UIManager", "GetPersistentPoolInfo", 0);
            List<string> infos = (List<string>)method.Exec();
            return infos.ToArray();
        }

        public string[] GetLRUPoolInfo()
        {
            IStaticMethod method = CodeLoader.GetStaticMethod("Application.Logic.UIManager", "GetLRUPoolInfo", 0);
            List<string> infos = (List<string>)method.Exec();
            return infos.ToArray();
        }

        public int GetLRUMaxCount()
        {
            IStaticMethod method = CodeLoader.GetStaticMethod("Application.Logic.UIManager", "GetLRUMaxCount", 0);
            int count = (int)method.Exec();
            return count;
        }

        #endif
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(UIDeveloper))]
	public class UIDeveloperEditor : UnityEditor.Editor
	{
        private UIDeveloper m_Target;
        private int m_Index;
        private int m_Index2;
        private string[] m_Ids;

        void OnEnable()
        {
            m_Target = (UIDeveloper)target;
        }

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

            EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);

            // Open
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Open"))
                {
                    m_Target.OpenUI(m_Ids[m_Index]);
                }

                if (EditorApplication.isPlaying)
                {
                    m_Ids = m_Target.GetIds();
                    m_Index = EditorGUILayout.Popup(m_Index, m_Ids);
                }
            }
            EditorGUILayout.EndHorizontal();

            // Close
            EditorGUILayout.BeginHorizontal();
            {
                if(GUILayout.Button("Close"))
                {
                    m_Target.CloseUI(m_Ids[m_Index2]);
                }
                
                if (EditorApplication.isPlaying)
                {
                    m_Ids = m_Target.GetIds();
                    m_Index2 = EditorGUILayout.Popup(m_Index2, m_Ids);
                }
            }
            EditorGUILayout.EndHorizontal();

            // CloseTop
            EditorGUILayout.BeginHorizontal();
            {
                if(GUILayout.Button("CloseTop"))
                {
                    m_Target.CloseTopUI();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            // 显示栈信息
            EditorGUILayout.LabelField(string.Format("StackInfo"), EditorStyles.largeLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                if (EditorApplication.isPlaying)
                {
                    string[] infos = m_Target.GetStackPanelInfo();
                    foreach (var info in infos)
                    {
                        EditorGUILayout.LabelField(info);
                    }
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();

            // 显示接收Update界面信息
            EditorGUILayout.LabelField(string.Format("UpdateInfo"), EditorStyles.largeLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                if (EditorApplication.isPlaying)
                {
                    string[] infos = m_Target.GetUpdatePanelInfo();
                    foreach (var info in infos)
                    {
                        EditorGUILayout.LabelField(info);
                    }
                }
            }
            EditorGUILayout.EndVertical();

            // 显示PersistentPool信息
            EditorGUILayout.LabelField(string.Format("Persistent Pool"), EditorStyles.largeLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                if (EditorApplication.isPlaying)
                {
                    string[] infos = m_Target.GetPersistentPoolInfo();
                    foreach (var info in infos)
                    {
                        EditorGUILayout.LabelField(info);
                    }
                }
            }
            EditorGUILayout.EndVertical();

            // 显示LRUPool信息
            string label = string.Format($"LRU Pool (?)");
            if(EditorApplication.isPlaying)
            {
                label = string.Format($"LRU Pool ({m_Target.GetLRUMaxCount()})");
            }
            EditorGUILayout.LabelField(label, EditorStyles.largeLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                if (EditorApplication.isPlaying)
                {
                    string[] infos = m_Target.GetLRUPoolInfo();
                    foreach (var info in infos)
                    {
                        EditorGUILayout.LabelField(info);
                    }
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUI.EndDisabledGroup();
		}
	}
    #endif
}