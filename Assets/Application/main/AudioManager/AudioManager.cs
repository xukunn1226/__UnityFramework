using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using Framework.Core;
using Framework.AssetManagement.Runtime;
using System.IO;

namespace Application.Runtime
{
    public class AudioManager : SingletonMono<AudioManager>
    {
        private List<string> m_BanksList = new List<string>();

        IEnumerator Start()
        {
            LoadAllBanks();
            yield break;
        }

        protected override void OnDestroy()
        {
            UnloadAllBanks();

            RuntimeManager[] manager = Resources.FindObjectsOfTypeAll<RuntimeManager>();
            foreach(var mgr in manager)
            {
                Destroy(mgr.gameObject);
            }
            base.OnDestroy();
        }

        private void LoadAllBanks()
        {
            string bankPath = GetBankFolder();
#if LOAD_FROM_PERSISTENT            // 因为资源已提取，可以使用System.IO.Directory
            string[] files = Directory.GetFiles(bankPath, "*.bank");
            foreach(var file in files)
            {
                string bankName = Path.GetFileNameWithoutExtension(file);
                RuntimeManager.LoadBank(bankName, bankPath);

                m_BanksList.Add(bankName);
            }
#else   // 资源未提取，使用Settings数据（这里记录了所有的bank）
            foreach (string masterBankFileName in Settings.Instance.MasterBanks)
            {
                RuntimeManager.LoadBank(masterBankFileName + ".strings", bankPath);
                RuntimeManager.LoadBank(masterBankFileName, bankPath);

                m_BanksList.Add(masterBankFileName + ".strings");
                m_BanksList.Add(masterBankFileName);
            }
            foreach(var bank in Settings.Instance.Banks)
            {
                RuntimeManager.LoadBank(bank, bankPath);

                m_BanksList.Add(bank);
            }
#endif

            RuntimeManager.WaitForAllLoads();
        }

        private void UnloadAllBanks()
        {
            foreach(var bank in m_BanksList)
            {
                RuntimeManager.UnloadBank(bank);
            }
            m_BanksList.Clear();
        }

        // FromEditor: 从Assets/FMODAssets读取数据
        // FromStreamingAssets: 从Application.streamingAssetsPath读取数据，真机环境加前缀"file:///android_asset"
        // FromPersistent：从Application.persistentDataPath读取数据
        private string GetBankFolder()
        {
            string bankFolder = string.Format("{0}/{1}/{2}", UnityEngine.Application.streamingAssetsPath, Utility.GetPlatformName(), Settings.Instance.TargetSubFolder);

            LoaderType type = Launcher.GetLauncherMode();
            switch(type)
            {
                case LoaderType.FromEditor:
                    {
#if UNITY_EDITOR
                        // Use original asset location because streaming asset folder will contain platform specific banks
                        Settings globalSettings = Settings.Instance;

                        bankFolder = globalSettings.SourceBankPath;
                        if (globalSettings.HasPlatforms)
                        {
                            Platform platform = Settings.Instance.CurrentEditorPlatform;

                            if (platform == Settings.Instance.DefaultPlatform)
                            {
                                platform = Settings.Instance.PlayInEditorPlatform;
                            }
                            bankFolder = RuntimeUtils.GetCommonPlatformPath(System.IO.Path.Combine(bankFolder, platform.BuildDirectory));
                        }
#endif
                    }
                    break;
                case LoaderType.FromStreamingAssets:
                    {
#if UNITY_ANDROID && !UNITY_EDITOR
                        bankFolder = string.Format("{0}/{1}/{2}", "file:///android_asset", Utility.GetPlatformName(), Settings.Instance.TargetSubFolder);
#endif
                    }
                    break;
                case LoaderType.FromPersistent:
                    {
                        bankFolder = string.Format("{0}/{1}/{2}", UnityEngine.Application.persistentDataPath, Utility.GetPlatformName(), Settings.Instance.TargetSubFolder);
                    }
                    break;
            }

            return bankFolder;
        }

