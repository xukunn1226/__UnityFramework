using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.Pathfinding
{
    [RequireComponent(typeof(AStarPath))]
    public class GridGraph : MonoBehaviour
    {        
        [Range(1, 2000)] public int         countOfRow  = 8;
        [Range(1, 2000)] public int         countOfCol  = 8;
        [Range(0.1f, 100.0f)] public float  gridSize    = 1;
        public Heuristic                    heuristic   = Heuristic.Manhattan;
        public GridData[]                   data        = new GridData[0];

        void Awake()
        {
            OnPostprocessData();
        }

        // 使用外部数据序列化对象
        // public void ImportSerializer(IGridDataSerializer serializer)
        // {
        //     countOfRow  = serializer.OnSerializeCountOfRow();
        //     countOfCol  = serializer.OnSerializeCountOfCol();
        //     gridSize    = serializer.OnSerializeGridSize();
        //     heuristic   = serializer.OnSerializeHeuristic();
        //     data        = serializer.OnSerializeData();

        //     OnPostprocessData();
        // }

        // 序列化之后对数据再处理，例如neighbors
        private void OnPostprocessData()
        {
            for(int i = 0; i < data.Length; ++i)
            {
                int rowIndex = i / countOfCol;
                int colIndex = i - rowIndex * countOfCol;

                data[i].neighbors = new List<ICellData>();

                // init neighbors
                AddNeighbor(data[i], rowIndex - 1,  colIndex);          // North
                AddNeighbor(data[i], rowIndex + 1,  colIndex);          // South
                AddNeighbor(data[i], rowIndex,      colIndex + 1);      // East
                AddNeighbor(data[i], rowIndex,      colIndex - 1);      // West
                AddNeighbor(data[i], rowIndex - 1,  colIndex + 1);      // North-East
                AddNeighbor(data[i], rowIndex - 1,  colIndex - 1);      // North-West
                AddNeighbor(data[i], rowIndex + 1,  colIndex + 1);      // South-East
                AddNeighbor(data[i], rowIndex + 1,  colIndex - 1);      // South-West
            }
        }

        private void AddNeighbor(GridData curGrid, int neighborRowIndex, int neighborColIndex)
        {
            if(neighborRowIndex >= 0 && neighborRowIndex < countOfRow && neighborColIndex >= 0 && neighborColIndex < countOfCol)
            {
                curGrid.neighbors.Add(data[neighborRowIndex * countOfCol + neighborColIndex]);
            }
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

        public ICellData GetGridData(int rowIndex, int colIndex)
        {
            if(data.Length != countOfRow * countOfCol)
                throw new System.Exception($"the data's length not equal to countOfRow*countOfCol({countOfRow}*{countOfCol})");

            if(rowIndex < 0 || rowIndex > countOfRow - 1 || colIndex < 0 || colIndex > countOfCol - 1)
                throw new System.ArgumentOutOfRangeException($"InRow({rowIndex}) OR InCol({colIndex})");

            return data[GetGridIndex(rowIndex, colIndex)];
        }

        public void SetGridData(int rowIndex, int colIndex, CellState state)
        {
            ICellData gridData = GetGridData(rowIndex, colIndex);
            gridData.state = state;
        }

#if UNITY_EDITOR
        // 区域大小发生变化时更新接口，默认保留之前的数据
        public void UpdateData(int newCountOfRow, int newCountOfCol)
        {
            // CellData[] newData = new CellData[newCountOfRow * newCountOfCol];
            // for(int rowIndex = 0; rowIndex < newCountOfRow; ++rowIndex)
            // {
            //     for(int colIndex = 0; colIndex < newCountOfCol; ++colIndex)
            //     {
            //         int index = rowIndex * newCountOfCol + colIndex;
            //         if(rowIndex >= countOfRow || colIndex >= countOfCol || index >= data.Length)
            //         {
            //             newData[index] = new CellData(rowIndex, colIndex);
            //         }
            //         else
            //         {
            //             newData[index] = data[index];
            //         }
            //     }
            // }
            // countOfRow = newCountOfRow;
            // countOfCol = newCountOfCol;
            // data = newData;

            OnPostprocessData();
        }
#endif
    }
}