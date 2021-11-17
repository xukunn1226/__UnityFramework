using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    public class QuadTreeDrawer : MonoBehaviour
    {
        private IQuadTreeDrawable m_QuadTreeDrawable;

#if UNITY_EDITOR
        void Start()
        {
            MonoBehaviour[] comps = GetComponents<MonoBehaviour>();
            foreach(var comp in comps)
            {
                m_QuadTreeDrawable = comp as IQuadTreeDrawable;
                if(m_QuadTreeDrawable != null)
                    break;
            }

            if(m_QuadTreeDrawable == null)
            {
                Debug.LogError($"can't find any QuadTreeDrawable");
            }
        }

        void OnDrawGizmosSelected()
        {

        }

        void OnDrawGizmos()
        {
            
        }
#endif        
    }

    public interface IQuadTreeDrawable
    {
        QuadTree quadTree { get; }
    }
}