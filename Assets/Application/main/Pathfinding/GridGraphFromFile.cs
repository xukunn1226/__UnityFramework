using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Pathfinding;
using System;
using System.IO;

namespace Application.Runtime
{
    /// <summary>
    /// 未完成，未测试
    /// <summary>
    public class GridGraphFromFile : GridGraph, IGridDataSerialization
    {
        public void OnSerializeCountOfRow(Stream stream)
        {
            byte[] bs = BitConverter.GetBytes(countOfRow);
            stream.Write(bs, 0, bs.Length);
        }

        public void OnSerializeCountOfCol(Stream stream)
        {
            byte[] bs = BitConverter.GetBytes(countOfCol);
            stream.Write(bs, 0, bs.Length);
        }

        public void OnSerializeGridSize(Stream stream)
        {
            byte[] bs = BitConverter.GetBytes(gridSize);
            stream.Write(bs, 0, bs.Length);
        }

        public void OnSerializeHeuristic(Stream stream)
        {
            byte[] bs = BitConverter.GetBytes((int)heuristic);
            stream.Write(bs, 0, bs.Length);
        }

        public void OnSerializeIsSkipCorner(Stream stream)
        {
            byte[] bs = BitConverter.GetBytes(isSkipCorner);
            stream.Write(bs, 0, bs.Length);
        }

        public void OnSerializeData(Stream stream)
        {
        }

        public int OnDeserializeCountOfRow(Stream stream)
        {
            // stream.Read()
            return 0;
        }

        public int OnDeserializeCountOfCol(Stream stream)
        {
            return 0;
        }

        public float OnDeserializeGridSize(Stream stream)
        {
            return 0;
        }

        public Heuristic OnDeserializeHeuristic(Stream stream)
        {
            return Heuristic.Manhattan;
        }

        public bool OnDeserializeIsSkipCorner(Stream stream)
        {
            return true;
        }

        public GridData[] OnDeserializeData(Stream stream)
        {
            return null;
        }

        public void ImportFromFile(string filepath)
        {
            using(var fs = new FileStream(filepath, FileMode.Open))
            {
                countOfRow      = OnDeserializeCountOfRow(fs);
                countOfCol      = OnDeserializeCountOfCol(fs);
                gridSize        = OnDeserializeGridSize(fs);
                heuristic       = OnDeserializeHeuristic(fs);
                isSkipCorner    = OnDeserializeIsSkipCorner(fs);
                graph           = OnDeserializeData(fs);
            }

            OnPostprocessNeighbors();
        }

        public void ExportToFile(string filepath)
        {
            using(var fs = new FileStream(filepath, FileMode.Create))
            {
                OnSerializeCountOfRow(fs);
                OnSerializeCountOfCol(fs);
                OnSerializeGridSize(fs);
                OnSerializeHeuristic(fs);
                OnSerializeIsSkipCorner(fs);
                OnSerializeData(fs);
            }
        }
    }
}