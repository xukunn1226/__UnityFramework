using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

// QualitySettings.vSyncCount
// = 0，不等待垂直同步；
// > 0，则将忽略Application.targetFrameRate的值，将使用 vSyncCount 和平台的默认渲染率来确定目标帧率，如FPS = Application.targetFrameRate / QualitySettings.vSyncCount
// QualitySettings.vSyncCount = 1，Sync framerate to monitors refresh rate

// Application.targetFrameRate = -1，表示以平台默认帧率渲染

public class Example : MonoBehaviour
{    
    // 设置渲染帧率，假设已设置QualitySettings.vSyncCount、Application.targetFrameRate
    public void SetRenderFrameRate(int targetFrameRate)
    {
        if(QualitySettings.vSyncCount > 0)
        {
            OnDemandRendering.renderFrameInterval = (Screen.currentResolution.refreshRate / QualitySettings.vSyncCount / targetFrameRate);
        }
        else
        {
            OnDemandRendering.renderFrameInterval = (UnityEngine.Application.targetFrameRate / targetFrameRate);
        }
    }

    // 恢复渲染帧率（每帧都渲染）
    public void RestoreRenderFrameRate()
    {
        OnDemandRendering.renderFrameInterval = 1;
    }

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        UnityEngine.Application.targetFrameRate = 120;
        OnDemandRendering.renderFrameInterval = 3;
    }

    void Update()
    {
        // Debug.Log("Will this frame render? " + OnDemandRendering.willCurrentFrameRender);

        if (!OnDemandRendering.willCurrentFrameRender)
        {
            // Frames that are not rendered may have some extra CPU cycles to spare for processes that would otherwise be too much of a burden.
            // For example: expensive math operations, spawning prefabs or loading assets.
            Debug.Log($"------- {Time.frameCount}");
        }
    }
}