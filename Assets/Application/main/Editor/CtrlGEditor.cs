using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Application.Runtime;

namespace Application.Editor
{
    static public class CtrlGEditor
    {
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
                    ConfigBuilder.DoRun();
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