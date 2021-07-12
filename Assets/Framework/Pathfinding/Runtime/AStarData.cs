using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.Pathfinding
{
    public class AStarData : MonoBehaviour
    {        
        [Range(1, 10000)] public int        countOfRow  = 8;
        [Range(1, 10000)] public int        countOfCol  = 8;
        [Range(0.1f, 100.0f)] public float  gridSize    = 1;
        public Heuristic                    heuristic   = Heuristic.Euclidean;
        public GridData[]                   data        = new GridData[0];
        public GridDetails[]                tempData    = new GridDetails[0];

        private GridData GetGridData(int rowIndex, int colIndex)
        {
            if(data.Length != countOfRow * countOfCol)
                throw new System.Exception($"the data's length not equal to countOfRow*countOfCol({countOfRow}*{countOfCol})");

            if(rowIndex < 0 || rowIndex > countOfRow - 1 || colIndex < 0 || colIndex > countOfCol - 1)
                throw new System.ArgumentOutOfRangeException($"InRow({rowIndex}) OR InCol({colIndex})");

            return data[countOfCol * rowIndex + colIndex];
        }

        public void SetGridData(int rowIndex, int colIndex, GridState state)
        {
            GridData gridData = GetGridData(rowIndex, colIndex);
            gridData.state = state;
        }

#if UNITY_EDITOR
        // 区域大小发生变化时更新接口，默认保留之前的数据
        public void UpdateData(int newCountOfRow, int newCountOfCol)
        {
            GridData[] newData = new GridData[newCountOfRow * newCountOfCol];
            for(int i = 0; i < newCountOfRow; ++i)
            {
                for(int j = 0; j < newCountOfCol; ++j)
                {
                    int index = i * newCountOfCol + j;
                    if(i >= countOfRow || j >= countOfCol || index >= data.Length)
                    {
                        newData[index] = new GridData(i, j);
                    }
                    else
                    {
                        newData[index] = data[index];
                    }
                }
            }
            countOfRow = newCountOfRow;
            countOfCol = newCountOfCol;
            data = newData;
        }
#endif
    }
}