using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AssetManagement.Runtime.Editor
{
    [CustomEditor(typeof(AssetManager))]
    public class AssetManagerEditor : UnityEditor.Editor
    {
        SerializedProperty m_PreAllocateAssetBundlePoolSizeProp;
        SerializedProperty m_PreAllocateAssetBundleLoaderPoolSizeProp;
        SerializedProperty m_PreAllocateAssetLoaderPoolSizeProp;
        SerializedProperty m_PreAllocateAssetLoaderAsyncPoolSizeProp;

        GUIStyle NormalStyle;
        GUIStyle TitleStyle;
        GUIStyle SubTitleStyle;
        GUIStyle AssetStyle;

        private void OnEnable()
        {
            m_PreAllocateAssetBundlePoolSizeProp = serializedObject.FindProperty("PreAllocateAssetBundlePoolSize");
            m_PreAllocateAssetBundleLoaderPoolSizeProp = serializedObject.FindProperty("PreAllocateAssetBundleLoaderPoolSize");
            m_PreAllocateAssetLoaderPoolSizeProp = serializedObject.FindProperty("PreAllocateAssetLoaderPoolSize");
            m_PreAllocateAssetLoaderAsyncPoolSizeProp = serializedObject.FindProperty("PreAllocateAssetLoaderAsyncPoolSize");
            
            NormalStyle = new GUIStyle();
            NormalStyle.fontSize = 12;
            NormalStyle.fontStyle = FontStyle.Normal;

            TitleStyle = new GUIStyle();
            TitleStyle.fontSize = 16;
            TitleStyle.fontStyle = FontStyle.Bold;

            SubTitleStyle = new GUIStyle();
            SubTitleStyle.fontSize = 14;
            SubTitleStyle.fontStyle = FontStyle.Bold;

            AssetStyle = new GUIStyle();
            AssetStyle.fontSize = 12;
            AssetStyle.fontStyle = FontStyle.Bold;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();


            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(true);
            //EditorGUILayout.TextField("AssetBundles OutputPath", AssetManager.assetBundleOutputPath);
            //EditorGUILayout.LabelField("上述配置不支持编辑器修改，请在AssetManagementSetting中修改");
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_PreAllocateAssetBundlePoolSizeProp, new GUIContent("Pre Allocate AssetBundle Pool Size"));
            EditorGUILayout.PropertyField(m_PreAllocateAssetBundleLoaderPoolSizeProp, new GUIContent("PreAllocate AssetBundleLoader Pool Size"));
            EditorGUILayout.PropertyField(m_PreAllocateAssetLoaderPoolSizeProp, new GUIContent("Pre Allocate AssetLoader Pool Size"));
            EditorGUILayout.PropertyField(m_PreAllocateAssetLoaderAsyncPoolSizeProp, new GUIContent("PreAllocate AssetLoaderAsync Pool Size"));

            
            DrawAssetInfo<GameObject>("Instantiate<GameObject>");

            DrawAssetInfo<Material>("LoadAsset<Material>");

            DrawAssetInfo<Texture2D>("LoadAsset<Texture2D>");

            DrawAssetInfo<AnimationClip>("LoadAsset<AnimationClip>");


            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAssetInfo<T>(string title) where T : UnityEngine.Object
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            DrawAssetLoaderAsync(string.Format("{0}(Async)[{1}]", title, AssetLoaderAsync<T>.Pool?.countAll ?? 0), AssetLoaderAsync<T>.Pool);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            DrawAssetLoader(string.Format("{0}[{1}]", title, AssetLoader<T>.Pool?.countAll ?? 0), AssetLoader<T>.Pool);
        }

        private void DrawAssetLoaderAsync<T>(string title, ResLoaderPool<AssetLoaderAsync<T>> Pool) where T : UnityEngine.Object
        {
            EditorGUILayout.LabelField(title, TitleStyle);
            if (Pool != null)
            {
                // draw active loader
                ++EditorGUI.indentLevel;
                {
                    EditorGUILayout.BeginFoldoutHeaderGroup(true, string.Format("     {0}[{1}]", "Active Loader Pool", Pool.countActive.ToString()), SubTitleStyle);
                    if (Pool.countActive != 0)
                    {
                        ++EditorGUI.indentLevel;
                        LinkedList<AssetLoaderAsync<T>>.Enumerator e = Pool.ActivedPool.GetEnumerator();
                        int index = 0;
                        while (e.MoveNext())
                        {
                            AssetLoaderAsync<T> loader = e.Current;

                            DrawAssetLoaderAsync<T>(index, loader);
                            ++index;
                        }
                        --EditorGUI.indentLevel;
                    }
                    EditorGUI.EndFoldoutHeaderGroup();

                    EditorGUILayout.BeginFoldoutHeaderGroup(true, string.Format("     {0}[{1}]", "Inactive Loader Pool", Pool.countInactive.ToString()), SubTitleStyle);
                    if (Pool.countInactive != 0)
                    {
                        ++EditorGUI.indentLevel;
                        Stack<LinkedListNode<AssetLoaderAsync<T>>>.Enumerator e = Pool.InactivedPool.GetEnumerator();
                        int index = 0;
                        while (e.MoveNext())
                        {
                            AssetLoaderAsync<T> loader = e.Current.Value;

                            DrawAssetLoaderAsync<T>(index, loader);
                            ++index;
                        }
                        --EditorGUI.indentLevel;
                    }
                    EditorGUI.EndFoldoutHeaderGroup();
                }
                --EditorGUI.indentLevel;
            }
        }

        private void DrawAssetLoader<T>(string title, ResLoaderPool<AssetLoader<T>> Pool) where T : UnityEngine.Object
        {
            EditorGUILayout.LabelField(title, TitleStyle);
            if (Pool != null)
            {
                // draw active loader
                ++EditorGUI.indentLevel;
                {
                    EditorGUILayout.BeginFoldoutHeaderGroup(true, string.Format("     {0}[{1}]", "Active Loader Pool", Pool.countActive.ToString()), SubTitleStyle);
                    if (Pool.countActive != 0)
                    {
                        ++EditorGUI.indentLevel;
                        LinkedList<AssetLoader<T>>.Enumerator e = Pool.ActivedPool.GetEnumerator();
                        int index = 0;
                        while (e.MoveNext())
                        {
                            AssetLoader<T> loader = e.Current;

                            DrawAssetLoader<T>(index, loader);
                            ++index;
                        }
                        --EditorGUI.indentLevel;
                    }
                    EditorGUI.EndFoldoutHeaderGroup();

                    EditorGUILayout.BeginFoldoutHeaderGroup(true, string.Format("     {0}[{1}]", "Inactive Loader Pool", Pool.countInactive.ToString()), SubTitleStyle);
                    if (Pool.countInactive != 0)
                    {
                        ++EditorGUI.indentLevel;
                        Stack<LinkedListNode<AssetLoader<T>>>.Enumerator e = Pool.InactivedPool.GetEnumerator();
                        int index = 0;
                        while (e.MoveNext())
                        {
                            AssetLoader<T> loader = e.Current.Value;

                            DrawAssetLoader<T>(index, loader);
                            ++index;
                        }
                        --EditorGUI.indentLevel;
                    }
                    EditorGUI.EndFoldoutHeaderGroup();
                }
                --EditorGUI.indentLevel;
            }
        }

        private void DrawAssetLoaderAsync<T>(int index, AssetLoaderAsync<T> loader) where T : UnityEngine.Object
        {
            EditorGUILayout.LabelField(string.Format("[{0}]  {1}", index, loader.AssetPath), AssetStyle);

            ++EditorGUI.indentLevel;
            DrawAssetBundleLoader(loader.abLoader);
            --EditorGUI.indentLevel;
        }

        private void DrawAssetLoader<T>(int index, AssetLoader<T> loader) where T : UnityEngine.Object
        {
            EditorGUILayout.LabelField(string.Format("[{0}]  {1}", index, loader.AssetPath), AssetStyle);

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
                EditorGUILayout.TextField("main asset bundle", abLoader.mainAssetBundleRef.Value.AssetBundleName);
            }

            if (abLoader.dependentAssetBundleRefs != null && abLoader.dependentAssetBundleRefs.Count != 0)
            {
                for (int i = 0; i < abLoader.dependentAssetBundleRefs.Count; ++i)
                {
                    EditorGUILayout.TextField("dependent asset bundle " + i, abLoader.dependentAssetBundleRefs[i].Value.AssetBundleName);
                }
            }
        }
    }
}