using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public interface IViewActor
    {
        ViewLayer   minViewLayer    { get; set; }
        ViewLayer   maxViewLayer    { get; set; }
        bool        visible         { get; set; }
        int         id              { get; set; }

        /// <summary>
        /// 处于[minViewLayer, maxViewLayer]时的轮询函数
        /// layer: 当前层级
        /// alpha: 0表示处于当前层级最低处，1表示处于最高处；
        /// <summary>
        void OnViewUpdate(ViewLayer layer, float alpha);
        void OnEnter(ViewLayer prevLayer);
        void OnLeave(ViewLayer nextLayer);
    }

    public class Actor : IViewActor
    {
        public ViewLayer   minViewLayer    { get; set; }
        public ViewLayer   maxViewLayer    { get; set; }
        public bool        visible         { get; set; }
        public int         id              { get; set; }
        public void OnViewUpdate(ViewLayer layer, float alpha) {}
        public void OnEnter(ViewLayer prevLayer) {}
        public void OnLeave(ViewLayer nextLayer) {}
    }
}