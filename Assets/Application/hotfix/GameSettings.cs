using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Framework.AssetManagement.Runtime;

namespace Application.Logic
{
    public class GameSettings
    {
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


        static private int MAX_LEVEL = 6;           // 等级数量同Quality Level
        static private int s_CurIndex = -1;
        static private int s_PrevIndex = -1;
        static private GameSettings[] s_GameSettings = new GameSettings[MAX_LEVEL];

        public delegate void ChangeSettingCallback(int curIndex, int prevIndex);
        static event ChangeSettingCallback onChangeGameSetting;

        static public void SetGameSettingsLevel(int index)
        {
            index = Mathf.Clamp(index, 0, MAX_LEVEL - 1);
            if (index == s_CurIndex)
                return;

            SetGlobalSettings();

            s_PrevIndex = s_CurIndex;
            s_CurIndex = index;
            InternalChangeSettings(s_CurIndex, s_PrevIndex);
        }

        static public int GetGameSettingsLevel()
        {
            return s_CurIndex;
        }

        static GameSettings GetGameSettings()
        {
            return s_GameSettings[s_CurIndex];
        }

        static public void IncreaseLevel()
        {
            if (s_CurIndex + 1 >= MAX_LEVEL)
                return;
            SetGameSettingsLevel(s_CurIndex + 1);
        }

        static public void DecreaseLevel()
        {
            if (s_CurIndex <= 0)
                return;
            SetGameSettingsLevel(s_CurIndex - 1);
        }

        static private void SetGlobalSettings()
        {
            QualitySettings.vSyncCount = 0;
            UnityEngine.Application.targetFrameRate = 300;
        }

        static private void InternalChangeSettings(int curIndex, int prevIndex)
        {
            QualitySettings.SetQualityLevel(curIndex);
            QualitySettings.renderPipeline = AssetManager.LoadAsset<RenderPipelineAsset>(s_GameSettings[curIndex].m_RenderPipelineAsset).asset;

            onChangeGameSetting?.Invoke(s_CurIndex, s_PrevIndex);
        }

        static public void Serialize(string assetPath, GameSettings settings)
        {

        }

        static public void Deserialize()
        {
            s_GameSettings[0] = new GameSettings()
            {
                m_RenderPipelineAsset = "assets/settings/universalrp-lowquality.asset"
            };

            s_GameSettings[1] = new GameSettings()
            {
                m_RenderPipelineAsset = "assets/settings/universalrp-lowquality.asset"
            };

            s_GameSettings[2] = new GameSettings()
            {
                m_RenderPipelineAsset = "assets/settings/universalrp-mediumquality.asset"
            };

            s_GameSettings[3] = new GameSettings()
            {
                m_RenderPipelineAsset = "assets/settings/universalrp-mediumquality.asset"
            };

            s_GameSettings[4] = new GameSettings()
            {
                m_RenderPipelineAsset = "assets/settings/universalrp-highquality.asset"
            };

            s_GameSettings[5] = new GameSettings()
            {
                m_RenderPipelineAsset = "assets/settings/universalrp-highquality.asset"
            };
        }

        public string m_RenderPipelineAsset = "assets/settings/universalrp-highquality.asset";
    }
}