using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Framework.Pathfinding
{
    [RequireComponent(typeof(AStarData))]
    public class AStarPath : MonoBehaviour
    {
        private AStarData                       m_Data;
        private BinaryHeap<ICellData>           m_OpenList              = new BinaryHeap<ICellData>(s_AscendingComparer);       // 小顶堆管理开启列表
        static private Comparer<ICellData>      s_AscendingComparer     = Comparer<ICellData>.Create(AscendingComparer);

        void Awake()
        {
            m_Data = GetComponent<AStarData>();
        }

        /// <summary>
        /// A star algorithm
        /// <summary>
        public bool CalculatePath(ICellData srcCellData, ICellData dstCellData, PathReporter path)
        {
            // check source path node validity
            if(srcCellData.state == CellState.Invalid)
            {
                path.status = PathReporterStatus.Invalid;
                Debug.LogError($"Calculate path failed, because source path node's position is Invalid: {src.rowIndex}    {src.colIndex}");
                return false;
            }
            if(srcCellData.state == CellState.Blocked)
            {
                path.status = PathReporterStatus.Blocked;
                Debug.LogError($"Calculate path failed, because source path node's status is Blocked: {src.rowIndex}    {src.colIndex}");
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

            // // check whether destination grid has been reached or not
            // if(src.rowIndex == dst.rowIndex && src.colIndex == dst.colIndex)
            // {
            //     return true;
            // }

            // if(srcGridData.Equals(dstGridData))
            // {
            //     return true;
            // }
            
            m_OpenList.Clear();
            
            // init the starting path node
            srcCellData.details = new CellDetails(srcCellData);
            m_OpenList.Push(srcCellData);

            while(m_OpenList.Count > 0)
            {
                // 取f值最小的格子，因是二叉堆数据结构，内部会重新排序
                ICellData curGrid = m_OpenList.Pop();

                // 标记为关闭列表中
                curGrid.details.inClosedList = true;

                // 遍历所有相邻的点
                // DoNeighbor(curGrid, dstGridData, curGrid.rowIndex - 1, curGrid.colIndex, path);        // North
            }

            return true;
        }

        /// <summary>
        /// 处理相邻节点
        /// true：找到目标点；false：未找到目标点
        /// <summary>
        private bool DoNeighbor(ICellData curGrid, ICellData dstGrid, int rowIndex, int colIndex, PathReporter path)
        {
            // if(dstGrid.rowIndex == rowIndex && dstGrid.colIndex == colIndex)
            // {
            //     return true;
            // }

            // GridData neighborGridData = m_Data.GetGridData(rowIndex, colIndex);
            // if(neighborGridData.state == GridState.Invalid)
            // {
            //     path.status = PathReporterStatus.Invalid;
            //     return false;
            // }
            // if(neighborGridData.state == GridState.Blocked)
            // {
            //     path.status = PathReporterStatus.Blocked;
            //     return false;
            // }


            return false;
        }

        static private int AscendingComparer(ICellData left, ICellData right)
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