using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework.Core.Editor
{
    [InitializeOnLoad]
    public static class SyncViewTool
    {
        private const string MenuName_SyncGameViewToSceneView = "Tools/SyncView/Sync GameView to SceneView";
        private const string MenuName_SyncSceneViewToGameView = "Tools/SyncView/Sync SceneView to GameView";
        private const string MenuName_EnableSync = "Tools/SyncView/Toggle Sync View";

        private const string KeyName_SyncView = "SyncViewTool_EnableSyncGameViewToSceneView";
        private const string KeyName_SyncValidate = "SyncViewTool_EnableSync";

        private static bool enableSyncGameViewToSceneView;      // true: sync game view to scene view; false: sync scene view to game view
        private static bool enableSync;                         // whether enable sync between game view and scene view

        static SyncViewTool()
        {
            enableSyncGameViewToSceneView = EditorPrefs.GetBool(KeyName_SyncView, false);
            enableSync = EditorPrefs.GetBool(KeyName_SyncValidate, false);

            ApplyMenu();

            if (enableSync)
                EditorApplication.update += UpdateViewSync;

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnSceneGUI;
#else
        SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
        }

        //[MenuItem(MenuName_EnableSync, false, 0)]
        private static void ToggleViewSync()
        {
            enableSync = !enableSync;
            EditorPrefs.SetBool(KeyName_SyncValidate, enableSync);

            if (enableSync)
                EditorApplication.update += UpdateViewSync;
            else
                EditorApplication.update -= UpdateViewSync;

            ApplyMenu();
        }

        //[MenuItem(MenuName_SyncGameViewToSceneView, false, 1)]
        public static void SyncGameViewToSceneView()
        {
            enableSyncGameViewToSceneView = true;
            EditorPrefs.SetBool(KeyName_SyncView, enableSyncGameViewToSceneView);

            ApplyMenu();
        }

        //[MenuItem(MenuName_SyncGameViewToSceneView, true)]
        public static bool VSyncGameViewToSceneView()
        {
            return enableSync;
        }

        //[MenuItem(MenuName_SyncSceneViewToGameView, false, 2)]
        public static void SyncSceneViewToGameView()
        {
            enableSyncGameViewToSceneView = false;
            EditorPrefs.SetBool(KeyName_SyncView, enableSyncGameViewToSceneView);

            ApplyMenu();
        }

        //[MenuItem(MenuName_SyncSceneViewToGameView, true)]
        public static bool VSyncSceneViewToGameView()
        {
            return enableSync;
        }

        static private void ApplyMenu()
        {
            //Menu.SetChecked(MenuName_SyncGameViewToSceneView, enableSyncGameViewToSceneView);
            //Menu.SetChecked(MenuName_SyncSceneViewToGameView, !enableSyncGameViewToSceneView);
            //Menu.SetChecked(MenuName_EnableSync, enableSync);
        }

        static void UpdateViewSync()
        {
            if (Camera.main == null || SceneView.lastActiveSceneView == null)
                return;

            if (enableSyncGameViewToSceneView)
            {
                SceneView.lastActiveSceneView.AlignViewToObject(Camera.main.transform);
            }
            else
            {
                Camera.main.transform.position = SceneView.lastActiveSceneView.camera.transform.position;
                Camera.main.transform.rotation = SceneView.lastActiveSceneView.camera.transform.rotation;
            }
        }

        static private void OnSceneGUI(SceneView sceneView)
        {
            Handles.BeginGUI();

            GUILayout.BeginHorizontal();

            GUI.color = enableSync ? Color.green : Color.gray;
            if (GUI.Button(new Rect(10, 10, 120, 25), "Enable SyncView", new GUIStyle("LargeButton")))
            {
                ToggleViewSync();
            }
            GUI.color = Color.white;

            if (Camera.main == null || !Camera.main.enabled)
            {
                GUIStyle style = EditorStyles.label;
                Color clr = style.normal.textColor;
                style.normal.textColor = Color.red;
                GUI.Label(new Rect(140, 0, 180, 40), "!!Camera not found or disable", style);
                style.normal.textColor = clr;
            }

            GUILayout.EndHorizontal();

            if (enableSync)
            {
                GUI.color = Color.cyan;
                if (GUI.Button(new Rect(10, 40, 100, 40), enableSyncGameViewToSceneView ? "Scene to Game" : "Game to Scene", EditorStyles.miniButtonLeft))
                {
                    if (enableSyncGameViewToSceneView)
                    {
                        SyncSceneViewToGameView();
                    }
                    else
                    {
                        SyncGameViewToSceneView();
                    }
                }
                GUI.color = Color.white;
            }

            Handles.EndGUI();
        }
    }
}