        void Update()
        {
            if(m_ElapsedTime >= 0)
            {
                m_ElapsedTime -= Time.deltaTime;
                if(m_ElapsedTime < 0)
                {
                    m_ElapsedTime = -1;
                    m_BGM = Play2D(m_PendingBGM, false);
                }                
            }
        }

        static private EventReference               m_PendingBGM;
        static private FMOD.Studio.EventInstance    m_BGM;
        static private float                        m_ElapsedTime = -1;

        static public void PlayBGM(string path, float delay = 0)
        {
            PlayBGM(RuntimeManager.PathToEventReference(path), delay);
        }
        
        static public void PlayBGM(EventReference eventReference, float delay = 0)
        {
            if(m_BGM.isValid())
            {
                m_BGM.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                m_BGM.release();
                m_BGM.clearHandle();
            }
            m_ElapsedTime = Mathf.Max(0, delay);
            m_PendingBGM = eventReference;
        }

        static public void StopBGM()
        {
            if(m_BGM.isValid())
            {
                m_BGM.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                m_BGM.release();
                m_BGM.clearHandle();
            }
        }

        static public void PlayOneShot(EventReference eventReference, Vector3 position = new Vector3())
        {
            PlayOneShot(eventReference.Guid, position);
        }

        static public void PlayOneShot(string path, Vector3 position = new Vector3())
        {
            PlayOneShot(RuntimeManager.PathToGUID(path), position);
        }

        static public void PlayOneShot(FMOD.GUID guid, Vector3 position = new Vector3())
        {
            RuntimeManager.PlayOneShot(guid, position);
        }

        static public void PlayOneShotAttached(EventReference eventReference, GameObject gameObject)
        {
            PlayOneShotAttached(eventReference.Guid, gameObject);
        }
        
        static public void PlayOneShotAttached(string path, GameObject gameObject)
        {
            PlayOneShotAttached(RuntimeManager.PathToGUID(path), gameObject);
        }

        static public void PlayOneShotAttached(FMOD.GUID guid, GameObject gameObject)
        {
            RuntimeManager.PlayOneShotAttached(guid, gameObject);
        }

        static public FMOD.Studio.EventInstance Play2D(EventReference eventReference, bool isOneShot = true)
        {
            return Play2D(eventReference.Guid, isOneShot);            
        }

        static public FMOD.Studio.EventInstance Play2D(string path, bool isOneShot = true)
        {
            return Play2D(RuntimeManager.PathToGUID(path), isOneShot);
        }

        // 2D音效播放接口，isOneShot=true表示音效播放完毕自动回收，也意味着播放完毕前失去了控制，不可停止，不可重播
        static public FMOD.Studio.EventInstance Play2D(FMOD.GUID guid, bool isOneShot = true)
        {
            FMOD.Studio.EventDescription desc = RuntimeManager.GetEventDescription(guid);
            bool isOneShotOfMeta;
            desc.isOneshot(out isOneShotOfMeta);
            // 循环音效被设置成isOneShot会导致handle失效，无法stop
            if(!isOneShotOfMeta && isOneShot)
            {
                Debug.LogWarning("设置循环音效为isOneShot，需要手动释放");
                isOneShot = false;
            }

            var instance = RuntimeManager.CreateInstance(guid);
            instance.start();
            if(isOneShot)
            {
                instance.release();
                instance.clearHandle();
            }
            return instance;
        }

        static public void Stop(FMOD.Studio.EventInstance instance, bool AllowFadeout = true)
        {
            if(instance.isValid())
            {
                instance.stop(AllowFadeout ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
                instance.release();
                instance.clearHandle();
            }
        }

        static public void Restart(FMOD.Studio.EventInstance instance, bool AllowFadeout = true)
        {
            if(instance.isValid())
            {
                instance.stop(AllowFadeout ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
                instance.start();
            }
        }
    }
}