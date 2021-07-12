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
            this.rowIndex   = rowIndex;
            this.colIndex   = colIndex;
            this.state      = state;
        }

        public int                          rowIndex;
        public int                          colIndex;
        public GridState                    state           = GridState.Reachable;
        public int                          cost;
        [NonSerialized] public GridDetails  details;
    }

    /// <summary>
    /// 计算路径时格子的临时数据，非序列化数据
    /// <summary>
    public class GridDetails
    {
        public GridDetails(GridData parent)
        {
            this.parent = parent;
            f = 0;
            g = 0;
            h = 0;
            inClosedList = false;
        }

        [NonSerialized] public GridData     parent;
        [NonSerialized] public int          f;              // f = g + h
        [NonSerialized] public int          g;
        [NonSerialized] public int          h;        
        [NonSerialized] public bool         inClosedList;
    }

    public struct PathPos
    {
        public int rowIndex;
        public int colIndex;
    }

    public enum PathReporterStatus
    {
        Success,            // 寻路成功
        UnReachable,        // 寻路不成功，因目标点是不可到达
        Blocked,            // 寻路不成功，因目标点是阻挡
        Invalid,            // 寻路不成功，因目标点为无效
    }

    public class PathReporter
    {
        public PathReporterStatus status;

        public PathReporter()
        {}

        public int GetPathsNonAlloc(out PathPos[] results)
        {
            results = null;
            return 0;
        }
    }
}