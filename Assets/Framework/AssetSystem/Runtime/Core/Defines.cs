using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    static public class Defines
    {
        public enum EPlayMode
        {
            /// <summary>
            /// 编辑器下模拟模式
            /// </summary>
            FromEditor,

            /// <summary>
            /// 单机模式
            /// </summary>
            FromStreamingAsset,

            /// <summary>
            /// 联机模式
            /// </summary>
            FromHost,
        }

        public enum EPackageLoadMethod
        {
            None,
            FromStreaming,
            FromCache,
            FromEditor,
        }

        public enum ELoadStatus
        {
            None = 0,
            Succeed = 1,
            Failed = 2,
        }

        //public enum EB
    }
}