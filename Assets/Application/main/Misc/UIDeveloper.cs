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

		public string[] GetIds()
		{
			IStaticMethod method = CodeLoader.GetStaticMethod("Application.Logic.UIDefines", "GetIds", 0);
			List<string> ids = (List<string>)method.Exec();
			return ids.ToArray();
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

            EditorGUI.EndDisabledGroup();

            // 显示栈信息
            // 显示接收Update界面信息
            // 显示LRU池信息
            // 显示队列信息
		}
	}
    #endif
}