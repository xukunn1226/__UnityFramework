using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Framework.Pathfinding;

namespace Application.Runtime
{
    
    public interface IGridDataSerialization
    {
        void        OnSerializeCountOfRow(Stream stream);
        void        OnSerializeCountOfCol(Stream stream);
        void        OnSerializeGridSize(Stream stream);
        void        OnSerializeHeuristic(Stream stream);
        void        OnSerializeIsSkipCorner(Stream stream);
        void        OnSerializeData(Stream stream);

        int         OnDeserializeCountOfRow(Stream stream);
        int         OnDeserializeCountOfCol(Stream stream);
        float       OnDeserializeGridSize(Stream stream);
        Heuristic   OnDeserializeHeuristic(Stream stream);
        bool        OnDeserializeIsSkipCorner(Stream stream);
        GridData[]  OnDeserializeData(Stream stream);
    }
}