using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Application.Runtime
{
    /// <summary>
    /// 
    /// <summary>
    public class MainLoop : MonoBehaviour
    {
        private const string kEmptySceneName    = "empty";
        private const string kEmptyScenePath    = "assets/res/scenes/empty.unity";
        private const string kBundlePath        = "assets/res/scenes.ab";

        void Awake()
        {
            if (Launcher.Instance == null)
                throw new System.Exception("MainLoop: Launcher.Instance == null");

            Launcher.Instance.Disable();        // 结束Launcher流程
        }

        public void Restart()
        {
            LevelManager.LevelContext ctx = new LevelManager.LevelContext();
            ctx.sceneName = kEmptySceneName;
            ctx.scenePath = kEmptyScenePath;
            ctx.additive = false;
            ctx.bundlePath = kBundlePath;
            LevelManager.Instance.LoadAsync(ctx);
        }
        
#if UNITY_EDITOR        
        public void RestartAndStay()
        {
            LevelManager.LevelContext ctx = new LevelManager.LevelContext();
            ctx.sceneName = kEmptySceneName;
            ctx.scenePath = kEmptyScenePath;
            ctx.additive = false;
            ctx.bundlePath = kBundlePath;
            LevelManager.Instance.LoadAsync(ctx);

            Launcher.s_shouldStay = true;
        }
#endif        

        public void Reconnect()
        {

        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MainLoop))]
    public class MainLoop_Inspector : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Restart"))
            {
                ((MainLoop)target).Restart();
            }
            
            if (GUILayout.Button("Restart And Stay"))
            {
                ((MainLoop)target).RestartAndStay();
            }

            if (GUILayout.Button("Reconnect"))
            {
                ((MainLoop)target).Reconnect();
            }
        }
    }
#endif
}