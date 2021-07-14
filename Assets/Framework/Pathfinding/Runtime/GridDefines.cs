using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Pathfinding
{    
    public interface IGridDataSerializer
    {
        int         OnSerializeCountOfRow();
        int         OnSerializeCountOfCol();
        float       OnSerializeGridSize();
        Heuristic   OnSerializeHeuristic();        
        ICellData[] OnSerializeData();                      // 二进制数据序列化为GridData        
        void        OnPostprocessData();                    // 序列化之后对数据再处理，例如details, neighbors
    }
}