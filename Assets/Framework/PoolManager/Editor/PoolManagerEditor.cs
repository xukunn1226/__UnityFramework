using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

namespace Framework.Cache.Editor
{
    [CustomEditor(typeof(PoolManager))]
    public class PoolManagerEditor : UnityEditor.Editor
    {
        private Dictionary<Type, IPool>                             Pools;

        private Dictionary<long, MonoPoolBase>                      MonoPools;

        // private Dictionary<string, IAssetLoader>                    ScriptedPools;

        private Dictionary<string, PoolManager.PrefabedPoolInfo>  PrefabedPools;

        // private Dictionary<string, PoolManager.LRUPoolInfo>         LRUPools;

        private void OnEnable()
        {
            Pools = PoolManager.Pools;
            MonoPools = PoolManager.MonoPools;
            PrefabedPools = PoolManager.PrefabedPools;

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
            base.OnInspectorGUI();
            
            DrawPools();

            EditorGUILayout.Space();

            DrawMonoPools();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // DrawAssetLoaders();

            EditorGUILayout.Space();

            DrawPrefabedPools();

            EditorGUILayout.Space();

            // DrawLRUPools();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Trim"))
                {
                    PoolManager.Trim();
                }

                // if(GUILayout.Button("Clear"))
                // {
                //     PoolManagerEx.Destroy();
                // }

                if (GUILayout.Button("Destroy"))
                {
                    PoolManager.Destroy();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPools()
        {
            EditorGUILayout.LabelField(string.Format("Pools[{0}]", Pools.Count), EditorStyles.largeLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                Dictionary<Type, IPool>.Enumerator e = Pools.GetEnumerator();
                while(e.MoveNext())
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField(e.Current.Key.ToString().Substring(e.Current.Key.Namespace.Length + 1));

                        string info = string.Format("({0}/{1})", e.Current.Value.countOfUsed, e.Current.Value.countAll);
                        EditorGUILayout.LabelField(info);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawMonoPools()
        {
            EditorGUILayout.LabelField(string.Format("MonoPools[{0}]", MonoPools.Count), EditorStyles.largeLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                Dictionary<long, MonoPoolBase>.Enumerator e = MonoPools.GetEnumerator();
                while (e.MoveNext())
                {
                    if (e.Current.Value.PrefabAsset == null) continue;

                    EditorGUILayout.BeginHorizontal();
                    {
                        MonoPoolBase pool = e.Current.Value;
                        EditorGUILayout.LabelField(pool.PrefabAsset.name);

                        string info = string.Format("({0}/{1})", pool.countOfUsed, pool.countAll);
                        EditorGUILayout.LabelField(info);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
        }

        // private void DrawAssetLoaders()
        // {
        //     EditorGUILayout.LabelField(string.Format("AssetLoaders[{0}]", ScriptedPools.Count), EditorStyles.largeLabel);
        //     EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        //     {
        //         Dictionary<string, IAssetLoader>.Enumerator e = ScriptedPools.GetEnumerator();
        //         while (e.MoveNext())
        //         {
        //             //if (e.Current.Value.asset == null) continue;

        //             EditorGUILayout.BeginHorizontal();
        //             {
        //                 MonoPooledObject comp = ((GameObject)e.Current.Value.asset).GetComponent<MonoPooledObject>();
        //                 EditorGUILayout.LabelField(string.Format("[{0}]{1}", comp.name, e.Current.Key));

        //                 // TODO: 把所有管理此对象的对象池显示出来
        //                 MonoPoolBase[] pools = PoolManager.GetMonoPools(comp);
        //                 if (pools.Length > 0)
        //                 {
        //                     string info = string.Format("({0}/{1})", pools[0].countOfUsed, pools[0].countAll);
        //                     EditorGUILayout.LabelField(info);
        //                 }
        //             }
        //             EditorGUILayout.EndHorizontal();
        //         }
        //     }
        //     EditorGUILayout.EndVertical();
        // }

        private void DrawPrefabedPools()
        {
            EditorGUILayout.LabelField(string.Format("PrefabedPools[{0}]", PrefabedPools.Count), EditorStyles.largeLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                Dictionary<string, PoolManager.PrefabedPoolInfo>.Enumerator e = PrefabedPools.GetEnumerator();
                while (e.MoveNext())
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        string displayName = e.Current.Key;
                        displayName = displayName.Substring(displayName.LastIndexOf("/") + 1);
                        EditorGUILayout.LabelField(new GUIContent(string.Format("{0}   [refCount:{1}]", displayName, e.Current.Value.m_RefCount), e.Current.Key));

                        string info = string.Format("({0}/{1})", e.Current.Value.m_Pool.countOfUsed, e.Current.Value.m_Pool.countAll);
                        EditorGUILayout.LabelField(info);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
        }

        // private void DrawLRUPools()
        // {
        //     EditorGUILayout.LabelField(string.Format("LRU Pools[{0}]", LRUPools.Count), EditorStyles.largeLabel);
        //     EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        //     {
        //         Dictionary<string, PoolManager.LRUPoolInfo>.Enumerator e = LRUPools.GetEnumerator();
        //         while (e.MoveNext())
        //         {
        //             EditorGUILayout.BeginHorizontal();
        //             {
        //                 string displayName = e.Current.Key;
        //                 displayName = displayName.Substring(displayName.LastIndexOf("/") + 1);
        //                 EditorGUILayout.LabelField(new GUIContent(string.Format("{0}   [refCount:{1}]", displayName, e.Current.Value.m_RefCount), e.Current.Key));

        //                 string info = string.Format("({0}/{1})", e.Current.Value.m_Pool.countOfUsed, e.Current.Value.m_Pool.countAll);
        //                 EditorGUILayout.LabelField(info);
        //             }
        //             EditorGUILayout.EndHorizontal();
        //         }
        //     }
        //     EditorGUILayout.EndVertical();
        // }
    }
}