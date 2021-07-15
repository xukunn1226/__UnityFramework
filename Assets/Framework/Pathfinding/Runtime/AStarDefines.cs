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
        internal ICellData dstCell { get; set; }
        private Stack<ICellData> m_Paths = new Stack<ICellData>();

        public PathReporterStatus status;

        public ICellData[] paths
        {
            get
            {
                if(status == PathReporterStatus.Success)
                {
                    m_Paths.Clear();

                    ICellData curCell = dstCell;
                    while (curCell != null)
                    {
                        m_Paths.Push(curCell);
                        curCell = curCell.details.parent;
                    }

                    ICellData[] list = new ICellData[m_Paths.Count];
                    int index = 0;
                    while(m_Paths.Count != 0)
                    {
                        list[index++] = m_Paths.Pop();
                    }
                    return list;
                }
                else
                {
                    return null;
                }
            }
        }

        public int GetPathsNonAlloc(ICellData[] results)
        {
            if(status == PathReporterStatus.Success)
            {
                m_Paths.Clear();

                ICellData curCell = dstCell;
                while(curCell != null)
                {
                    m_Paths.Push(curCell);
                    curCell = curCell.details.parent;
                }
                
                int count = Mathf.Min(results.Length, m_Paths.Count);
                for(int i = 0; i < count; ++i)
                {
                    results[i] = m_Paths.Pop();
                }
                return count;
            }
            else
            {
                return 0;
            }
        }
    }
}