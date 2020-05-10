using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using UnityEngine.U2D;

namespace Tests
{
    public class TestSpriteAtlasManager : MonoBehaviour
    {
        public LoaderType m_Type;
        string info;

        public SpriteAtlas m_AtlasHD;
        public SpriteAtlas m_AtlasSD;

        private void Awake()
        {
            ResourceManager.Init(m_Type);
        }

        private void OnDestroy()
        {
            ResourceManager.Uninit();
        }
        void OnEnable()
        {
            SpriteAtlasManager.atlasRequested += RequestAtlas;
            //SpriteAtlasManager.atlasRegistered += AtlasRegistered;
        }

        void OnDisable()
        {
            SpriteAtlasManager.atlasRequested -= RequestAtlas;
            //SpriteAtlasManager.atlasRegistered -= AtlasRegistered;
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 200, 80), "Load"))
            {
                Load();
            }

            if (GUI.Button(new Rect(100, 280, 200, 80), "Load2"))
            {
                Load2();
            }

            if (!string.IsNullOrEmpty(info))
            {
                GUI.Label(new Rect(100, 600, 500, 100), info);
            }
        }

        void RequestAtlas(string tag, System.Action<SpriteAtlas> callback)
        {
            Debug.Log($"{Time.frameCount}   RequestAtlas： {tag}");

            DoLoadSprite(tag, callback);

            //StartCoroutine(DoLoadSpriteAsync(tag, callback));
        }

        void AtlasRegistered(SpriteAtlas spriteAtlas)
        {
            Debug.LogFormat($"{Time.frameCount}     Registered {spriteAtlas.name}.");
        }

        private void DoLoadSprite(string tag, System.Action<SpriteAtlas> callback)
        {
            if (tag == "NewSpriteAtlas1")
            {
                AssetLoader<SpriteAtlas> st = ResourceManager.LoadAsset<SpriteAtlas>("assets/gameplay/tests/runtime/res/atlas/newspriteatlas1.spriteatlas");
                callback(st.asset);
            }

            if (tag == "NewSpriteAtlas2")
            {
                AssetLoader<SpriteAtlas> st = ResourceManager.LoadAsset<SpriteAtlas>("assets/gameplay/tests/runtime/res/atlas2/newspriteatlas2.spriteatlas");
                callback(st.asset);
            }
        }

        private IEnumerator DoLoadSpriteAsync(string tag, System.Action<SpriteAtlas> callback)
        {
            yield return new WaitForSeconds(3);

            if (tag == "NewSpriteAtlas1")
            {
                AssetLoaderAsync<SpriteAtlas> sa = ResourceManager.LoadAssetAsync<SpriteAtlas>("assets/gameplay/tests/runtime/res/atlas/newspriteatlas1.spriteatlas");
                yield return sa;
                callback(sa.asset);
            }
            if (tag == "NewSpriteAtlas2")
            {
                AssetLoaderAsync<SpriteAtlas> sa = ResourceManager.LoadAssetAsync<SpriteAtlas>("assets/gameplay/tests/runtime/res/atlas2/newspriteatlas2.spriteatlas");
                yield return sa;
                callback(sa.asset);
            }
        }

        private void Load()
        {
            Debug.Log($"Load        {Time.frameCount}");
            ResourceManager.InstantiatePrefab("assets/gameplay/tests/runtime/res/prefabs/canvas.prefab");
            //AssetLoader< UnityEngine.U2D.SpriteAtlas> st = ResourceManager.LoadAsset<UnityEngine.U2D.SpriteAtlas>("assets/res/atlas/newspriteatlas1.spriteatlas");
            //if(st.asset != null)
            //{
            //    Sprite s = st.asset.GetSprite("Icon1");
            //    info = string.Format($"{st.asset.spriteCount}  {s?.name}    {s?.texture.name}");
            //    Debug.Log($"{info}");
            //}
        }

        private void Load2()
        {
            Debug.Log($"Load2        {Time.frameCount}");
            ResourceManager.InstantiatePrefab("assets/gameplay/tests/runtime/res/prefabs/canvas2.prefab");
        }

        private void Load3()
        {
            AssetLoader<SpriteAtlas> st = ResourceManager.LoadAsset<SpriteAtlas>("assets/gameplay/tests/runtime/res/atlas2/newspriteatlas2.spriteatlas");
            if (st.asset != null)
            {
                Sprite s = st.asset.GetSprite("Icon2");
                info = string.Format($"{st.asset.spriteCount}  {s?.name}    {s?.texture.name}");
                Debug.Log($"{info}");
            }
        }

        private void Load4()
        {
            if (m_AtlasHD == null)
                return;

            m_AtlasHD.GetSprite("icon1");
        }

        private void LoadHD()
        {
            m_AtlasHD.GetSprite("Icon1");
        }

        private void LoadSD()
        {
            m_AtlasSD.GetSprite("Icon1");
        }
    }
}