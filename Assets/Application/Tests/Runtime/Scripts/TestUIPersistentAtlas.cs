using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Application.Runtime.Tests
{
    public class TestUIPersistentAtlas : MonoBehaviour
    {
        public List<string> m_SpriteAtlases;

        private void Awake()
        {
            //ResourceManager.GetAtlas("1.Bag", "sdsdf");
            foreach (var atlasName in m_SpriteAtlases)
            {
                // ResourceManager.RegisterPersistentAtlas(atlasName, gameObject.name);
            }            
        }

        private void OnDestroy()
        {
            //ResourceManager.ReleaseAtlas("1.Bag", "sdsdf");
            foreach (var atlasName in m_SpriteAtlases)
            {
                // ResourceManager.UnregisterPersistentAtlas(atlasName, gameObject.name);
            }
        }
    }
}