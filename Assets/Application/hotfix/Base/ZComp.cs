using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.HotFix
{
    /// <summary>
    /// 组件基类
    /// ZComp必须依附于ZActor，强烈建议不直接生成，而使用ZActor.AddComponent
    /// <summary>
    public class ZComp : ZEntity
    {
        public ZActor actor { get; }

        public ZComp(ZActor actor)
        {
            Debug.Assert(actor != null);
            this.actor = actor;
        }
    }
}