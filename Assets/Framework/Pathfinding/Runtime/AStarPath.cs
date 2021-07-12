using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Pathfinding
{
    [RequireComponent(typeof(AStarData))]
    public class AStarPath : MonoBehaviour
    {
        private AStarData m_Data;

        void Awake()
        {
            m_Data = GetComponent<AStarData>();            
        }

        public bool CalculatePath(Vector2 src, Vector2 dst, PathReporter path)
        {
            return true;
        }
    }
}