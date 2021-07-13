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
        private BinaryHeap<GridData>              m_OpenList              = new BinaryHeap<GridData>(s_AscendingComparer);
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
                return true;
            
            m_OpenList.Clear();
            
            // init the starting path node
            srcGridData.details = new GridDetails(srcGridData);
            m_OpenList.Push(srcGridData);

            while(m_OpenList.Count > 0)
            {
                // 取f值最小的格子，因是二叉堆数据结构，内部会重新排序
                GridData minData = m_OpenList.Pop();

                // 标记为关闭列表中
                minData.details.inClosedList = true;

                // 遍历所有相邻的点
            }

            return true;
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