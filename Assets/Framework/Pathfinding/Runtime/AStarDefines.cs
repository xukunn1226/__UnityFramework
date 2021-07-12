using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.Pathfinding
{    
    public enum GridState
    {
        Invalid,            // 无效（不在可寻路范围内）
        UnReachable,        // 不可到达（在可寻路范围内）
        Blocked,            // 阻挡（在可寻路范围内）
        Reachable,          // 可到达（在可寻路范围内）
    }

    public enum Heuristic
    {
        Manhattan,
        Diagonal,
        Euclidean,
        None,               // 退化至Dijkstra算法
    }

    /// <summary>
    /// 格子数据，序列化数据
    /// <summary>
    public class GridData
    {
        public GridData(int rowIndex, int colIndex, GridState state = GridState.Reachable)
        {
            this.rowIndex = rowIndex;
            this.colIndex = colIndex;
            this.state = state;
        }

        public int                          rowIndex;
        public int                          colIndex;
        public GridState                    state           = GridState.Reachable;
        public int                          cost;
    }

    /// <summary>
    /// 计算路径时格子的临时数据，非序列化数据
    /// <summary>
    public class GridDetails
    {
        [NonSerialized] public GridData     parent;
        [NonSerialized] public int          f;              // f = g + h
        [NonSerialized] public int          g;
        [NonSerialized] public int          h;        
        [NonSerialized] public bool         inClosedList;
    }

    public class PathReporter
    {
        public PathReporter()
        {}

        public int GetPathsNonAlloc(out Vector2[] results)
        {
            results = null;
            return 0;
        }
    }
}