using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnimationInstancingModule.Runtime;
using Framework.AssetManagement.Runtime;

namespace Application.Runtime
{
    /// <summary>
    /// 测试用例，故显示规则如下：
    /// ViewLayer_0: LOD0
    /// ViewLayer_1: LOD1
    /// ViewLayer_2: Sphere
    /// <summary>
    public class TestActor : IEntity, IViewLayer, ILoader
    {
        public string                   name            { get; set; }
        public int                      id              { get; set; }
        public ViewLayer                minViewLayer    { get; set; } = ViewLayer.ViewLayer_0;
        public ViewLayer                maxViewLayer    { get; set; } = ViewLayer.ViewLayer_1;
        public bool                     visible         { get; set; }
        public int                      viewId          { get; set; }
        public string                   assetPath       { get; set; }
        public string                   symbolAssetPath { get; set; }
        public GameObject               display         { get; set; }           // the root gameobject of any display object
        private GameObjectLoaderAsync   m_LoaderAsync;
        private GameObjectLoader        m_ActorLoader;        
        private GameObjectLoader        m_SymbolLoader;
        public int                      loaderId        { get; set; }

        public void Init()
        {
            display = new GameObject();
            display.transform.position = new Vector3(Random.Range(0, 1.0f), 0, Random.Range(0, 1.0f)) * Random.Range(-50, 50);
            display.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
        }

        public void Uninit()
        {
            if(m_LoaderAsync != null)
            {
                ResourceManager.ReleaseInst(m_LoaderAsync);
                m_LoaderAsync = null;
            }

            if(m_ActorLoader != null)
            {
                ResourceManager.ReleaseInst(m_ActorLoader);
                m_ActorLoader = null;
            }

            if(m_SymbolLoader != null)
            {
                ResourceManager.ReleaseInst(m_SymbolLoader);
                m_SymbolLoader = null;
            }

            if(display != null)
            {
                Object.Destroy(display);
            }
        }

        public void OnViewUpdate(ViewLayer layer, float alpha) { }
        public void OnEnter(ViewLayer prevLayer, ViewLayer curLayer)
        {
            if(prevLayer == ViewLayer.ViewLayer_Invalid && curLayer == ViewLayer.ViewLayer_Invalid)
                return;     // ViewLayerManager尚未Update时

            // 请求当前层级的显示对象
            // AssetLoadingAsyncManager.SendAysncLoading(loaderId, LoadActorPrefabAsync(), OnActorPrefabLoadFinished);

            // 0与1层使用资源assetPath
            if(curLayer == ViewLayer.ViewLayer_0 || curLayer == ViewLayer.ViewLayer_1)
            {
                if(m_ActorLoader == null)
                {
                    m_ActorLoader = ResourceManager.Instantiate(assetPath);                    
                    GameObject go = m_ActorLoader.asset;
                    go.transform.parent = display.transform;
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localEulerAngles = Vector3.zero;
                    AnimationInstancing inst = go.GetComponent<AnimationInstancing>();
                    inst.PlayAnimation("walk");
                }
            }

            // 2层使用资源symbolAssetPath
            if(curLayer == ViewLayer.ViewLayer_2)
            {
                if(m_SymbolLoader == null)
                {
                    m_SymbolLoader = ResourceManager.Instantiate(symbolAssetPath);
                    m_SymbolLoader.asset.transform.parent = display.transform;
                    m_SymbolLoader.asset.transform.localPosition = Vector3.zero;
                    m_SymbolLoader.asset.transform.localEulerAngles = Vector3.zero;
                }
            }

            if(curLayer == ViewLayer.ViewLayer_0 || curLayer == ViewLayer.ViewLayer_1)
            {
                if(m_SymbolLoader != null)
                {
                    m_SymbolLoader.asset.SetActive(false);
                }
                if(m_ActorLoader != null)
                {
                    m_ActorLoader.asset.SetActive(true);
                }
            }
            else if(curLayer == ViewLayer.ViewLayer_2)
            {
                if(m_SymbolLoader != null)
                {
                    m_SymbolLoader.asset.SetActive(true);
                }
                if(m_ActorLoader != null)
                {
                    m_ActorLoader.asset.SetActive(false);
                }
            }
            else
            {
                if(m_SymbolLoader != null)
                {
                    m_SymbolLoader.asset.SetActive(false);
                }
                if(m_ActorLoader != null)
                {
                    m_ActorLoader.asset.SetActive(false);
                }                
            }
        }

        public void OnLeave(ViewLayer curLayer, ViewLayer nextLayer)
        {
            // 0层与1层之间切换不做替换
            // if(curLayer == ViewLayer.ViewLayer_0 || nextLayer == ViewLayer.ViewLayer_1)
            //     return;
            // if(curLayer == ViewLayer.ViewLayer_1 || nextLayer == ViewLayer.ViewLayer_0)
            //     return;

            // // if(display.transform.childCount > 0)
            // // {
            // //     display.transform.GetChild(0).gameObject.SetActive(false);
            // // }

            // if(nextLayer == ViewLayer.ViewLayer_2)
            // { // 退出0层或1层
            //     if(m_ActorLoader != null)
            //     {
            //         m_ActorLoader.asset.SetActive(false);
            //     }
            // }
        }

        public IEnumerator LoadActorPrefabAsync()
        {
            m_LoaderAsync = ResourceManager.InstantiateAsync(assetPath);
            yield return m_LoaderAsync;
        }

        private void OnActorPrefabLoadFinished()
        {

        }
    }
}