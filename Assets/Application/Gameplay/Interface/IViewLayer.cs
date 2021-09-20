using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public interface IViewLayer
    {
        ViewLayer   minViewLayer    { get; set; }
        ViewLayer   maxViewLayer    { get; set; }
        bool        visible         { get; set; }
        int         viewId          { get; set; }

        /// <summary>
        /// 对象处于[minViewLayer, maxViewLayer]时的轮询函数
        /// layer: 当前层级
        /// alpha: 0表示处于当前层级最低处，1表示处于最高处；
        /// <summary>
        void OnViewUpdate(ViewLayer layer, float alpha);
        void OnEnter(ViewLayer prevLayer, ViewLayer curLayer);
        void OnLeave(ViewLayer curLayer, ViewLayer nextLayer);
    }    

    // 视野层级
    public enum ViewLayer
    {
        ViewLayer_Invalid = -1,
        ViewLayer_0,
        ViewLayer_1,
        ViewLayer_2,
        ViewLayer_3,
        ViewLayer_Max,
    }
}