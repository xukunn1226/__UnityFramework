using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Framework.Pathfinding
{
    [RequireComponent(typeof(AStarData))]
    public class AStarPath : MonoBehaviour
    {
        private AStarData                   m_Data;
        private BinaryHeap<GridData>        m_OpenList              = new BinaryHeap<GridData>(s_AscendingComparer);        // 小顶堆管理开启列表
        static private Comparer<GridData>   s_AscendingComparer     = Comparer<GridData>.Create(AscendingComparer);

        void Awake()
        {
            m_Data = GetComponent<AStarData>();
        }

        /// <summary>
        /// A star algorithm
        /// <summary>
        public bool CalculatePath(PathPos src, PathPos dst, PathReporter path)
        {
            // check source path node validity
            GridData srcGridData = m_Data.GetGridData(src.rowIndex, src.colIndex);
            if(srcGridData.state == GridState.Invalid)
            {
                path.status = PathReporterStatus.Invalid;
                return false;
            }
            if(srcGridData.state == GridState.Blocked)
            {
                path.status = PathReporterStatus.Blocked;
                return false;
            }

            // check destination path node validity
            GridData dstGridData = m_Data.GetGridData(dst.rowIndex, dst.colIndex);
            if(dstGridData.state == GridState.Invalid)
            {
                path.status = PathReporterStatus.Invalid;
                return false;
            }
            if(dstGridData.state == GridState.Blocked)
            {
                path.status = PathReporterStatus.Blocked;
                return false;
            }

            // check whether destination grid has been reached or not
            if(src.rowIndex == dst.rowIndex && src.colIndex == dst.colIndex)
            {
                return true;
            }
            
            m_OpenList.Clear();
            
            // init the starting path node
            srcGridData.details = new GridDetails(srcGridData);
            m_OpenList.Push(srcGridData);

            while(m_OpenList.Count > 0)
            {
                // 取f值最小的格子，因是二叉堆数据结构，内部会重新排序
                GridData curGrid = m_OpenList.Pop();

                // 标记为关闭列表中
                curGrid.details.inClosedList = true;

                // 遍历所有相邻的点
                DoNeighbor(curGrid, dstGridData, curGrid.rowIndex - 1, curGrid.colIndex, path);        // North
            }

            return true;
        }

        /// <summary>
        /// 处理相邻节点
        /// true：找到目标点；false：未找到目标点
        /// <summary>
        private bool DoNeighbor(GridData curGrid, GridData dstGrid, int rowIndex, int colIndex, PathReporter path)
        {
            if(dstGrid.rowIndex == rowIndex && dstGrid.colIndex == colIndex)
            {
                return true;
            }

            GridData neighborGridData = m_Data.GetGridData(rowIndex, colIndex);
            if(neighborGridData.state == GridState.Invalid)
            {
                path.status = PathReporterStatus.Invalid;
                return false;
            }
            if(neighborGridData.state == GridState.Blocked)
            {
                path.status = PathReporterStatus.Blocked;
                return false;
            }


            return false;
        }

        static private int AscendingComparer(GridData left, GridData right)
        {
            if(left.details.f > right.details.f)
                return 1;
            else if(left.details.f < right.details.f)
                return -1;
            else
                return 0;
        }
    }
}