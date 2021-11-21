using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using Framework.Core;
using Framework.AssetManagement.Runtime;

namespace Application.Runtime
{
    public class AudioManager : SingletonMono<AudioManager>
    {
        protected override void Awake()
        {
            base.Awake();

            string bankPath = GetBankFolder();

            //foreach (string masterBankFileName in Settings.Instance.MasterBanks)
            //{
            //    RuntimeManager.LoadBank(masterBankFileName, bankPath);
            //    RuntimeManager.LoadBank(masterBankFileName + ".strings", bankPath);
            //}

            //foreach (var bank in Settings.Instance.Banks)
            //{
            //    RuntimeManager.LoadBank(bank, bankPath);
            //}

            string[] files = System.IO.Directory.GetFiles(bankPath, "*.bank");
            foreach(var file in files)
            {
                RuntimeManager.LoadBank(System.IO.Path.GetFileNameWithoutExtension(file), bankPath);
            }
        }

        private string GetBankFolder()
        {
            // FromPersistent and FromStreamingAssets(mobile android)
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
            }

            return bankFolder;
        }
    }
}