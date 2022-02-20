using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Application.Runtime
{
    public class UIDeveloper : MonoBehaviour
    {
        IList<ICanvasElement> m_LayoutRebuildQueue;
        IList<ICanvasElement> m_GraphicRebuildQueue;

#if UNITY_EDITOR
        private void Awake()
        {
            System.Type type = typeof(CanvasUpdateRegistry);
            FieldInfo field = type.GetField("m_LayoutRebuildQueue", BindingFlags.NonPublic | BindingFlags.Instance);
            m_LayoutRebuildQueue = (IList<ICanvasElement>)field.GetValue(CanvasUpdateRegistry.instance);
            field = type.GetField("m_GraphicRebuildQueue", BindingFlags.NonPublic | BindingFlags.Instance);
            m_GraphicRebuildQueue = (IList<ICanvasElement>)field.GetValue(CanvasUpdateRegistry.instance);
        }

        private void Update()
        {
            for (int j = 0; j < m_LayoutRebuildQueue.Count; j++)
            {
                var rebuild = m_LayoutRebuildQueue[j];
                if (ObjectValidForUpdate(rebuild))
                {
                    //Debug.LogFormat("{0}引起网格重建", rebuild.transform.name,);
                }
            }

            for (int j = 0; j < m_GraphicRebuildQueue.Count; j++)
            {
                var element = m_GraphicRebuildQueue[j];
                if (ObjectValidForUpdate(element))
                {
                    Debug.LogFormat("{0}引起{1}网格重建", element.transform.name, element.transform.GetComponent<Graphic>().canvas.name);
                }
            }
        }
        private bool ObjectValidForUpdate(ICanvasElement element)
        {
            var valid = element != null;

            var isUnityObject = element is Object;
            //Here we make use of the overloaded UnityEngine.Object == null, that checks if the native object is alive.
            if (isUnityObject)
                valid = (element as Object) != null;

            return valid;
        }
#endif

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

        public void CloseAllUI()
        {
            IStaticMethod start = CodeLoader.GetStaticMethod("Application.Logic.UIManager", "Get", 0);
            System.Object inst = start.Exec();

            IMemberMethod method = CodeLoader.GetMemberMethod("Application.Logic.UIManager", "CloseAll", 0);
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

            // CloseAll
            EditorGUILayout.BeginHorizontal();
            {
                if(GUILayout.Button("CloseAll"))
                {
                    m_Target.CloseAllUI();
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