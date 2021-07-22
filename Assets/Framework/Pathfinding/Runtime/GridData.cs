using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.Pathfinding
{
    [Serializable]
    public class GridData : ICellData
    {
        [SerializeField]
        private CellState           m_State;
        public CellState            state       { get { return m_State; } set { m_State = value; } }
        public List<ICellData>      neighbors   { get; set; }
        public CellDetails          details     { get; set; }

        public int                  rowIndex;
        public int                  colIndex;

        public GridData(int rowIndex, int colIndex, CellState state = CellState.Reachable)
        {
            this.rowIndex = rowIndex;
            this.colIndex = colIndex;
            this.state = state;
        }

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

    public class GridPathReporter
    {
        private static GridData[]   m_Paths         = new GridData[32];
        internal PathReporter       pathReporter    = new PathReporter();        
        public PathReporterStatus   status          { get { return pathReporter.status; } }

        public Vector2Int[] paths
        {
            get
            {                
                ICellData[] data = pathReporter.paths;
                if(data == null)
                    return null;

                Vector2Int[] results = new Vector2Int[data.Length];
                for(int i = 0; i < results.Length; ++i)
                {
                    results[i].x = ((GridData)data[i]).colIndex;
                    results[i].y = ((GridData)data[i]).rowIndex;
                }
                return results;
            }
        }

        public int GetPathsNonAlloc(Vector2Int[] results)
        {
            if(results.Length > m_Paths.Length)
                m_Paths = new GridData[results.Length];
                
            int count = pathReporter.GetPathsNonAlloc(m_Paths);
            if(count == 0)
                return 0;

            for(int i = 0; i < count; ++i)
            {
                results[i].x = ((GridData)m_Paths[i]).colIndex;
                results[i].y = ((GridData)m_Paths[i]).rowIndex;
            }
            return count;
        }
    }
}