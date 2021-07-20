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
        private int                 m_HandleOp;
        private GridData            m_SelectedGrid;
        private GridData            m_SourceGrid;
        private GridData            m_DestinationGrid;
        private float               m_LineGap = 0.03f;
        private bool                m_isPainting;
        private bool                m_wasPainting;
        private GridPathReporter    m_Result;
        private Vector2Int[]        m_PathList = new Vector2Int[32];
        private string              m_ResultInfo;

        void OnEnable()
        {
            m_Target = (GridGraph)target;
            m_Result = null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            int newCountOfRow = Mathf.Max(1, EditorGUILayout.DelayedIntField("Count of Row", m_Target.countOfRow));
            int newCountOfCol = Mathf.Max(1, EditorGUILayout.DelayedIntField("Count of Col", m_Target.countOfCol));
            m_Target.isIgnoreCorner = EditorGUILayout.Toggle("isIgnoreCorner", m_Target.isIgnoreCorner);
            if(EditorGUI.EndChangeCheck())
            {
                m_Target.UpdateData(newCountOfRow, newCountOfCol);

                EditorUtility.SetDirty(target);
            }

            EditorGUI.BeginChangeCheck();
            m_Target.gridSize = Mathf.Max(0.1f, EditorGUILayout.DelayedFloatField("Grid Size", m_Target.gridSize));
            m_Target.heuristic = (Heuristic)EditorGUILayout.EnumPopup("Heuristic", m_Target.heuristic);
            if(EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }

            if(GUILayout.Button("PrintIt"))
            {
                m_Target.PrintIt();
            }

            serializedObject.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
            Handles.Label(m_Target.transform.position, "GridGraph");

            /////////////////// draw menu
            Handles.BeginGUI();
            
            GUI.Label(new Rect(20, 40, 100, 90), $"Shift: \n刷新单个格子的状态\n\nShift + Ctrl：\n连续刷新格子状态\n\n{m_SelectedGrid?.rowIndex??-1}  {m_SelectedGrid?.colIndex??-1}  {m_SelectedGrid?.state??CellState.Invalid}", EditorStyles.helpBox);

            int startRow = 150;
            int width_small = 100;
            int width_big = 120;
            int height_small = 30;
            int height_big = 40;
            switch(m_HandleOp)
            {
                case 0:     // for "Reachable" flag
                    GUI.Button(new Rect(20, startRow, width_big, height_big), "Reachable");
                    if(GUI.Button(new Rect(20, startRow + 70, width_small, height_small), "Block"))
                    {
                        m_HandleOp = 1;
                    }
                    if(GUI.Button(new Rect(20, startRow + 140, width_small, height_small), "Invalid"))
                    {
                        m_HandleOp = 2;
                    }
                    if(GUI.Button(new Rect(20, startRow + 210, width_small, height_small), "Pick source"))
                    {
                        m_HandleOp = 3;
                    }
                    if(GUI.Button(new Rect(20, startRow + 280, width_small, height_small), "Pick destination"))
                    {
                        m_HandleOp = 4;
                    }
                    break;
                case 1:     // for "Block" flag
                    if(GUI.Button(new Rect(20, startRow, width_small, height_small), "Reachable"))
                    {
                        m_HandleOp = 0;
                    }
                    GUI.Button(new Rect(20, startRow + 70, width_big, height_big), "Block");
                    if(GUI.Button(new Rect(20, startRow + 140, width_small, height_small), "Invalid"))
                    {
                        m_HandleOp = 2;
                    }
                    if(GUI.Button(new Rect(20, startRow + 210, width_small, height_small), "Pick source"))
                    {
                        m_HandleOp = 3;
                    }
                    if(GUI.Button(new Rect(20, startRow + 280, width_small, height_small), "Pick destination"))
                    {
                        m_HandleOp = 4;
                    }
                    break;
                case 2:     // for "Invalid" flag
                    if(GUI.Button(new Rect(20, startRow, width_small, height_small), "Reachable"))
                    {
                        m_HandleOp = 0;
                    }
                    if(GUI.Button(new Rect(20, startRow + 70, width_small, height_small), "Block"))
                    {
                        m_HandleOp = 1;
                    }
                    GUI.Button(new Rect(20, startRow + 140, width_big, height_big), "Invalid");
                    if(GUI.Button(new Rect(20, startRow + 210, width_small, height_small), "Pick source"))
                    {
                        m_HandleOp = 3;
                    }
                    if(GUI.Button(new Rect(20, startRow + 280, width_small, height_small), "Pick destination"))
                    {
                        m_HandleOp = 4;
                    }
                    break;
                case 3:     // for "Pick source" flag
                    if(GUI.Button(new Rect(20, startRow, width_small, height_small), "Reachable"))
                    {
                        m_HandleOp = 0;
                    }
                    if(GUI.Button(new Rect(20, startRow + 70, width_small, height_small), "Block"))
                    {
                        m_HandleOp = 1;
                    }
                    if(GUI.Button(new Rect(20, startRow + 140, width_small, height_small), "Invalid"))
                    {
                        m_HandleOp = 2;
                    }
                    GUI.Button(new Rect(20, startRow + 210, width_big, height_big), "Pick source");
                    if(GUI.Button(new Rect(20, startRow + 280, width_small, height_small), "Pick destination"))
                    {
                        m_HandleOp = 4;
                    }
                    break;
                case 4:     // for "Pick destination" flag
                    if(GUI.Button(new Rect(20, startRow, width_small, height_small), "Reachable"))
                    {
                        m_HandleOp = 0;
                    }
                    if(GUI.Button(new Rect(20, startRow + 70, width_small, height_small), "Block"))
                    {
                        m_HandleOp = 1;
                    }
                    if(GUI.Button(new Rect(20, startRow + 140, width_small, height_small), "Invalid"))
                    {
                        m_HandleOp = 2;
                    }
                    if(GUI.Button(new Rect(20, startRow + 210, width_small, height_small), "Pick source"))
                    {
                        m_HandleOp = 3;
                    }
                    GUI.Button(new Rect(20, startRow + 280, width_big, height_big), "Pick destination");                    
                    break;
            }

            if(GUI.Button(new Rect(20, startRow + 350, width_big, 40), "Calculate Path"))
            {
                if(m_SourceGrid != null && m_DestinationGrid != null)
                {
                    m_Target.UpdateData(m_Target.countOfRow, m_Target.countOfCol);
                    m_Result = m_Target.CalculatePath(m_SourceGrid.rowIndex, m_SourceGrid.colIndex, m_DestinationGrid.rowIndex, m_DestinationGrid.colIndex);
                    m_Result.GetPathsNonAlloc(m_PathList);
                }
            }

            if(m_Result == null)
            {
                m_ResultInfo = null;
            }
            else
            {
                switch(m_Result.status)
                {
                    case PathReporterStatus.Success:
                        m_ResultInfo = "寻路成功";
                        break;
                    case PathReporterStatus.Blocked:
                        m_ResultInfo = "寻路不成功，因目标点是阻挡";
                        break;
                    case PathReporterStatus.Invalid:
                        m_ResultInfo = "寻路不成功，因目标点为无效";
                        break;
                    case PathReporterStatus.UnReachable:
                        m_ResultInfo = "寻路不成功，因目标点是不可到达";
                        break;
                }
            }
            GUI.Label(new Rect(150, startRow + 350, 200, height_big), m_ResultInfo);
            
            Handles.EndGUI();

            //////////////////// draw grids
            Handles.color = Color.green;
            float gridSize = m_Target.gridSize;            
            for(int row = 0; row < m_Target.countOfRow; ++row)
            {
                for(int col = 0; col < m_Target.countOfCol; ++col)
                {
                    float thickness = 0.5f;

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
                    
                    if(m_SelectedGrid != null && m_SelectedGrid.rowIndex == row&& m_SelectedGrid.colIndex == col)
                    {
                        Handles.color = Color.red;
                    }
                    else if(m_SourceGrid != null && m_SourceGrid.rowIndex == row && m_SourceGrid.colIndex == col)
                    {
                        Handles.color = Color.yellow;
                        thickness = 2;
                    }
                    else if(m_DestinationGrid != null && m_DestinationGrid.rowIndex == row && m_DestinationGrid.colIndex == col)
                    {
                        Handles.color = Color.magenta;
                        thickness = 2;
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
            m_SelectedGrid = null;
            if (Event.current.shift)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                Plane grid = new Plane(m_Target.transform.TransformVector(Vector3.up), m_Target.transform.position);
                float enter;
                if (grid.Raycast(ray, out enter))
                {
                    Vector3 intersection = ray.origin + ray.direction * enter;
                    int selectedRowIndex = (int)((intersection.x - m_Target.transform.position.x) / gridSize);
                    int selectedColIndex = (int)((intersection.z - m_Target.transform.position.z) / gridSize);
                    m_SelectedGrid = m_Target.GetGridData(selectedRowIndex, selectedColIndex);
                }
                RepaintSceneView();
            }

            // if has the selected grid, it will set grid state
            m_wasPainting = m_isPainting;
            m_isPainting = false;
            if (m_SelectedGrid != null)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    SetSelectedGrid();
                    EditorUtility.SetDirty(m_Target);
                }
                
                if(Event.current.shift && Event.current.control)
                {
                    m_isPainting = true;
                }
            }

            if(m_isPainting)
            {
                SetSelectedGrid();
                RepaintSceneView();                
            }

            if(m_wasPainting && !m_isPainting)
            {
                EditorUtility.SetDirty(m_Target);
            }

            // draw path results
            if(m_Result != null && m_Result.status == PathReporterStatus.Success)
            {

            }
        }

        private void SetSelectedGrid()
        {
            switch (m_HandleOp)
            {
                case 0:     // reachable
                    m_Target.SetGridData(m_SelectedGrid.rowIndex, m_SelectedGrid.colIndex, CellState.Reachable);
                    break;
                case 1:     // block
                    m_Target.SetGridData(m_SelectedGrid.rowIndex, m_SelectedGrid.colIndex, CellState.Blocked);
                    break;
                case 2:     // invalid
                    m_Target.SetGridData(m_SelectedGrid.rowIndex, m_SelectedGrid.colIndex, CellState.Invalid);
                    break;
                case 3:     // pick source
                    {
                        GridData selectedGrid = m_Target.GetGridData(m_SelectedGrid.rowIndex, m_SelectedGrid.colIndex);
                        if (selectedGrid.state == CellState.Reachable)
                        {
                            m_SourceGrid = selectedGrid;
                        }
                        break;
                    }
                case 4:     // pick destination
                    {
                        GridData selectedGrid = m_Target.GetGridData(m_SelectedGrid.rowIndex, m_SelectedGrid.colIndex);
                        if (selectedGrid.state == CellState.Reachable)
                        {
                            m_DestinationGrid = selectedGrid;
                        }
                        break;
                    }
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