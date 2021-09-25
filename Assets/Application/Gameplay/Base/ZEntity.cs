using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Application.Runtime
{
    /// <summary>
    /// ZEntity、ZComp、ZActor框架设计目的
    /// 1、逻辑与显示分离
    /// <summary>
    public class ZEntity
    {
        public string name { get; set; }
        public virtual void InitData() {}
        public virtual void Start() {}
        public virtual void Destroy() {}
    }
}