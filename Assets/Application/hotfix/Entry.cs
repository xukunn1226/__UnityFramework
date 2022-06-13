using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Application.Runtime;

namespace Application.Logic
{
    static public class Entry
    {
        static public void Start()
        {
            // Debug.Log("Entry.Start======");
            // await System.Threading.Tasks.Task.Delay(3000);
            Debug.Log("Entry.Start======");
            Prepare().Start().OnCompleted.AddListener(OnCompletedPrepare);
        }

        static public void OnDestroy()
        {
            SingletonBase.DestroyAll();
            CodeLoader.Instance.Update              -= Update;
            CodeLoader.Instance.OnApplicationQuit   -= OnApplicationQuit;
            CodeLoader.Instance.OnApplicationFocus  -= OnApplicationFocus;
            CodeLoader.Instance.OnDestroy           -= OnDestroy;
        }

        static public void Update()
        {
            SingletonBase.Update(Time.deltaTime);
        }

        static public void OnApplicationFocus(bool isFocus)
        {}

        static public void OnApplicationQuit()
        {}

        static System.Collections.IEnumerator Prepare()
        {
            if(Launcher.GetLauncherMode() == Framework.AssetManagement.Runtime.LoaderType.FromStreamingAssets)
            { // 仅FromStreamingAssets时需要提取db，FromEditor从本地读取，FromPersistent会首次启动时提取
                yield return ConfigManager.ExtractDatabase();
            }
        }

        static private void OnCompletedPrepare(bool stopped)
        {
            CodeLoader.Instance.Update              += Update;
            CodeLoader.Instance.OnApplicationQuit   += OnApplicationQuit;
            CodeLoader.Instance.OnApplicationFocus  += OnApplicationFocus;
            CodeLoader.Instance.OnDestroy           += OnDestroy;

            GameSettings.Deserialize();
            GameSettings.SetGameSettingsLevel(5);

            UIManager.Instance.Init();

            GameModeManager.Instance.SwitchTo(GameState.World);

            // ConfigManagerDemo.Start();
        }
    }
}