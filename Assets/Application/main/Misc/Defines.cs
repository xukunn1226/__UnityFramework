using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public enum LoaderType
    {
        FromEditor,                 // 编辑器模式使用，内部使用AssetDatabase加载资源
        FromStreamingAssets,        // 从StreamingAssetsPath下加载资源，运行时和编辑器时均可使用
        FromPersistent,             // 从PersistentDataPath下加载资源，启动时需把资源从streamingAssets解压至persistentData
    }
}