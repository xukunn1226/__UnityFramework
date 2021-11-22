using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.Core.Tests;

namespace Framework.Core.Editor
{
    [CustomEditor(typeof(TestQuadTree))]
    public class TestQuadTreeEditor : UnityEditor.Editor
    {
        private QuadTree<TestQuadNodeObject> m_QuadTree;
        private QuadTree<TestQuadNodeObject>.Node m_Root;
        private float m_Thickness = 2;

        void OnEnable()
        {
            
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button("Create QuadTree"))
            {
                ((TestQuadTree)target).CreateQuadTree();
                RepaintSceneView();
            }
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Insert small object"))
            {
                ((TestQuadTree)target).Insert(0.01f, 0.05f);
                RepaintSceneView();
            }
            if(GUILayout.Button("Insert big object"))
            {
                ((TestQuadTree)target).Insert(0.1f, 0.5f);
                RepaintSceneView();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void OnSceneGUI()
        {
            Handles.Label(((TestQuadTree)target).transform.position, "QuadTree");

            m_QuadTree = ((TestQuadTree)target).quadTree;
            if(m_QuadTree == null)
                return;

            m_Root = ((TestQuadTree)target).rootNode;
            if(m_Root == null)
                return;

            Color cachedColor = Handles.color;

            // draw border
            DrawRect(m_Root.rect, ToColor(0));      // 使用第一层颜色绘制边界

            DrawNode(m_Root);

            Handles.color = cachedColor;
        }

        private void DrawNode(QuadTree<TestQuadNodeObject>.Node node)
        {
            if(node == null)
                return;

            if(node.children != null)
            {
                DrawCross(node.rect, ToColor(node.depth + 1));
                Handles.Label(new Vector3(node.rect.center.x + 0.2f, 0, node.rect.center.y - 0.2f), m_QuadTree.Count(node).ToString(), EditorStyles.boldLabel);
            }

            if(node.objects != null)
            {
                foreach(var obj in node.objects)
                {
                    DrawRect(obj.rect, ToColor(node.depth));
                }
            }

            if(node.children != null)
            {
                foreach(var subNode in node.children)
                {
                    DrawNode(subNode);
                }
            }
        }

        private Color ToColor(int depth)
        {
            switch(depth)
            {
                case 0:
                    return Color.cyan;
                case 1:
                    return Color.blue;
                case 2:
                    return Color.yellow;
                case 3:
                    return Color.red;
                case 4:
                    return Color.magenta;
                case 5:
                    return Color.gray;
                case 6:
                    return Color.white;
            }
            return Color.black;
        }

        private void DrawRect(Rect rect, Color clr)
        {
            Handles.color = clr;
            Vector3[] lines = new Vector3[4];
            lines[0] = new Vector3(rect.x, 0, rect.y);
            lines[1] = new Vector3(rect.x + rect.width, 0, rect.y);
            lines[2] = new Vector3(rect.x + rect.width, 0, rect.y + rect.height);
            lines[3] = new Vector3(rect.x, 0, rect.y + rect.height);
            Handles.DrawLine(lines[0], lines[1], m_Thickness);
            Handles.DrawLine(lines[1], lines[2], m_Thickness);
            Handles.DrawLine(lines[2], lines[3], m_Thickness);
            Handles.DrawLine(lines[3], lines[0], m_Thickness);
        }

        private void DrawCross(Rect rect, Color clr)
        {
            Handles.color = clr;
            Handles.DrawLine(new Vector3(rect.center.x - rect.width * 0.5f, 0, rect.center.y), new Vector3(rect.center.x + rect.width * 0.5f, 0, rect.center.y), m_Thickness);
            Handles.DrawLine(new Vector3(rect.center.x, 0, rect.center.y - rect.height * 0.5f), new Vector3(rect.center.x, 0, rect.center.y + rect.height * 0.5f), m_Thickness);
        }

        private void RepaintSceneView()
        {
            if (!UnityEngine.Application.isPlaying || EditorApplication.isPaused)
            {
                SceneView.RepaintAll();
            }
        }
    }
}