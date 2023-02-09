using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    static public partial class AssetManagerEx
    {
        static private bool s_Init;
        static private GameObject s_Driver;
        static private AssetSystem s_AssetSystem;
        static EOperationStatus s_OperationStatus;
        static string s_Error;
        static InitializationOperation m_InitOp;

        static public InitializationOperation Initialize(InitializeParameters para)
        {
            // 允许重复初始化
            if (s_Init)
            {
                return m_InitOp;
            }

            s_Init = true;
            s_Driver = new GameObject($"AssetManager");
            s_Driver.AddComponent<AssetDriver>();
            UnityEngine.Object.DontDestroyOnLoad(s_Driver);
            Debug.Log("AssetManager initialize!");

            AsyncOperationSystem.Initialize();
            s_AssetSystem = new AssetSystem();

            m_InitOp = s_AssetSystem.InitializeAsync(para);
            m_InitOp.Completed += InitializeOperation_Completed;

            return m_InitOp;
        }

        static private void InitializeOperation_Completed(AsyncOperationBase op)
        {
            s_OperationStatus = op.status;
            s_Error = op.lastError;
        }

        static public void Destroy()
        {
            if (!s_Init)
                return;

            s_Init = false;

            // destroy other system
            AsyncOperationSystem.Destroy();
            DownloadSystem.DestroyAll();
            s_AssetSystem?.Destroy();

            if (s_Driver != null)
                UnityEngine.Object.Destroy(s_Driver);
            Debug.Log("AssetManagement destroy!");
        }

        static public void Update()
        {
            if (!s_Init)
                return;

            AsyncOperationSystem.Update();
            DownloadSystem.Update();
            s_AssetSystem?.Update();
        }

        static public DebugReport GetDebugReport()
        {
            if (!s_Init)
                return null;

            DebugReport report = new DebugReport();
            report.FrameCount = Time.frameCount;
            report.DebugProviderInfos = s_AssetSystem.GetDebugProviderInfos();
            return report;
        }
    }
}