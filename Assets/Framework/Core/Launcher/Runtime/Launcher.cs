using System;
using UnityEngine;

namespace Framework.Core
{
    /// <summary>
    /// 负责游戏启动（obb下载、补丁、资源提取、修复、启动画面显示、隐藏，多语言，网络状态提示等等）
    /// </summary>
    public class Launcher : MonoBehaviour
    {
        public VersionManager   versionManager;
        new public Camera       camera;
        public Canvas           canvas;

        static public Launcher Instance
        {
            get; private set;
        }

        private void Awake()
        {
            if (FindObjectsOfType<Launcher>().Length > 1)
            {
                DestroyImmediate(this);
                throw new Exception("Launcher has already exist...");
            }

            gameObject.name = "[Launcher]";

            Instance = this;

            DontDestroyOnLoad(gameObject);

            if(versionManager == null)
                throw new ArgumentNullException("versionManager");

            if(camera == null)
                throw new ArgumentNullException("camera");

            if(canvas == null)
                throw new ArgumentNullException("canvas");

            versionManager.OnVersionControlFinished += OnVersionControlFinished;
        }

        private void Start()
        {
            StartWork();
        }

        public void StartWork()
        {
            canvas.enabled = true;
            versionManager.StartWork();
        }

        private void OnDestroy()
        {
            versionManager.OnVersionControlFinished -= OnVersionControlFinished;
            Instance = null;
        }

        private void OnVersionControlFinished(string error)
        {
            canvas.enabled = false;
        }

        static public bool IsWifi()
        {
            return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
        }

        // 网络链接是否断开
        static public bool IsNetworkReachability()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
    }
}