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
    public interface ICellData :    IEquatable<ICellData>,              // 以此判断是否为目标点
                                    IComparable<ICellData>              // 以此比较f值大小，重排序开启列表
    {
        CellState           state       { get; set; }
        List<ICellData>     neighbors   { get; set; }
        CellDetails         details     { get; set; }                   // 记录寻路过程的临时数据，每次寻路时需要重置
    }

    /// <summary>
    /// 计算路径时格子的临时数据，非序列化数据
    /// <summary>
    public class CellDetails
    {
        public CellDetails()
        {
            parent = null;
            f = 0;
            g = 0;
            h = 0;
            state = CellPhase.NotInOpenList;
        }

        public enum CellPhase
        {
            NotInOpenList,          // 尚未加入开启列表
            AlreadyInOpenList,      // 已在开启列表中
            InClosedList,           // 关闭列表中
        }

        [NonSerialized] public ICellData    parent;
        [NonSerialized] public float        f;
        [NonSerialized] public float        g;
        [NonSerialized] public float        h;        
        [NonSerialized] internal CellPhase  state;
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
        internal ICellData          dstCell     { get; set; }
        private Stack<ICellData>    m_Paths     = new Stack<ICellData>();
        public PathReporterStatus   status;

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

                if(results == null || results.Length < 2)
                    throw new ArgumentException($"GetPathsNonAlloc: results == null || results.Length < 2");
                
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