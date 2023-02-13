using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.Core;

namespace Framework.AssetManagement.AssetEditorWindow
{
    [CustomEditor(typeof(PlayerBuilderSetting))]
    public class PlayerBuilderSettingEditor : Editor
    {
        SerializedProperty              m_projectNameProp;
        SerializedProperty              m_developmentProp;
        SerializedProperty              m_compressWithLz4Prop;
        SerializedProperty              m_compressWithLz4HCProp;
        SerializedProperty              m_strictModeProp;
        SerializedProperty              m_releaseNativeProp;
        SerializedProperty              m_useIL2CPPProp;
        SerializedProperty              m_useMTRenderingProp;
        SerializedProperty              m_buildAppBundleProp;
        SerializedProperty              m_createSymbolsProp;
        SerializedProperty              m_useCustomKeystoreProp;
        SerializedProperty              m_keystoreNameProp;
        SerializedProperty              m_keystorePassProp;
        SerializedProperty              m_keyaliasNameProp;
        SerializedProperty              m_keyaliasPassProp;
        SerializedProperty              m_macroDefinesProp;
        SerializedProperty              m_excludedDefinesProp;
        SerializedProperty              m_clearRenderPipelineAssetProp;

        AppVersion                      m_AppVersion;
        PlayerBuilderSetting.VersionChangedMode m_EnumValueIndex;

        private void Awake()
        {
            m_projectNameProp           = serializedObject.FindProperty("projectName");
            m_developmentProp           = serializedObject.FindProperty("development");
            m_compressWithLz4Prop       = serializedObject.FindProperty("compressWithLz4");
            m_compressWithLz4HCProp     = serializedObject.FindProperty("compressWithLz4HC");
            m_strictModeProp            = serializedObject.FindProperty("strictMode");
            m_releaseNativeProp         = serializedObject.FindProperty("releaseNative");
            m_useIL2CPPProp             = serializedObject.FindProperty("useIL2CPP");
            m_useMTRenderingProp        = serializedObject.FindProperty("useMTRendering");
            m_buildAppBundleProp        = serializedObject.FindProperty("buildAppBundle");
            m_createSymbolsProp         = serializedObject.FindProperty("createSymbols");
            m_useCustomKeystoreProp     = serializedObject.FindProperty("useCustomKeystore");
            m_keystoreNameProp          = serializedObject.FindProperty("keystoreName");
            m_keystorePassProp          = serializedObject.FindProperty("keystorePass");
            m_keyaliasNameProp          = serializedObject.FindProperty("keyaliasName");
            m_keyaliasPassProp          = serializedObject.FindProperty("keyaliasPass");

            m_macroDefinesProp          = serializedObject.FindProperty("macroDefines");
            m_excludedDefinesProp       = serializedObject.FindProperty("excludedDefines");
            m_clearRenderPipelineAssetProp = serializedObject.FindProperty("clearRenderPipelineAsset");
        }

        void OnEnable()
        {
            m_AppVersion = AppVersion.EditorLoad();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();

            // draw HEADER
            GUIStyle boldStyle = EditorStyles.boldLabel;
            boldStyle.alignment = TextAnchor.MiddleLeft;
            EditorGUILayout.LabelField("Player Builder Setting  (" + Utility.GetPlatformName() + ")", boldStyle);

            // draw BODY
            DrawPlayerBuilderSetting();

            serializedObject.ApplyModifiedProperties();

            if(EditorGUI.EndChangeCheck())
            {
                AssetDatabase.SaveAssets();
            }
        }

        private void DrawPlayerBuilderSetting()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {                
                string projectName = EditorGUILayout.TextField("ProjectName", m_projectNameProp.stringValue);
                projectName = projectName.Trim(new char[] { '/', '\\', ' ', '\'', '.', '*'});
                m_projectNameProp.stringValue = projectName;

                //EditorGUILayout.BeginHorizontal();
                //EditorGUI.BeginDisabledGroup(true);
                //GUILayout.Label("Full Output:", EditorStyles.largeLabel);
                //GUILayout.Button(((PlayerBuilderSetting)target).GetLocalPathName());
                //EditorGUI.EndDisabledGroup();
                //EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                DrawBuildOptions();

                EditorGUILayout.Space();
                DrawPlayerSettings();

                EditorGUILayout.Space();
                DrawBuildScenes();
            }
            GUILayout.EndVertical();
        }

