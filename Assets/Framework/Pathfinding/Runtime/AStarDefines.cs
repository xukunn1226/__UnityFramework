using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.Pathfinding
{    
    public enum CellState
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
    /// A*算法使用的基础数据结构接口
    /// <summary>
    public interface ICellData : IEquatable<ICellData>, IComparable<ICellData>
    {
        CellState           state       { get; set; }
        List<ICellData>     neighbors   { get; set; }
        CellDetails         details     { get; set; }
    }

    /// <summary>
    /// 计算路径时格子的临时数据，非序列化数据
    /// <summary>
    public class CellDetails
    {
        public CellDetails(ICellData parent)
        {
            this.parent = parent;
            f = 0;
            g = 0;
            h = 0;
            inClosedList = false;
        }

        [NonSerialized] public ICellData    parent;
        [NonSerialized] public float        f;
        [NonSerialized] public float        g;
        [NonSerialized] public float        h;        
        [NonSerialized] public bool         inClosedList;
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
        internal List<ICellData> pathNodeList { get; set; }

        public PathReporterStatus status;

        public PathReporter()
        {}

        public int GetPathsNonAlloc(out ICellData[] results)
        {
            results = null;
            return 0;
        }
    }
}