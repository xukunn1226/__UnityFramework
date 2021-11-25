using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Application.Runtime
{
    /// <summary>
    /// 在任何场景可以用 ctrl+g 启动游戏
    /// </summary>
    public class CtrlG : MonoBehaviour
    {
#if UNITY_EDITOR
        static bool _Inited = false;
        const string FirstSceneName = "launcher";

        void Awake()
        {
            if (_Inited) return;
            _Inited = true;

            string curSceneName = SceneManager.GetActiveScene().name;
            if (curSceneName != FirstSceneName)
            {
                SceneManager.LoadScene(FirstSceneName, LoadSceneMode.Single);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.EnteredEditMode)
            {
                GameObject go = GameObject.Find("/CtrlG");
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                    EditorSceneManager.OpenScene(EditorPrefs.GetString("CtrlG_PrevScenePath"), OpenSceneMode.Single);
                }
                UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == FirstSceneName)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }        
#endif
    }   
}
