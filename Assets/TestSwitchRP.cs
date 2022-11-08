using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Framework.AssetManagement.Runtime;
using Framework.Core;
using UnityEngine.Rendering.Universal;

public class TestSwitchRP : MonoBehaviour
{
    void OnGUI()
    {
        GUI.Button(new Rect(20, 20, 100, 40), $"{QualitySettings.GetQualityLevel()}");

        if(GUI.Button(new Rect(20, 100, 100, 40), "Level 1"))
        {
            Application.Runtime.GameSettings.SetGameSettingsLevel(0);
        }

        if (GUI.Button(new Rect(20, 150, 100, 40), "Level 2"))
        {
            Application.Runtime.GameSettings.SetGameSettingsLevel(1);
        }

        if (GUI.Button(new Rect(20, 200, 100, 40), "Level 3"))
        {
            Application.Runtime.GameSettings.SetGameSettingsLevel(2);
        }

        if (GUI.Button(new Rect(20, 250, 100, 40), "Level 4"))
        {
            Application.Runtime.GameSettings.SetGameSettingsLevel(3);
        }

        if (GUI.Button(new Rect(20, 300, 100, 40), "Level 5"))
        {
            Application.Runtime.GameSettings.SetGameSettingsLevel(4);
        }

        if (GUI.Button(new Rect(20, 350, 100, 40), "Level 6"))
        {
            Application.Runtime.GameSettings.SetGameSettingsLevel(5);
        }
    }
}

namespace Application.Runtime
{
    public class GameSettings
    {
        static private int MAX_LEVEL = 6;           // 等级数量同Quality Level
        static private int s_CurIndex = -1;
        static private int s_PrevIndex = -1;
        static public GameSettings[] s_GameSettings = new GameSettings[MAX_LEVEL];
        static private bool s_Init;
        static public bool Inited => s_Init;

        public delegate void ChangeSettingCallback(int curIndex, int prevIndex);
        static event ChangeSettingCallback onChangeGameSetting;

        static public void SetGameSettingsLevel(int index)
        {
            index = Mathf.Clamp(index, 0, MAX_LEVEL - 1);
            if (index == s_CurIndex)
                return;

            Init();

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
            UnityEngine.Application.targetFrameRate = 80;
            //切换到android平台时，editor失去焦点后，就不render了
            UnityEngine.Application.runInBackground = true;
            QualitySettings.vSyncCount = 0;
        }

        static AssetLoader<RenderPipelineAsset> sCurRPLoader = null;

        static private void InternalChangeSettings(int curIndex, int prevIndex)
        {
            // switching quality runtime may lead to leak
            //QualitySettings.SetQualityLevel(curIndex);

            //var rpLoader = AssetManager.LoadAsset<RenderPipelineAsset>(s_GameSettings[curIndex].m_RenderPipelineAsset);
            //var rp = UniversalRenderPipelineAsset.Instantiate(rpLoader.asset);
            ////var rp = rpLoader.asset;
            //var lastRp = GraphicsSettings.defaultRenderPipeline;
            //GraphicsSettings.defaultRenderPipeline = rp;

            //if (sCurRPLoader != null)
            //{
            //    AssetManager.UnloadAsset(sCurRPLoader);
            //}
            //sCurRPLoader = rpLoader;

            
            QualitySettings.SetQualityLevel(curIndex);

            // load new RP Asset
            RenderPipelineAsset prevAsset = QualitySettings.renderPipeline;
            AssetLoader<RenderPipelineAsset> loader = AssetManager.LoadAsset<RenderPipelineAsset>(s_GameSettings[curIndex].m_RenderPipelineAsset);            
            QualitySettings.renderPipeline = loader.asset;

            // unload prev RP Asset
            if (prevAsset != null)
                Resources.UnloadAsset(prevAsset);
            if (sCurRPLoader != null)
                AssetManager.UnloadAsset(sCurRPLoader);

            sCurRPLoader = loader;

            onChangeGameSetting?.Invoke(s_CurIndex, s_PrevIndex);
        }

        static private void Init()
        {
            if (s_Init)
            {
                return;
            }

            s_Init = true;

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