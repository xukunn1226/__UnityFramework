using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework.Pathfinding.Editor
{
    [CustomEditor(typeof(GridGraph))]
    [DisallowMultipleComponent]
    public class GridGraphEditor : UnityEditor.Editor
    {
        private GridGraph           m_Target;
        private int                 m_HandleGrid;

        void OnEnable()
        {
            m_Target = (GridGraph)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            m_Target.countOfRow = Mathf.Max(1, EditorGUILayout.DelayedIntField("Count of Row", m_Target.countOfRow));
            m_Target.countOfCol = Mathf.Max(1, EditorGUILayout.DelayedIntField("Count of Col", m_Target.countOfCol));
            if(EditorGUI.EndChangeCheck())
            {
                m_Target.UpdateData(m_Target.countOfRow, m_Target.countOfCol);

                EditorUtility.SetDirty(target);

                // if (!Application.isPlaying || EditorApplication.isPaused) SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            m_Target.gridSize = Mathf.Max(0.1f, EditorGUILayout.DelayedFloatField("Grid Size", m_Target.gridSize));
            m_Target.heuristic = (Heuristic)EditorGUILayout.EnumPopup("Heuristic", m_Target.heuristic);
            if(EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
            Handles.Label(m_Target.transform.position, "GridGraph");

            // // draw all horizontal lines that along with z axis in local space
            // Vector3 vColDir = m_Target.transform.TransformVector(Vector3.forward * m_CountOfColProp.intValue * m_GridSizeProp.floatValue);
            // for(int row = 0; row < m_CountOfRowProp.intValue + 1; ++row)
            // {
            //     Vector3 p = m_Target.transform.TransformPoint(Vector3.right * row * m_GridSizeProp.floatValue);
            //     Handles.DrawLine(p, p + vColDir, thickness);                
            // }

            // // draw all vertical lines that along with x axis in local space
            // Vector3 vRowDir = m_Target.transform.TransformVector(Vector3.right * m_CountOfRowProp.intValue * m_GridSizeProp.floatValue);
            // for(int col = 0; col < m_CountOfColProp.intValue + 1; ++col)
            // {
            //     Vector3 p = m_Target.transform.TransformPoint(Vector3.forward * col * m_GridSizeProp.floatValue);
            //     Handles.DrawLine(p, p + vRowDir, thickness);
            // }

            Handles.BeginGUI();
            switch(m_HandleGrid)
            {
                case 0:
                    GUI.Button(new Rect(20, 80, 100, 40), "Reachable");
                    if(GUI.Button(new Rect(20, 150, 80, 30), "Block"))
                    {
                        m_HandleGrid = 1;
                    }
                    if(GUI.Button(new Rect(20, 220, 80, 30), "Invalid"))
                    {
                        m_HandleGrid = 2;
                    }
                    break;
                case 1:
                    if(GUI.Button(new Rect(20, 80, 80, 30), "Reachable"))
                    {
                        m_HandleGrid = 0;
                    }
                    GUI.Button(new Rect(20, 150, 100, 40), "Block");
                    if(GUI.Button(new Rect(20, 220, 80, 30), "Invalid"))
                    {
                        m_HandleGrid = 2;
                    }
                    break;
                case 2:
                    if(GUI.Button(new Rect(20, 80, 80, 30), "Reachable"))
                    {
                        m_HandleGrid = 0;
                    }
                    if(GUI.Button(new Rect(20, 150, 80, 30), "Block"))
                    {
                        m_HandleGrid = 1;
                    }
                    GUI.Button(new Rect(20, 220, 100, 40), "Invalid");
                    break;
            }
            Handles.EndGUI();

            Handles.color = Color.green;
            float thickness = 0.5f;
            const float lineGap = 0.005f;
            float gridSize = m_Target.gridSize;
            for(int row = 0; row < m_Target.countOfRow; ++row)
            {
                for(int col = 0; col < m_Target.countOfCol; ++col)
                {
                    Vector3 p0 = Vector3.forward * (col * gridSize + lineGap)       + Vector3.right * (row * gridSize + lineGap);
                    p0 = m_Target.transform.TransformPoint(p0);

                    Vector3 p1 = Vector3.forward * ((col + 1) * gridSize - lineGap) + Vector3.right * (row * gridSize + lineGap);
                    p1 = m_Target.transform.TransformPoint(p1);

                    Vector3 p2 = Vector3.forward * ((col + 1) * gridSize - lineGap) + Vector3.right * ((row + 1) * gridSize - lineGap);
                    p2 = m_Target.transform.TransformPoint(p2);

                    Vector3 p3 = Vector3.forward * (col * gridSize + lineGap)       + Vector3.right * ((row + 1) * gridSize - lineGap);
                    p3 = m_Target.transform.TransformPoint(p3);
                    
                    GridData grid = m_Target.GetGridData(row, col);
                    if(grid == null)
                        continue;
                    
                    Handles.color = Color.red;
                    switch(grid.state)
                    {
                        case CellState.Reachable:
                            Handles.color = Color.green;
                            break;
                        case CellState.Blocked:
                            Handles.color = Color.blue;
                            break;
                        case CellState.Invalid:
                            Handles.color = Color.gray;
                            break;
                    }
                    Handles.DrawLine(p0, p1, thickness);
                    Handles.DrawLine(p1, p2, thickness);
                    Handles.DrawLine(p2, p3, thickness);
                    Handles.DrawLine(p3, p0, thickness);
                }
            }

            if (m_HandleGrid >= 0 && m_HandleGrid <= 2 && Event.current.modifiers == EventModifiers.Shift)
            {
                if (Event.current.type == EventType.MouseMove)
                {
                    Debug.LogWarning("");
                    Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    Plane grid = new Plane(m_Target.transform.TransformVector(Vector3.up), m_Target.transform.position);
                    float enter;
                    if(grid.Raycast(ray, out enter))
                    {
                        Debug.Log($"--{enter}   {ray.origin + ray.direction * enter}");
                    }
                }
            }
        }
    }
}