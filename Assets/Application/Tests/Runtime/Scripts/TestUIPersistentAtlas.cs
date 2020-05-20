using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
{
    public class TestUIPersistentAtlas : MonoBehaviour
    {
        public List<string> m_SpriteAtlases;

        private void OnEnable()
        {
            foreach(var atlasName in m_SpriteAtlases)
            {
                ResourceManager.RegisterPersistentAtlas(atlasName, gameObject.name);
            }
        }

        private void OnDisable()
        {
            foreach(var atlasName in m_SpriteAtlases)
            {
                ResourceManager.UnregisterPersistentAtlas(atlasName, gameObject.name);
            }
        }
    }
}