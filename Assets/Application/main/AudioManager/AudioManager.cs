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
        protected override void Awake()
        {
            base.Awake();

            LoadAllBanks();
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
            string[] files = Directory.GetFiles(bankPath, "*.bank");
            foreach(var file in files)
            {
                RuntimeManager.LoadBank(Path.GetFileNameWithoutExtension(file), bankPath);
            }

            RuntimeManager.WaitForAllLoads();
        }

        private void UnloadAllBanks()
        {
            string bankPath = GetBankFolder();
            string[] files = Directory.GetFiles(bankPath, "*.bank");
            foreach(var file in files)
            {
                RuntimeManager.UnloadBank(Path.GetFileNameWithoutExtension(file));
            }
        }

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

        static public void PlayOneShot(EventReference eventReference, Vector3 position = new Vector3())
        {
            PlayOneShot(eventReference.Guid, position);
        }

        static public void PlayOneShot(string path, Vector3 position = new Vector3())
        {
            PlayOneShot(RuntimeManager.PathToGUID(path), position);
        }

        // 播放3D非循环音效
        static public void PlayOneShot(FMOD.GUID guid, Vector3 position = new Vector3())
        {
            #if UNITY_EDITOR
            FMOD.Studio.EventDescription desc = RuntimeManager.GetEventDescription(guid);
            bool is3D;
            desc.is3D(out is3D);
            bool isOneShot;
            desc.isOneshot(out isOneShot);
            if(!is3D || !isOneShot)
                throw new System.Exception($"PlayOneShot exception: is3D: {is3D}    isOneShot: {isOneShot}");
            #endif

            RuntimeManager.PlayOneShot(guid, position);
        }

        // 播放3D音效，并依附于gameObject，音效播放完毕或对象删除内部才删除
        static public void PlayOneShotAttached(EventReference eventReference, GameObject gameObject)
        {
            RuntimeManager.PlayOneShotAttached(eventReference.Guid, gameObject);
        }

        static public FMOD.Studio.EventInstance Play2D(EventReference eventReference, bool release = true)
        {
            return Play2D(eventReference.Guid, release);            
        }

        static public FMOD.Studio.EventInstance Play2D(string path, bool release = true)
        {
            return Play2D(RuntimeManager.PathToGUID(path), release);
        }

        static public FMOD.Studio.EventInstance Play2D(FMOD.GUID guid, bool release = true)
        {
            var instance = RuntimeManager.CreateInstance(guid);
            instance.start();
            if(release)
            {
                instance.release();
                instance.clearHandle();
            }
            return instance;
        }

        static public void Stop2D(FMOD.Studio.EventInstance instance, bool AllowFadeout = true)
        {
            if(instance.isValid())
            {
                instance.stop(AllowFadeout ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
                instance.release();
                instance.clearHandle();
            }
        }

        static public void Restart2D(FMOD.Studio.EventInstance instance)
        {
            if(instance.isValid())
            {
                instance.start();
            }
        }
    }
}