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
        private int                 m_SelectedRowIndex;
        private int                 m_SelectedColIndex;
        private float               m_LineGap = 0.03f;
        private bool                m_isPainting;
        private bool                m_wasPainting;

        void OnEnable()
        {
            m_Target = (GridGraph)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            int newCountOfRow = Mathf.Max(1, EditorGUILayout.DelayedIntField("Count of Row", m_Target.countOfRow));
            int newCountOfCol = Mathf.Max(1, EditorGUILayout.DelayedIntField("Count of Col", m_Target.countOfCol));
            if(EditorGUI.EndChangeCheck())
            {
                m_Target.UpdateData(newCountOfRow, newCountOfCol);

                EditorUtility.SetDirty(target);
            }

            EditorGUI.BeginChangeCheck();
            m_Target.gridSize = Mathf.Max(0.1f, EditorGUILayout.DelayedFloatField("Grid Size", m_Target.gridSize));
            m_Target.heuristic = (Heuristic)EditorGUILayout.EnumPopup("Heuristic", m_Target.heuristic);
            m_Target.isIgnoreCorner = EditorGUILayout.Toggle("isIgnoreCorner", m_Target.isIgnoreCorner);
            if(EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
            Handles.Label(m_Target.transform.position, "GridGraph");

            /////////////////// draw menu
            Handles.BeginGUI();
            switch(m_HandleGrid)
            {
                case 0:     // for "Reachable" flag
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
                case 1:     // for "Block" flag
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
                case 2:     // for "Invalid" flag
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
            
            GUI.Label(new Rect(20, 280, 100, 80), "Shift: \n刷新单个格子的状态\n\nShift + Ctrl：\n连续刷新格子状态\n", EditorStyles.helpBox);
            Handles.EndGUI();

            //////////////////// draw grids
            Handles.color = Color.green;
            float gridSize = m_Target.gridSize;
            float thickness = 0.5f;
            for(int row = 0; row < m_Target.countOfRow; ++row)
            {
                for(int col = 0; col < m_Target.countOfCol; ++col)
                {
                    Vector3 p0 = Vector3.forward * (col * gridSize + m_LineGap)       + Vector3.right * (row * gridSize + m_LineGap);
                    p0 = m_Target.transform.TransformPoint(p0);

                    Vector3 p1 = Vector3.forward * ((col + 1) * gridSize - m_LineGap) + Vector3.right * (row * gridSize + m_LineGap);
                    p1 = m_Target.transform.TransformPoint(p1);

                    Vector3 p2 = Vector3.forward * ((col + 1) * gridSize - m_LineGap) + Vector3.right * ((row + 1) * gridSize - m_LineGap);
                    p2 = m_Target.transform.TransformPoint(p2);

                    Vector3 p3 = Vector3.forward * (col * gridSize + m_LineGap)       + Vector3.right * ((row + 1) * gridSize - m_LineGap);
                    p3 = m_Target.transform.TransformPoint(p3);
                    
                    GridData grid = m_Target.GetGridData(row, col);
                    if(grid == null)
                        continue;
                    
                    Handles.color = Color.red;
                    if(row == m_SelectedRowIndex && col == m_SelectedColIndex)
                    {
                        Handles.color = Color.red;
                    }
                    else
                    {
                        switch (grid.state)
                        {
                            case CellState.Reachable:
                                Handles.color = Color.green;
                                break;
                            case CellState.Blocked:
                                Handles.color = Color.blue;
                                break;
                            case CellState.Invalid:
                                Handles.color = Color.white;
                                break;
                        }
                    }
                    Handles.DrawLine(p0, p1, thickness);
                    Handles.DrawLine(p1, p2, thickness);
                    Handles.DrawLine(p2, p3, thickness);
                    Handles.DrawLine(p3, p0, thickness);
                }
            }

            // make sure the selected grid
            m_SelectedRowIndex = -1;
            m_SelectedColIndex = -1;
            if (Event.current.shift)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                Plane grid = new Plane(m_Target.transform.TransformVector(Vector3.up), m_Target.transform.position);
                float enter;
                if (grid.Raycast(ray, out enter))
                {
                    Vector3 intersection = ray.origin + ray.direction * enter;
                    m_SelectedRowIndex = (int)((intersection.x - m_Target.transform.position.x) / gridSize);
                    m_SelectedColIndex = (int)((intersection.z - m_Target.transform.position.z) / gridSize);
                }
                RepaintSceneView();
            }

            // if has the selected grid, it will set grid state
            m_wasPainting = m_isPainting;
            m_isPainting = false;
            if (m_SelectedRowIndex >= 0 && m_SelectedRowIndex < m_Target.countOfRow && 
                m_SelectedColIndex >= 0 && m_SelectedColIndex < m_Target.countOfCol)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    PaintSelectedGrid();
                    EditorUtility.SetDirty(m_Target);
                }
                
                if(Event.current.shift && Event.current.control)
                {
                    m_isPainting = true;
                }
            }

            if(m_isPainting)
            {
                PaintSelectedGrid();
                RepaintSceneView();                
            }

            if(m_wasPainting && !m_isPainting)
            {
                EditorUtility.SetDirty(m_Target);
            }
        }

        private void PaintSelectedGrid()
        {
            switch (m_HandleGrid)
            {
                case 0:     // reachable
                    m_Target.SetGridData(m_SelectedRowIndex, m_SelectedColIndex, CellState.Reachable);
                    break;
                case 1:     // block
                    m_Target.SetGridData(m_SelectedRowIndex, m_SelectedColIndex, CellState.Blocked);
                    break;
                case 2:     // invalid
                    m_Target.SetGridData(m_SelectedRowIndex, m_SelectedColIndex, CellState.Invalid);
                    break;
            }
        }

        private void RepaintSceneView()
        {
            if (!Application.isPlaying || EditorApplication.isPaused)
            {
                SceneView.RepaintAll();
            }
        }
    }
}