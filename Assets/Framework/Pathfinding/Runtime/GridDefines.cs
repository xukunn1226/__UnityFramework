using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Pathfinding
{    
    public interface IGridDataSerializer
    {
        int         OnSerializeCountOfRow();
        int         OnSerializeCountOfCol();
        float       OnSerializeGridSize();
        Heuristic   OnSerializeHeuristic();        
        GridData[]  OnSerializeData();                      // 二进制数据序列化为GridData
    }

    public class GridData : ICellData
    {
        [SerializeField]
        private CellState           m_State;
        public CellState            state       { get { return m_State; } set { m_State = value; } }
        public List<ICellData>      neighbors   { get; set; }
        public CellDetails          details     { get; set; }

        public int                  rowIndex;
        public int                  colIndex;

        public bool Equals(ICellData other)
        {
            GridData data = (GridData)other;
            if(data == null)
                return false;

            return rowIndex == data.rowIndex && colIndex == data.colIndex;
        }

        public int CompareTo(ICellData other)
        {
            if(details.f > other.details.f)
                return 1;
            else if(details.f < other.details.f)
                return -1;
            else
                return 0;
        }
    }
}