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

        private AssetLoader<SpriteAtlas> m_AtlasLoader;
        private AssetLoaderAsync<SpriteAtlas> m_AtlasLoaderAsync;
        private GameObject m_Canvas;

        private AssetLoader<SpriteAtlas> m_AtlasLoader2;
        private AssetLoaderAsync<SpriteAtlas> m_AtlasLoaderAsync2;
        private GameObject m_Canvas2;

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
            UnityEngine.U2D.SpriteAtlasManager.atlasRequested += RequestAtlas;
        }

        void OnDisable()
        {
            UnityEngine.U2D.SpriteAtlasManager.atlasRequested -= RequestAtlas;
        }

        // case 1: 通过实例化UI Prefab验证spriteAtlas加载、释放
        // 结论：正确释放，释放后再次触发回调RequestAtlas
        //private void OnGUI()
        //{
        //    if (GUI.Button(new Rect(100, 100, 120, 60), "Load"))
        //    {
        //        InstantiateCanvas(1);
        //    }

        //    if (GUI.Button(new Rect(100, 300, 120, 60), "Unload Canvas"))
        //    {
        //        UnloadCanvas(1);
        //    }
        //}

        // case 2: 直接验证Atlas的加载、释放
        // 结论：正确释放，除首次外，释放后无法触发回调RequestAtlas
        //private void OnGUI()
        //{
        //    if (GUI.Button(new Rect(100, 100, 120, 60), "Load"))
        //    {
        //        LoadAtlas(1);
        //    }

        //    if (GUI.Button(new Rect(100, 300, 120, 60), "Unload Canvas"))
        //    {
        //        UnloadCanvas(1);
        //    }

        //    if (!string.IsNullOrEmpty(info))
        //    {
        //        GUI.Label(new Rect(100, 500, 120, 60), info);
        //    }
        //}

        // case 3： 上述两种方法的混合使用
        //private void OnGUI()
        //{
        //    if (GUI.Button(new Rect(100, 100, 120, 60), "LoadAtlas"))
        //    {
        //        LoadAtlas(1);
        //    }

        //    if (GUI.Button(new Rect(100, 200, 120, 60), "InstantiateCanvas"))
        //    {
        //        InstantiateCanvas(1);
        //    }

        //    if (GUI.Button(new Rect(100, 300, 120, 60), "Unload Canvas"))
        //    {
        //        UnloadCanvas(1);
        //    }

        //    if (!string.IsNullOrEmpty(info))
        //    {
        //        GUI.Label(new Rect(100, 600, 500, 100), info);
        //    }
        //}

        // case 4： 替换Canvas的sprite
        //private void OnGUI()
        //{
        //    if (GUI.Button(new Rect(100, 100, 120, 60), "InstantiateCanvas"))
        //    {
        //        InstantiateCanvas(1);
        //    }

        //    //if (GUI.Button(new Rect(100, 200, 120, 60), "InstantiateCanvas2"))
        //    //{
        //    //    InstantiateCanvas(2);
        //    //}

        //    if (GUI.Button(new Rect(100, 300, 120, 60), "Replace Atlas"))
        //    {
        //        ReplaceAtlas();
        //    }

        //    if (GUI.Button(new Rect(100, 400, 120, 60), "Unload Canvas"))
        //    {
        //        UnloadCanvas(1);
        //        UnloadCanvas(2);
        //    }


        //    if (!string.IsNullOrEmpty(info))
        //    {
        //        GUI.Label(new Rect(100, 600, 500, 100), info);
        //    }
        //}

        // case 5: 异步加载图集A，尚未完成有其他指定加载图集A
        private void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 120, 60), "InstantiateCanvas"))
            {
                //m_SkipRequest = false;
                InstantiateCanvas(1);
            }

            if (GUI.Button(new Rect(100, 200, 120, 60), "InstantiateCanvas2"))
            {
                //m_SkipRequest = false;
                InstantiateCanvas(2);
            }

            if (GUI.Button(new Rect(100, 300, 120, 60), "Load Atlas"))
            {
                //m_SkipRequest = true;
                //StartCoroutine(LoadAtlasAsync(1));
                LoadAtlas(1);
            }

            if (GUI.Button(new Rect(100, 400, 120, 60), "Unload Canvas"))
            {
                UnloadCanvas(1);
                UnloadCanvas(2);
            }


            if (!string.IsNullOrEmpty(info))
            {
                GUI.Label(new Rect(100, 600, 500, 100), info);
            }
        }

        private bool m_SkipRequest;

        void RequestAtlas(string tag, System.Action<SpriteAtlas> callback)
        {
            info = string.Format($"{Time.frameCount}   RequestAtlas： {tag}");
            Debug.Log($"{info}");

            //if (m_SkipRequest)
            //    return;

            DoLoadSprite(tag, callback);
            //StartCoroutine(DoLoadSpriteAsync(tag, callback));
        }

        private void UnloadCanvas(int flag = 1)
        {
            info = "";
            if (flag == 1)
            {
                if (m_AtlasLoader != null)
                    ResourceManager.UnloadAsset(m_AtlasLoader);
                if (m_AtlasLoaderAsync != null)
                    ResourceManager.UnloadAsset(m_AtlasLoaderAsync);
                if (m_Canvas != null)
                    Destroy(m_Canvas);

                m_AtlasLoader = null;
                m_AtlasLoaderAsync = null;
                m_Canvas = null;
            }
            else if( flag == 2)
            {
                if (m_AtlasLoader2 != null)
                    ResourceManager.UnloadAsset(m_AtlasLoader2);
                if (m_AtlasLoaderAsync2 != null)
                    ResourceManager.UnloadAsset(m_AtlasLoaderAsync2);
                if (m_Canvas2 != null)
                    Destroy(m_Canvas2);

                m_AtlasLoader2 = null;
                m_AtlasLoaderAsync2 = null;
                m_Canvas2 = null;
            }
        }
        
        private void ReplaceAtlas()
        {
            Debug.Log($"{Time.frameCount}   ReplaceAtlas");
            m_AtlasLoader2 = ResourceManager.LoadAsset<SpriteAtlas>("assets/application/tests/runtime/res/atlas2/newspriteatlas2.spriteatlas");
            if (m_AtlasLoader2.asset != null)
            {
                Sprite s = m_AtlasLoader2.asset.GetSprite("Icon2");

                UnityEngine.UI.Image img = GameObject.FindObjectOfType<UnityEngine.UI.Image>();
                if (img != null)
                {
                    img.sprite = s;
                }
            }
        }

        private void DoLoadSprite(string tag, System.Action<SpriteAtlas> callback)
        {
            Debug.Log($"DoLoadSprite Begin: {Time.frameCount}");

            if (tag == "NewSpriteAtlas1")
            {
                if(m_AtlasLoader == null)
                    m_AtlasLoader = ResourceManager.LoadAsset<SpriteAtlas>("assets/application/tests/runtime/res/atlas/newspriteatlas1.spriteatlas");

                callback(m_AtlasLoader.asset);
            }

            if (tag == "NewSpriteAtlas2")
            {
                if(m_AtlasLoader2 == null)
                    m_AtlasLoader2 = ResourceManager.LoadAsset<SpriteAtlas>("assets/application/tests/runtime/res/atlas2/newspriteatlas2.spriteatlas");
                callback(m_AtlasLoader2.asset);
            }
        }

        private IEnumerator DoLoadSpriteAsync(string tag, System.Action<SpriteAtlas> callback)
        {
            Debug.Log($"DoLoadSpriteAsync Begin: {Time.frameCount}");

            if (tag == "NewSpriteAtlas1")
            {
                if (m_AtlasLoaderAsync == null)
                {
                    m_AtlasLoaderAsync = ResourceManager.LoadAssetAsync<SpriteAtlas>("assets/application/tests/runtime/res/atlas/newspriteatlas1.spriteatlas");
                    yield return m_AtlasLoaderAsync;
                }
                yield return new WaitForSeconds(2);
                callback(m_AtlasLoaderAsync.asset);
            }

            if (tag == "NewSpriteAtlas2")
            {
                if (m_AtlasLoaderAsync2 == null)
                {
                    m_AtlasLoaderAsync2 = ResourceManager.LoadAssetAsync<SpriteAtlas>("assets/application/tests/runtime/res/atlas2/newspriteatlas2.spriteatlas");
                    yield return m_AtlasLoaderAsync2;
                }
                callback(m_AtlasLoaderAsync2.asset);
            }

            Debug.Log($"DoLoadSpriteAsync Done.     {Time.frameCount}");
        }

        private void InstantiateCanvas(int flag = 1)
        {
            Debug.Log($"InstantiateCanvas        {Time.frameCount}   flag: {flag}");
            // if(flag == 1)
            //     m_Canvas = ResourceManager.InstantiatePrefab("assets/application/tests/runtime/res/prefabs/canvas.prefab");
            // else if(flag == 2)
            //     m_Canvas2 = ResourceManager.InstantiatePrefab("assets/application/tests/runtime/res/prefabs/canvas2.prefab");
        }

        private void LoadAtlas(int flag = 1)
        {
            Debug.Log($"LoadAtlas        {Time.frameCount}   flag: {flag}");
            if (flag == 1)
                m_AtlasLoader = ResourceManager.LoadAsset<SpriteAtlas>("assets/application/tests/runtime/res/atlas/newspriteatlas1.spriteatlas");
            else if(flag == 2)
                m_AtlasLoader2 = ResourceManager.LoadAsset<SpriteAtlas>("assets/application/tests/runtime/res/atlas2/newspriteatlas2.spriteatlas");
        }

        private IEnumerator LoadAtlasAsync(int flag = 1)
        {
            Debug.Log($"LoadAtlasAsync        {Time.frameCount}   flag: {flag}");
            if (flag == 1)
            {
                //AssetLoaderAsync<SpriteAtlas> loader;
                m_AtlasLoaderAsync = ResourceManager.LoadAssetAsync<SpriteAtlas>("assets/application/tests/runtime/res/atlas/newspriteatlas1.spriteatlas");
                yield return m_AtlasLoaderAsync;
            }
            else if (flag == 2)
            {
                m_AtlasLoaderAsync2 = ResourceManager.LoadAssetAsync<SpriteAtlas>("assets/application/tests/runtime/res/atlas2/newspriteatlas2.spriteatlas");
                yield return m_AtlasLoaderAsync2;
            }
        }
    }
}