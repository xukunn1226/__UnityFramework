using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using UnityEngine.U2D;

namespace Tests
{
    public class TestUIAtlas : MonoBehaviour
    {
        public LoaderType m_Type;
        //string info;

        [SoftObject]
        public SoftObject m_SoftObject;
        private GameObject m_Go;

        private SpriteAtlas m_Atlas;

        private void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 120, 60), "Load"))
            {
                m_Go = m_SoftObject?.Instantiate();
            }

            if (GUI.Button(new Rect(100, 200, 120, 60), "Unload Canvas"))
            {
                Destroy(m_Go);
            }

            if (GUI.Button(new Rect(100, 400, 120, 60), "GetAtlas"))
            {
                m_Atlas = ResourceManager.GetAtlas("0.Common", "12321");
            }

            if (GUI.Button(new Rect(100, 500, 120, 60), "ReleaseAtlas"))
            {
                ResourceManager.ReleaseAtlas("0.Common", "12321");
            }


            //if (!string.IsNullOrEmpty(info))
            //{
            //    GUI.Label(new Rect(100, 600, 500, 100), info);
            //}
        }
    }
}