using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Application.Runtime;

namespace Application.Logic
{
    public class ViewLayerComponent : ZComp
    {
        public delegate void onViewUpdateHandler(ViewLayer layer, float alpha);
        public delegate void onEnterHandler(ViewLayer prevLayer, ViewLayer curLayer);
        public delegate void onLeaveHandler(ViewLayer curLayer, ViewLayer nextLayer);

        public event onViewUpdateHandler onViewUpdate;
        public event onEnterHandler onEnter;
        public event onLeaveHandler onLeave;
        
        public ViewLayer   minViewLayer     { get; set; }
        public ViewLayer   maxViewLayer     { get; set; }
        public int         id               { get; private set; }

        public ViewLayerComponent(ZActor actor) : base(actor) {}

        public override void Start()
        {
            base.Start();
            id = ViewLayerManager.Instance.AddInstance(this);
        }

        public override void Destroy()
        {
            ViewLayerManager.Instance.RemoveInstance(this);
            base.Destroy();
        }

        /// <summary>
        /// 对象处于[minViewLayer, maxViewLayer]时的轮询函数
        /// layer: 当前层级
        /// alpha: 0表示处于当前层级最低处，1表示处于最高处；
        /// <summary>
        public void OnViewUpdate(ViewLayer layer, float alpha)
        {
            onViewUpdate?.Invoke(layer, alpha);
        }

        public void OnEnter(ViewLayer prevLayer, ViewLayer curLayer)
        {
            onEnter?.Invoke(prevLayer, curLayer);
        }

        public void OnLeave(ViewLayer curLayer, ViewLayer nextLayer)
        {
            onLeave?.Invoke(curLayer, nextLayer);
        }
    }
}