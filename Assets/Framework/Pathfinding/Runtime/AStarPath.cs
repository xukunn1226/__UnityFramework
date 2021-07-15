using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Framework.Pathfinding
{
    [RequireComponent(typeof(AStarData))]
    public class AStarPath : MonoBehaviour
    {
        public delegate float OnCalculateValue(ICellData cur, ICellData neighbor);

        // private OnCalculateValue                m_GValueFunc;
        // private OnCalculateValue                m_HValueFunc;
        private SimplePriorityQueue<ICellData>  m_OpenList              = new SimplePriorityQueue<ICellData>(100);       // 小顶堆管理开启列表
        // static private Comparer<ICellData>      s_AscendingComparer     = Comparer<ICellData>.Create(AscendingComparer);

        /// <summary>
        /// A star algorithm
        /// <summary>
        public bool CalculatePath(ICellData srcCellData,
                                  ICellData dstCellData, 
                                  OnCalculateValue gValueFunc,
                                  OnCalculateValue hValueFunc, 
                                  PathReporter result)
        {
            // check source path node validity
            if(srcCellData.state == CellState.Invalid)
            {
                result.status = PathReporterStatus.Invalid;
                Debug.LogError($"Calculate path failed, because source path node's position is Invalid");
                return false;
            }
            if(srcCellData.state == CellState.Blocked)
            {
                result.status = PathReporterStatus.Blocked;
                Debug.LogError($"Calculate path failed, because source path node's status is Blocked");
                return false;
            }

            // check destination path node validity
            if(dstCellData.state == CellState.Invalid)
            {
                result.status = PathReporterStatus.Invalid;
                Debug.LogError($"Calculate path failed, because destination path node's position is Invalid");
                return false;
            }
            if(dstCellData.state == CellState.Blocked)
            {
                result.status = PathReporterStatus.Blocked;
                Debug.LogError($"Calculate path failed, because destination path node's status is Blocked");
                return false;
            }

            // check whether destination grid has been reached or not
            if(srcCellData.Equals(dstCellData))
            {
                return true;
            }

            // m_GValueFunc = gValueFunc;
            // m_HValueFunc = hValueFunc;
            
            m_OpenList.Clear();
            
            // init the starting path node
            srcCellData.details = new CellDetails(null);
            m_OpenList.Push(srcCellData);

            while(m_OpenList.Count > 0)
            {
                // 取f值最小的格子，因是二叉堆数据结构，内部会重新排序
                ICellData curGrid = m_OpenList.Pop();

                // 标记为关闭列表中
                curGrid.details.inClosedList = true;

                // 遍历所有相邻的点
                foreach(var neighbor in curGrid.neighbors)
                {
                    // 不可到达或已在关闭列表中，则略过
                    if( neighbor.state == CellState.Invalid ||
                        neighbor.state == CellState.Blocked ||
                        neighbor?.details?.inClosedList == true)
                        continue;

                    // neighbor可能不在开启列表，需要初始化details
                    if(neighbor.details == null)
                        neighbor.details = new CellDetails(curGrid);

                    // 找到目标点
                    if(neighbor.Equals(dstCellData))
                    {
                        neighbor.details.parent = curGrid;
                        result.dstCell = dstCellData;
                        return true;
                    }

                    // 计算新的g、h、f
                    float gNew = gValueFunc?.Invoke(curGrid, neighbor) ?? 0;
                    float hNew = hValueFunc?.Invoke(curGrid, neighbor) ?? 0;
                    float fNew = gNew + hNew;

                    if( neighbor.details.parent == null ||          // 不在开启列表
                        neighbor.details.f > fNew)                  // 更低的消耗（f）
                    {
                        bool bNew = neighbor.details.parent == null;

                        neighbor.details.f = fNew;
                        neighbor.details.g = gNew;
                        neighbor.details.h = hNew;
                        neighbor.details.parent = curGrid;

                        // 更新开启列表，重新排序
                        if(bNew)
                        {
                            m_OpenList.Push(neighbor);
                        }
                        else
                        {
                            m_OpenList.UpdatePriority(neighbor);
                        }
                    }
                }
            }

            result.status = PathReporterStatus.UnReachable;
            return false;
        }

        // static private int AscendingComparer(ICellData left, ICellData right)
        // {
        //     if(left.details.f > right.details.f)
        //         return 1;
        //     else if(left.details.f < right.details.f)
        //         return -1;
        //     else
        //         return 0;
        // }
    }
}