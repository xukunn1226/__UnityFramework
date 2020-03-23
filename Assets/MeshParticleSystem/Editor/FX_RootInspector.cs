using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MeshParticleSystem
{
    [CustomEditor(typeof(FX_Root))]
    public class FX_RootInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            FX_Root root = (FX_Root)target;

            //Color clr = GUI.color;
            //if (root.LifeTime <= 0)
            //{
            //    GUI.color = Color.grey;
            //    if (GUILayout.Button("自动销毁 ：  关闭"))
            //    {
            //        root.LifeTime = 1;
            //    }
            //    GUI.color = clr;
            //}
            //else
            //{
            //    GUI.color = new Color(0.6f, 0.9f, 0.8f);
            //    if (GUILayout.Button("自动销毁 ： 开启"))
            //    {
            //        root.LifeTime = 0;
            //    }
            //    GUI.color = clr;

            //    root.LifeTime = EditorGUILayout.FloatField("生命周期", root.LifeTime);
            //}

            base.OnInspectorGUI();

            GUILayout.Space(10);
            Color clr = GUI.color;
            GUI.color = new Color(0, 1, 0);
            if (GUILayout.Button("Replay"))
            {
                //root.Replay();
            }
            GUI.color = clr;
        }
    }
}