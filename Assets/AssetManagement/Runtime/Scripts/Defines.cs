using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetManagement.Runtime
{
    public enum LoaderType
    {
        FromEditor,         // 编辑器模式使用，内部使用AssetDatabase加载资源
        FromAB,             // 运行时和编辑器时均可使用
    }
}