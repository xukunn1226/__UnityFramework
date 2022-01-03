using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Application.Runtime;
using System;

namespace Application.Editor
{
    static public class CtrlGEditor
    {
        public delegate void onPreprocessQuickLaunch();
        static public event onPreprocessQuickLaunch OnPreprocessQuickLaunch;

        [MenuItem("Tools/Quick Launch %g", false)]
        static void RunOrStopGame()
        {
            InternalRunOrStopGame(true);
        }

        [MenuItem("Tools/Quick Launch Without Build %#z", false)]
        static void RunOrStopGameWithoutBuild()
        {
            InternalRunOrStopGame(false);
        }

        static void InternalRunOrStopGame(bool buildOther)
        {
            if(EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
            else
            {
                if(buildOther)
                {
                    OnPreprocessQuickLaunch?.Invoke();
                }

                EditorPrefs.SetString("CtrlG_PrevScenePath", EditorSceneManager.GetActiveScene().path);
                UnityEngine.SceneManagement.Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                GameObject go = GameObject.Find("/CtrlG");
                if(go == null)
                {
                    new GameObject("CtrlG", typeof(CtrlG));
                }
                EditorApplication.isPlaying = true;
            }
        }
    }
}