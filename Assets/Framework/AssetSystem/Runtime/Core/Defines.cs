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
            /// �༭����ģ��ģʽ
            /// </summary>
            FromEditor,

            /// <summary>
            /// ����ģʽ
            /// </summary>
            FromStreamingAsset,

            /// <summary>
            /// ����ģʽ
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