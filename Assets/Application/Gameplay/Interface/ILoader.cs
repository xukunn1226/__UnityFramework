using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Application.Runtime
{
    /// <summary>
    /// 实现资源异步加载请求，统一由AssetLoadingAsyncManager负责
    /// <summary>
    public interface ILoader
    {
        int         loaderId { get; set; }
        // IEnumerator LoadAsync(Action onLoadFinished);
    }
}