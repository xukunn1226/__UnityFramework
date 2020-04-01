﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Cache.Editor
{
    [CustomEditor(typeof(PoolManager))]
    public class PoolManagerEditor : UnityEditor.Editor
    {
        private Dictionary<Type, IPool> Pools;

        private Dictionary<long, MonoPoolBase> MonoPools;

        private Dictionary<string, IAssetLoaderProxy> AssetLoaders;

        private void OnEnable()
        {
            Pools = PoolManager.Pools;

            MonoPools = PoolManager.MonoPools;

            AssetLoaders = PoolManager.AssetLoaders;
        }

        public override void OnInspectorGUI()
        {
            DrawPools();

            EditorGUILayout.Space();

            DrawMonoPools();

            EditorGUILayout.Space();

            DrawAssetLoaders();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if(GUILayout.Button("Trim"))
            {
                PoolManager.TrimAllObjectPools();
            }

            if(GUILayout.Button("Clear"))
            {
                PoolManager.Clear();
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
                        EditorGUILayout.LabelField(e.Current.Key.Name);

                        string info = string.Format("({0}/{1})", e.Current.Value.countActive, e.Current.Value.countAll);
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
                    EditorGUILayout.BeginHorizontal();
                    {
                        MonoPoolBase pool = e.Current.Value;
                        EditorGUILayout.LabelField(pool.PrefabAsset.name);

                        string info = string.Format("({0}/{1})", pool.countActive, pool.countAll);
                        EditorGUILayout.LabelField(info);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawAssetLoaders()
        {
            EditorGUILayout.LabelField(string.Format("AssetLoaders[{0}]", AssetLoaders.Count), EditorStyles.largeLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                Dictionary<string, IAssetLoaderProxy>.Enumerator e = AssetLoaders.GetEnumerator();
                while (e.MoveNext())
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        MonoPooledObjectBase comp = ((GameObject)e.Current.Value.asset).GetComponent<MonoPooledObjectBase>();
                        EditorGUILayout.LabelField(string.Format("[{0}]{1}", comp.name, e.Current.Key));

                        // TODO: 把所有管理此对象的对象池显示出来
                        MonoPoolBase[] pools = PoolManager.GetMonoPools(comp);
                        if (pools.Length > 0)
                        {
                            string info = string.Format("({0}/{1})", pools[0].countActive, pools[0].countAll);
                            EditorGUILayout.LabelField(info);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
}