using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Application.Logic
{
    static public class GameSetting
    {
        static public void Init()
        {
            QualitySettings.vSyncCount = 0;
            UnityEngine.Application.targetFrameRate = 300;
        }

        // 设置渲染帧率，假设已设置QualitySettings.vSyncCount、Application.targetFrameRate
        static public void SetRenderFrameRate(int targetFrameRate)
        {
            if (QualitySettings.vSyncCount > 0)
            {
                OnDemandRendering.renderFrameInterval = (Screen.currentResolution.refreshRate / QualitySettings.vSyncCount / targetFrameRate);
            }
            else
            {
                OnDemandRendering.renderFrameInterval = (UnityEngine.Application.targetFrameRate / targetFrameRate);
            }
        }

        // 恢复渲染帧率（每帧都渲染）
        static public void RestoreRenderFrameRate()
        {
            OnDemandRendering.renderFrameInterval = 1;
        }
    }
}