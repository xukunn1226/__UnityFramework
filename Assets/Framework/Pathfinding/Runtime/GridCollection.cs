using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.Pathfinding
{
    // for Grid
    public class GridCollection : MonoBehaviour
    {        
        [Range(1, 2000)] public int         countOfRow  = 8;
        [Range(1, 2000)] public int         countOfCol  = 8;
        [Range(0.1f, 100.0f)] public float  gridSize    = 1;
        public Heuristic                    heuristic   = Heuristic.Euclidean;
        public GridData[]                   data        = new GridData[0];

        void Awake()
        {
            OnPostprocessData();
        }

        // 使用外部数据序列化对象
        public void ImportSerializer(IGridDataSerializer serializer)
        {
            countOfRow  = serializer.OnSerializeCountOfRow();
            countOfCol  = serializer.OnSerializeCountOfCol();
            gridSize    = serializer.OnSerializeGridSize();
            heuristic   = serializer.OnSerializeHeuristic();
            data        = serializer.OnSerializeData();

            OnPostprocessData();
        }

        // 序列化之后对数据再处理，例如details, neighbors
        private void OnPostprocessData()
        {

        }

        private int GetGridIndex(int rowIndex, int colIndex)
        {
            return countOfCol * rowIndex + colIndex;
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