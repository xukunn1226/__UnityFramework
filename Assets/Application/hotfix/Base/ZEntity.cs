using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Application.Runtime
{
    /// <summary>
    /// ZEntity、ZComp、ZActor框架设计目的
    /// 1、逻辑与显示分离
    /// 2、自定义事件函数
    /// <summary>
    public class ZEntity
    {
        public string name { get; set; }
        public virtual void Prepare(IDataSource data) {}    // prepare data
        public virtual void Start() {}                      // start work
        public virtual void Destroy() {}
    }

    public interface IDataSource
    {}
}