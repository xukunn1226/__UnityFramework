using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    /// <summary>
    /// ZComp挂载至ZActor，派生类不建议生成无参数构造函数
    /// <summary>
    public class ZComp : ZEntity
    {
        public ZActor actor { get; }

        protected ZComp(ZActor actor)
        {
            Debug.Assert(actor != null);
            this.actor = actor;
        }
    }
}