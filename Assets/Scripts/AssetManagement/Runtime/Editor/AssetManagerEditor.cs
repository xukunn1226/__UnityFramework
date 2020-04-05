using UnityEngine;
using UnityEditor;
using Cache;

namespace AssetManagement.Runtime.Editor
{
    [CustomEditor(typeof(AssetManager))]
    public class AssetManagerEditor : UnityEditor.Editor
    {
        //SerializedProperty m_PreAllocateAssetBundlePoolSizeProp;
        //SerializedProperty m_PreAllocateAssetBundleLoaderPoolSizeProp;
        //SerializedProperty m_PreAllocateAssetLoaderPoolSizeProp;
        //SerializedProperty m_PreAllocateAssetLoaderAsyncPoolSizeProp;

        GUIStyle AssetStyle;

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


            DrawAssetInfo<GameObject>("<Prefab>");

            EditorGUILayout.Space();

            DrawAssetInfo<UnityEngine.Object>("<Object>");

            EditorGUILayout.Space();

            DrawAssetInfo<Material>("<Material>");

            EditorGUILayout.Space();

            DrawAssetInfo<Texture2D>("<Texture2D>");

            EditorGUILayout.Space();

            DrawAssetInfo<AnimationClip>("<AnimationClip>");

            EditorGUILayout.Space();

            DrawAssetInfo<ScriptableObject>("<ScriptableObject>");

            EditorGUILayout.Space();

            DrawAssetInfo<AudioClip>("<AudioClip>");

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAssetInfo<T>(string title) where T : UnityEngine.Object
        {
            GUIStyle boldStyle = EditorStyles.boldLabel;
            boldStyle.alignment = TextAnchor.MiddleLeft;
            boldStyle.fontSize = 14;
            EditorGUILayout.LabelField(title, boldStyle);

            GUILayout.BeginVertical(new GUIStyle("HelpBox"));
            {
                DrawAssetLoader(AssetLoader<T>.kPool);

                EditorGUILayout.Separator();
             
                DrawAssetLoaderAsync(AssetLoaderAsync<T>.kPool);
            }
            GUILayout.EndVertical();
        }

        private void DrawAssetLoader<T>(LinkedObjectPool<AssetLoader<T>> Pool) where T : UnityEngine.Object
        {
            string title = string.Format("Sync Loader [{0}/{1}]", Pool?.countOfUsed ?? 0, Pool?.countAll ?? 0);
            EditorGUILayout.LabelField(title, EditorStyles.label);
            if (Pool != null)
            {
                // draw used loader
                {
                    //EditorGUILayout.BeginFoldoutHeaderGroup(true, string.Format(@"      {0}[{1}]", "Used Loader", Pool.countOfUsed.ToString()), EditorStyles.label);
                    if (Pool.countOfUsed != 0)
                    {
                        LinkedObjectPool<AssetLoader<T>>.Enumerator e = Pool.GetEnumerator();
                        int index = 0;
                        while (e.MoveNext())
                        {
                            DrawAssetLoader<T>(index, e.Current);
                            ++index;
                        }
                        e.Dispose();
                    }
                    //EditorGUI.EndFoldoutHeaderGroup();
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
                {
                    //EditorGUILayout.BeginFoldoutHeaderGroup(true, string.Format("     {0}[{1}]", "Active Loader Pool", Pool.countOfUsed.ToString()), SubTitleStyle);
                    if (Pool.countOfUsed != 0)
                    {
                        LinkedObjectPool<AssetLoaderAsync<T>>.Enumerator e = Pool.GetEnumerator();
                        int index = 0;
                        while (e.MoveNext())
                        {
                            DrawAssetLoaderAsync<T>(index, e.Current);
                            ++index;
                        }
                        e.Dispose();
                    }
                    //EditorGUI.EndFoldoutHeaderGroup();
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