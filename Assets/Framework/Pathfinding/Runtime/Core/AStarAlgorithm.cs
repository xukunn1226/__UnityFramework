using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Framework.Pathfinding
{
    public class AStarAlgorithm
    {
        public delegate float OnCalculateValue<T>(T cur, T other) where T : ICellData;

        static private SimplePriorityQueue<ICellData>  m_OpenList       = new SimplePriorityQueue<ICellData>(100);       // 开启列表（小顶堆）

        /// <summary>
        /// A star algorithm
        /// <summary>
        static internal bool CalculatePath<T>(  T srcCellData, 
                                                T dstCellData,
                                                OnCalculateValue<T> gValueFunc,
                                                OnCalculateValue<T> hValueFunc,
                                                ref PathReporter result) where T : ICellData
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
                result.status = PathReporterStatus.Invalid;
                Debug.LogError($"Calculate path failed, because destination path node is same as source path node");
                return false;
            }

            m_OpenList.Clear();
            
            // init the starting path node
            srcCellData.details = new CellDetails();
            m_OpenList.Push(srcCellData);

            while(m_OpenList.Count > 0)
            {
                // 取f值最小的格子，因是二叉堆数据结构，内部会重新排序
                ICellData curGrid = m_OpenList.Pop();

                // 标记为关闭列表中
                curGrid.details.state = CellDetails.CellPhase.InClosedList;

                // 遍历所有相邻的点
                foreach(var neighbor in curGrid.neighbors)
                {
                    // 不可到达或已在关闭列表中，则略过
                    if( neighbor.state == CellState.Invalid ||
                        neighbor.state == CellState.Blocked ||
                        (neighbor.details != null && neighbor.details.state == CellDetails.CellPhase.InClosedList))
                        continue;

                    // neighbor可能不在开启列表，需要初始化details
                    if(neighbor.details == null)
                        neighbor.details = new CellDetails();

                    // 找到目标点
                    if(neighbor.Equals(dstCellData))
                    {
                        neighbor.details.parent = curGrid;

                        result.status = PathReporterStatus.Success;
                        result.dstCell = dstCellData;
                        return true;
                    }

                    // 计算新的g、h、f
                    float gNew = curGrid.details.g + gValueFunc?.Invoke((T)curGrid, (T)neighbor) ?? 0;
                    float hNew = hValueFunc?.Invoke((T)neighbor, (T)dstCellData) ?? 0;
                    float fNew = gNew + hNew;

                    if( neighbor.details.state == CellDetails.CellPhase.NotInOpenList ||            // 不在开启列表
                        neighbor.details.f > fNew)                                                  // 更低的消耗（f）
                    {
                        neighbor.details.f = fNew;
                        neighbor.details.g = gNew;
                        neighbor.details.h = hNew;
                        neighbor.details.parent = curGrid;

                        if(neighbor.details.state == CellDetails.CellPhase.NotInOpenList)
                        { // 不在开启列表则加入
                            neighbor.details.state = CellDetails.CellPhase.AlreadyInOpenList;
                            m_OpenList.Push(neighbor);
                        }
                        else
                        { // 已在开启列表则更新优先级
                            m_OpenList.UpdatePriority(neighbor);
                        }
                    }
                }
            }

            result.status = PathReporterStatus.UnReachable;
            return false;
        }
    }
}