        private void DrawBuildOptions()
        {
            EditorGUILayout.LabelField("BuildOptions", EditorStyles.largeLabel);
            GUILayout.BeginVertical(new GUIStyle("HelpBox"));
            {
                m_developmentProp.boolValue = EditorGUILayout.Toggle(new GUIContent("Development"), m_developmentProp.boolValue);
                if (!m_developmentProp.boolValue)
                    EditorGUI.BeginDisabledGroup(true);
                if (!m_developmentProp.boolValue)
                    EditorGUI.EndDisabledGroup();

                if (m_compressWithLz4HCProp.boolValue)
                    EditorGUI.BeginDisabledGroup(true);
                m_compressWithLz4Prop.boolValue = EditorGUILayout.Toggle(new GUIContent("CompressWithLz4"), m_compressWithLz4Prop.boolValue);
                if (m_compressWithLz4HCProp.boolValue)
                    EditorGUI.EndDisabledGroup();
                m_compressWithLz4HCProp.boolValue = EditorGUILayout.Toggle(new GUIContent("CompressWithLz4HC"), m_compressWithLz4HCProp.boolValue);
                m_strictModeProp.boolValue = EditorGUILayout.Toggle(new GUIContent("StrictMode"), m_strictModeProp.boolValue);
            }
            GUILayout.EndVertical();
        }

