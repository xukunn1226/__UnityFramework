using UnityEngine;
using UnityEditor;
using Framework.Cache;

namespace Framework.AssetManagement.Runtime.Editor
{
    [CustomEditor(typeof(AssetManager))]
    public class AssetManagerEditor : UnityEditor.Editor
    {
        //SerializedProperty m_PreAllocateAssetBundlePoolSizeProp;
        //SerializedProperty m_PreAllocateAssetBundleLoaderPoolSizeProp;
        //SerializedProperty m_PreAllocateAssetLoaderPoolSizeProp;
        //SerializedProperty m_PreAllocateAssetLoaderAsyncPoolSizeProp;

        GUIStyle AssetStyle;

        bool bFoldout_Prefab = true;
        bool bFoldout_Object = true;
        bool bFoldout_Material = true;
        bool bFoldout_Texture2D = true;
        bool bFoldout_AnimationClip = true;
        bool bFoldout_ScriptableObject = true;
        bool bFoldout_AudioClip = true;

        private void OnEnable()
        {
            //m_PreAllocateAssetBundlePoolSizeProp = serializedObject.FindProperty("PreAllocateAssetBundlePoolSize");
            //m_PreAllocateAssetBundleLoaderPoolSizeProp = serializedObject.FindProperty("PreAllocateAssetBundleLoaderPoolSize");
            //m_PreAllocateAssetLoaderPoolSizeProp = serializedObject.FindProperty("PreAllocateAssetLoaderPoolSize");
            //m_PreAllocateAssetLoaderAsyncPoolSizeProp = serializedObject.FindProperty("PreAllocateAssetLoaderAsyncPoolSize");
            
            AssetStyle = new GUIStyle();
            AssetStyle.fontSize = 12;
            AssetStyle.fontStyle = FontStyle.Bold;

            EditorApplication.hierarchyChanged += ForceUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= ForceUpdate;
        }

        private void ForceUpdate()
        {
            Repaint();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //EditorGUILayout.PropertyField(m_PreAllocateAssetBundlePoolSizeProp, new GUIContent("Pre Allocate AssetBundle Pool Size"));
            //EditorGUILayout.PropertyField(m_PreAllocateAssetBundleLoaderPoolSizeProp, new GUIContent("PreAllocate AssetBundleLoader Pool Size"));
            //EditorGUILayout.PropertyField(m_PreAllocateAssetLoaderPoolSizeProp, new GUIContent("Pre Allocate AssetLoader Pool Size"));
            //EditorGUILayout.PropertyField(m_PreAllocateAssetLoaderAsyncPoolSizeProp, new GUIContent("PreAllocate AssetLoaderAsync Pool Size"));


            bFoldout_Prefab = DrawAssetInfo<GameObject>("<Prefab>", bFoldout_Prefab);

            EditorGUILayout.Space();

            bFoldout_Object = DrawAssetInfo<UnityEngine.Object>("<Object>", bFoldout_Object);

            EditorGUILayout.Space();

            bFoldout_Material = DrawAssetInfo<Material>("<Material>", bFoldout_Material);

            EditorGUILayout.Space();

            bFoldout_Texture2D = DrawAssetInfo<Texture2D>("<Texture2D>", bFoldout_Texture2D);

            EditorGUILayout.Space();

            bFoldout_AnimationClip = DrawAssetInfo<AnimationClip>("<AnimationClip>", bFoldout_AnimationClip);

            EditorGUILayout.Space();

            bFoldout_ScriptableObject = DrawAssetInfo<ScriptableObject>("<ScriptableObject>", bFoldout_ScriptableObject);

            EditorGUILayout.Space();

            bFoldout_AudioClip = DrawAssetInfo<AudioClip>("<AudioClip>", bFoldout_AudioClip);

            serializedObject.ApplyModifiedProperties();
        }

        private bool DrawAssetInfo<T>(string title, bool bFoldout) where T : UnityEngine.Object
        {
            GUIStyle boldStyle = EditorStyles.foldout;
            boldStyle.alignment = TextAnchor.MiddleLeft;
            boldStyle.fontSize = 14;
            bFoldout = EditorGUILayout.Foldout(bFoldout, title, boldStyle);
            
            if (bFoldout)
            {
                GUILayout.BeginVertical(new GUIStyle("HelpBox"));
                {
                    DrawAssetLoader(AssetLoader<T>.kPool);

                    EditorGUILayout.Separator();

                    DrawAssetLoaderAsync(AssetLoaderAsync<T>.kPool);
                }
                GUILayout.EndVertical();
            }
            return bFoldout;
        }

        private void DrawAssetLoader<T>(LinkedObjectPool<AssetLoader<T>> Pool) where T : UnityEngine.Object
        {
            string title = string.Format("Sync Loader [{0}/{1}]", Pool?.countOfUsed ?? 0, Pool?.countAll ?? 0);
            EditorGUILayout.LabelField(title, EditorStyles.label);
            if (Pool != null)
            {
                // draw used loader
                if (Pool.countOfUsed != 0)
                {
                    int index = 0;
                    foreach (var loader in Pool)
                    {
                        DrawAssetLoader<T>(index, loader);
                        ++index;
                    }
                }
            }
        }

        private void DrawAssetLoaderAsync<T>(LinkedObjectPool<AssetLoaderAsync<T>> Pool) where T : UnityEngine.Object
        {
            string title = string.Format("Async Loader [{0}/{1}]", Pool?.countOfUsed ?? 0, Pool?.countAll ?? 0);
            EditorGUILayout.LabelField(title, EditorStyles.label);
            if (Pool != null)
            {
                // draw used loader
                if (Pool.countOfUsed != 0)
                {
                    int index = 0;
                    foreach (var loader in Pool)
                    {
                        DrawAssetLoaderAsync<T>(index, loader);
                        ++index;
                    }
                }
            }
        }

        private void DrawAssetLoaderAsync<T>(int index, AssetLoaderAsync<T> loader) where T : UnityEngine.Object
        {
            EditorGUILayout.LabelField(string.Format("[{0}]  {1}", index, loader.assetPath), AssetStyle);

            ++EditorGUI.indentLevel;
            DrawAssetBundleLoader(loader.abLoader);
            --EditorGUI.indentLevel;
        }

        private void DrawAssetLoader<T>(int index, AssetLoader<T> loader) where T : UnityEngine.Object
        {
            EditorGUILayout.LabelField(string.Format("[{0}]  {1}", index, loader.assetPath), AssetStyle);

            ++EditorGUI.indentLevel;
            DrawAssetBundleLoader(loader.abLoader);
            --EditorGUI.indentLevel;
        }

        private void DrawAssetBundleLoader(AssetBundleLoader abLoader)
        {
            if (abLoader == null)
                return;

            if (abLoader.mainAssetBundleRef != null)
            {
                EditorGUILayout.TextField(string.Format("[refs: {0}] main asset bundle", abLoader.mainAssetBundleRef.refs), abLoader.mainAssetBundleRef.assetBundleName);
            }

            if (abLoader.dependentAssetBundleRefs != null && abLoader.dependentAssetBundleRefs.Count != 0)
            {
                for (int i = 0; i < abLoader.dependentAssetBundleRefs.Count; ++i)
                {
                    EditorGUILayout.TextField(string.Format("[refs: {0}] dependent asset bundle {1}", abLoader.dependentAssetBundleRefs[i].refs, i), abLoader.dependentAssetBundleRefs[i].assetBundleName);
                }
            }
        }
    }
}