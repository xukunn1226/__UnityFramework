using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.Pathfinding
{
    [RequireComponent(typeof(AStarPath))]
    public class GridGraph : MonoBehaviour
    {        
        [Range(1, 2000)] public int         countOfRow      = 8;
        [Range(1, 2000)] public int         countOfCol      = 8;
        [Range(0.1f, 100.0f)] public float  gridSize        = 1;
        public Heuristic                    heuristic       = Heuristic.Manhattan;
        public GridData[]                   graph           = new GridData[0];
        [Tooltip("是否忽略拐角")]
        public bool                         isIgnoreCorner  = true;

        private AStarPath                   m_Algorithm;
        private GridPathReporter            m_Result        = new GridPathReporter();
        public GridPathReporter             result          { get { return m_Result; } }

        void Awake()
        {
            m_Algorithm = GetComponent<AStarPath>();
            if(m_Algorithm == null)
                throw new ArgumentNullException("m_Algorithm == null");

#if UNITY_EDITOR
            UpdateData(countOfRow, countOfCol);         // ensure the graph's length match to countOfRow * countOfCol
#else
            OnPostprocessData();
#endif            
        }        

        // 序列化之后对数据再处理，例如neighbors
        private void OnPostprocessData()
        {
            for(int i = 0; i < graph.Length; ++i)
            {
                int rowIndex = i / countOfCol;
                int colIndex = i - rowIndex * countOfCol;

                graph[i].neighbors = new List<ICellData>();

                // init neighbors
                AddNeighbor(graph[i], rowIndex - 1,  colIndex);          // North
                AddNeighbor(graph[i], rowIndex + 1,  colIndex);          // South
                AddNeighbor(graph[i], rowIndex,      colIndex + 1);      // East
                AddNeighbor(graph[i], rowIndex,      colIndex - 1);      // West
                AddNeighbor(graph[i], rowIndex - 1,  colIndex + 1);      // North-East
                AddNeighbor(graph[i], rowIndex - 1,  colIndex - 1);      // North-West
                AddNeighbor(graph[i], rowIndex + 1,  colIndex + 1);      // South-East
                AddNeighbor(graph[i], rowIndex + 1,  colIndex - 1);      // South-West
            }
        }

        private void AddNeighbor(GridData curGrid, int neighborRowIndex, int neighborColIndex)
        {
            if(neighborRowIndex >= 0 && neighborRowIndex < countOfRow && neighborColIndex >= 0 && neighborColIndex < countOfCol)
            {
                GridData neighbor = graph[neighborRowIndex * countOfCol + neighborColIndex];
                if(!NeedIgnoreNeighbor(curGrid, neighbor))
                {
                    curGrid.neighbors.Add(neighbor);
                }
            }
        }

        // 此邻居节点是否需要忽略（不加入neighbors）
        // 当邻居节点与当前节点呈对角关系，且周边节点存在block、invalid状态时忽略
        private bool NeedIgnoreNeighbor(GridData curGrid, GridData neighbor)
        {
            if(!isIgnoreCorner)
                return false;

            // diagonal position?
            if(Mathf.Abs(curGrid.rowIndex - neighbor.rowIndex) + Mathf.Abs(curGrid.colIndex - neighbor.colIndex) != 2)
                return false;
            
            int minRowIndex = Mathf.Min(curGrid.rowIndex, neighbor.rowIndex);
            int maxRowIndex = Mathf.Max(curGrid.rowIndex, neighbor.rowIndex);
            int minColIndex = Mathf.Min(curGrid.colIndex, neighbor.colIndex);
            int maxColIndex = Mathf.Max(curGrid.colIndex, neighbor.colIndex);

            // [minRowIndex, minColIndex]
            if(HasBlockedOrInvalidNeighbor(curGrid, neighbor, minRowIndex, minColIndex))
            {
                return true;
            }

            // [minRowIndex, maxColIndex]
            if(HasBlockedOrInvalidNeighbor(curGrid, neighbor, minRowIndex, maxColIndex))
            {
                return true;
            }

            // [maxRowIndex, minColIndex]
            if(HasBlockedOrInvalidNeighbor(curGrid, neighbor, maxRowIndex, minColIndex))
            {
                return true;
            }

            // [maxRowIndex, maxColIndex]
            if(HasBlockedOrInvalidNeighbor(curGrid, neighbor, maxRowIndex, maxColIndex))
            {
                return true;
            }
            return false;
        }

        // 判断curGrid与neighbor周边是否有blocked或invalid状态的节点
        private bool HasBlockedOrInvalidNeighbor(GridData curGrid, GridData neighbor, int otherNeighborRowIndex, int otherNeighborColIndex)
        {
            GridData otherNeighbor = graph[otherNeighborRowIndex * countOfCol + otherNeighborColIndex];
            if(!curGrid.Equals(otherNeighbor) && !neighbor.Equals(otherNeighbor))
            { // 另外两个邻居节点
                return otherNeighbor.state == CellState.Blocked || otherNeighbor.state == CellState.Invalid;
            }
            return false;
        }

        public bool CalculatePath(int srcRowIndex, int srcColIndex, int dstRowIndex, int dstColIndex)
        {
            // clear all grid's details
            foreach(var grid in graph)
            {
                grid.details = null;
            }

            GridData srcGrid = graph[srcRowIndex * countOfCol + srcColIndex];
            GridData dstGrid = graph[dstRowIndex * countOfRow + dstColIndex];
            return m_Algorithm.CalculatePath(srcGrid, dstGrid, OnCalculateGValue, OnCalculateHValue, ref m_Result.pathReporter);
        }

        private float OnCalculateGValue(GridData cur, GridData neighbor)
        {
            float place = Mathf.Abs(cur.rowIndex - neighbor.rowIndex) + Mathf.Abs(cur.colIndex - neighbor.colIndex);
            if( place == 1)         // 上下左右方位时
                return 1;
            else if(place == 2)     // 对角方位时
                return 1.414f;
            else
                return 0;
        }

        private float OnCalculateHValue(GridData cur, GridData dst)
        {
            switch(heuristic)
            {
                case Heuristic.Manhattan:
                    return Mathf.Abs(cur.rowIndex - dst.rowIndex) + Mathf.Abs(dst.colIndex - dst.colIndex);
                case Heuristic.Euclidean:
                    return Mathf.Sqrt((cur.rowIndex - dst.rowIndex) * (cur.rowIndex - dst.rowIndex) + (cur.colIndex - dst.colIndex) * (cur.colIndex - dst.colIndex));
                case Heuristic.Diagonal:
                    {
                        float dx = Mathf.Abs(cur.colIndex - dst.colIndex);
                        float dy = Mathf.Abs(cur.rowIndex - dst.rowIndex);
                        return (dx + dy) - 0.586f * Mathf.Min(dx, dy);      // D * (dx + dy) + (D2 - 2 * D) * min(dx, dy)
                    }
                case Heuristic.None:
                    return 0;
            }
            return 0;
        }

        private int GetGridIndex(int rowIndex, int colIndex)
        {
            return rowIndex * countOfCol + colIndex;
        }

        public GridData GetGridData(int rowIndex, int colIndex)
        {
            if(graph.Length != countOfRow * countOfCol)
                throw new System.Exception($"the data's length not equal to countOfRow*countOfCol({countOfRow}*{countOfCol})");

            if(rowIndex < 0 || rowIndex > countOfRow - 1 || colIndex < 0 || colIndex > countOfCol - 1)
                return null;

            return graph[GetGridIndex(rowIndex, colIndex)];
        }

        public void SetGridData(int rowIndex, int colIndex, CellState state)
        {
            GridData center = GetGridData(rowIndex, colIndex);
            center.state = state;

            UpdateNeighbors(center);
        }

        // 因格子状态发生变化，更新邻居节点的数据
        private void UpdateNeighbors(GridData center)
        {
            if(isIgnoreCorner)
                return;

            GridData nNeighbor = GetGridData(center.rowIndex - 1,   center.colIndex);           // North's neighbor
            GridData sNeighbor = GetGridData(center.rowIndex + 1,   center.colIndex);           // South's neighbor
            GridData wNeighbor = GetGridData(center.rowIndex,       center.colIndex - 1);       // West's neighbor
            GridData eNeighbor = GetGridData(center.rowIndex,       center.colIndex + 1);       // East's neighbor
            
            UpdateNeighbor(center, nNeighbor, wNeighbor);       // north-west
            UpdateNeighbor(center, wNeighbor, sNeighbor);       // west-south
            UpdateNeighbor(center, sNeighbor, eNeighbor);       // south-east
            UpdateNeighbor(center, nNeighbor, eNeighbor);       // east-north
        }

        private void UpdateNeighbor(GridData center, GridData n1, GridData n2)
        {
            if(n1 != null && n2 != null)
            {
                if(center.state == CellState.Blocked || center.state == CellState.Invalid)
                { // 中心节点状态变阻挡或无效时，n1和n2互删
                    n1.neighbors.Remove(n2);
                    n2.neighbors.Remove(n1);
                }
                else if(center.state == CellState.Reachable)
                { // 中心节点变可到达时
                    if(!n1.neighbors.Exists((item) =>item.Equals(n2)))
                    {
                        n1.neighbors.Add(n2);
                    }
                    if(!n2.neighbors.Exists((item) =>item.Equals(n1)))
                    {
                        n2.neighbors.Add(n1);
                    }
                }
            }
        }

#if UNITY_EDITOR
        // 区域大小发生变化时更新接口，默认保留之前的数据
        public void UpdateData(int newCountOfRow, int newCountOfCol)
        {
            GridData[] newData = new GridData[newCountOfRow * newCountOfCol];
            for(int rowIndex = 0; rowIndex < newCountOfRow; ++rowIndex)
            {
                for(int colIndex = 0; colIndex < newCountOfCol; ++colIndex)
                {
                    int index = rowIndex * newCountOfCol + colIndex;
                    if(rowIndex >= countOfRow || colIndex >= countOfCol)
                    {
                        // 超出之前范围则重置
                        newData[index] = new GridData(rowIndex, colIndex);
                    }
                    else
                    {
                        // 之前范围之内则继承
                        newData[index] = graph[rowIndex * countOfCol + colIndex];
                    }
                }
            }
            countOfRow = newCountOfRow;
            countOfCol = newCountOfCol;
            graph = newData;

            OnPostprocessData();
        }
#endif
    }
}