        private void DrawBuildScenes()
        {
            EditorGUILayout.LabelField("Scenes In Build", EditorStyles.largeLabel);
            GUILayout.BeginVertical(new GUIStyle("HelpBox"));
            {
                for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
                {
                    EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];

                    EditorGUI.BeginChangeCheck();

                    bool bExist = AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(scene.guid.ToString())) != null;
                    EditorGUI.BeginDisabledGroup(!bExist);
                    EditorGUILayout.ToggleLeft(new GUIContent(scene.path), scene.enabled);
                    EditorGUI.EndDisabledGroup();

                    if(EditorGUI.EndChangeCheck())
                    {
                        List<EditorBuildSettingsScene> newBuildSettingsScenes = CopyEditorBuildSettingsScene();
                        newBuildSettingsScenes[i].enabled = !newBuildSettingsScenes[i].enabled;
                        EditorBuildSettings.scenes = newBuildSettingsScenes.ToArray();
                        break;
                    }
                }
            }
            GUILayout.EndVertical();
        }

        static private List<EditorBuildSettingsScene> CopyEditorBuildSettingsScene()
        {
            List<EditorBuildSettingsScene> newScenes = new List<EditorBuildSettingsScene>();
            foreach(var scene in EditorBuildSettings.scenes)
            {
                newScenes.Add(new EditorBuildSettingsScene(scene.guid, scene.enabled));
            }
            return newScenes;
        }

        private void DrawPlayerSettings()
        {
            EditorGUILayout.LabelField("PlayerSettings", EditorStyles.largeLabel);
            GUILayout.BeginVertical(new GUIStyle("HelpBox"));
            {
                EditorGUILayout.LabelField("App Identifier", PlayerSettings.applicationIdentifier, GUILayout.Width(400));

                // m_bundleVersionProp.stringValue = EditorGUILayout.TextField("Bundle Version", m_bundleVersionProp.stringValue);

                {
                    EditorGUI.BeginDisabledGroup(m_AppVersion == null);
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField("AppVersion");
                    if(m_AppVersion == null)
                    {
                        GUIStyle style = new GUIStyle(EditorStyles.whiteLabel);
                        style.normal.textColor = Color.red;
                        EditorGUILayout.LabelField("Missing AppVersion asset in Resources folder", style);
                    }
                    else
                    {
                        m_EnumValueIndex = (PlayerBuilderSetting.VersionChangedMode)EditorGUILayout.EnumPopup("", (PlayerBuilderSetting.VersionChangedMode)m_EnumValueIndex, GUILayout.Width(150));
                        ((PlayerBuilderSetting)target).versionChangedMode = (PlayerBuilderSetting.VersionChangedMode)m_EnumValueIndex;

                        switch(((PlayerBuilderSetting)target).versionChangedMode)
                        {
                            case PlayerBuilderSetting.VersionChangedMode.NoChanged:
                            case PlayerBuilderSetting.VersionChangedMode.Increase:
                                EditorGUI.BeginDisabledGroup(true);
                                EditorGUILayout.IntField("", m_AppVersion.MainVersion, GUILayout.Width(60));
                                EditorGUILayout.IntField("", m_AppVersion.MinorVersion, GUILayout.Width(60));
                                EditorGUILayout.IntField("", m_AppVersion.Revision, GUILayout.Width(60));
                                EditorGUI.EndDisabledGroup();
                                break;
                            case PlayerBuilderSetting.VersionChangedMode.Specific:
                                ((PlayerBuilderSetting)target).mainVersion = EditorGUILayout.IntField("", ((PlayerBuilderSetting)target).mainVersion, GUILayout.Width(60));
                                ((PlayerBuilderSetting)target).minorVersion = EditorGUILayout.IntField("", ((PlayerBuilderSetting)target).minorVersion, GUILayout.Width(60));
                                ((PlayerBuilderSetting)target).revision = EditorGUILayout.IntField("", ((PlayerBuilderSetting)target).revision, GUILayout.Width(60));
                                break;
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUI.EndDisabledGroup();
                }

                m_releaseNativeProp.boolValue = EditorGUILayout.Toggle("ReleaseNative", m_releaseNativeProp.boolValue);
                m_useIL2CPPProp.boolValue = EditorGUILayout.Toggle("UseIL2CPP", m_useIL2CPPProp.boolValue);

                m_useMTRenderingProp.boolValue = EditorGUILayout.Toggle("UseMTRendering", m_useMTRenderingProp.boolValue);

                m_buildAppBundleProp.boolValue = EditorGUILayout.Toggle("Build App Bundle", m_buildAppBundleProp.boolValue);

                m_createSymbolsProp.boolValue = EditorGUILayout.Toggle("Create Symbols", m_createSymbolsProp.boolValue);

                m_useCustomKeystoreProp.boolValue = EditorGUILayout.Toggle("Use Custom Keystore", m_useCustomKeystoreProp.boolValue);
                EditorGUI.BeginDisabledGroup(!m_useCustomKeystoreProp.boolValue);
                {
                    m_keystoreNameProp.stringValue = EditorGUILayout.TextField("KeystoreName", m_keystoreNameProp.stringValue);
                    m_keystorePassProp.stringValue = EditorGUILayout.TextField("KeystorePass", m_keystorePassProp.stringValue);
                    m_keyaliasNameProp.stringValue = EditorGUILayout.TextField("KeyaliasName", m_keyaliasNameProp.stringValue);
                    m_keyaliasPassProp.stringValue = EditorGUILayout.TextField("KeyaliasPass", m_keyaliasPassProp.stringValue);
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginChangeCheck();
                m_macroDefinesProp.stringValue = EditorGUILayout.TextField(new GUIContent("Macro Defines(;)"), m_macroDefinesProp.stringValue);
                if(EditorGUI.EndChangeCheck())
                {
                    m_macroDefinesProp.stringValue = m_macroDefinesProp.stringValue.Trim(new char[] { ';' });
                }

                EditorGUI.BeginChangeCheck();
                m_excludedDefinesProp.stringValue = EditorGUILayout.TextField(new GUIContent("Exclude Defines(;)"), m_excludedDefinesProp.stringValue);
                if(EditorGUI.EndChangeCheck())
                {
                    m_excludedDefinesProp.stringValue = m_excludedDefinesProp.stringValue.Trim(new char[] {';'});
                }

                m_clearRenderPipelineAssetProp.boolValue = EditorGUILayout.Toggle("ClearRenderPipelineAsset", m_clearRenderPipelineAssetProp.boolValue);
            }
            GUILayout.EndVertical();
        }
    }
}
