using UnityEngine;

namespace Framework.LevelManager.Tests
{
    public class TestLevelManager : MonoBehaviour
    {
        private const string m_SceneBundlePath = "assets/framework/core/levelmanager/tests/res/scenes.ab";
        
        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            LevelManager.levelCommandBegin += LevelCommandBegin;
            LevelManager.levelCommandEnd += LevelCommandEnd;

            // LevelManager.LoadLevelContext ctx = new LevelManager.LoadLevelContext();
            // ctx.identifier = "tscene1";
            // ctx.fromBundle = true;
            // ctx.levelPath = "Assets/Framework/LevelManager/Tests/Res/Scenes/TestScene1.unity";
            // ctx.additive = true;
            // ctx.bundlePath = m_SceneBundlePath;

            // LevelManager.Instance.LoadAsync(ctx);
        }

        private void LevelCommandBegin(string identifier, bool isLoaded)
        {
            Debug.Log($"LevelCommandBegin: {Time.frameCount}    {identifier}    {isLoaded}");
        }
        
        private void LevelCommandEnd(string identifier, bool isLoaded)
        {
            Debug.Log($"LevelCommandEnd: {Time.frameCount}    {identifier}    {isLoaded}");
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 300, 120), "Load Scene2"))
            {
                LevelManager.LevelContext ctx = new LevelManager.LevelContext();
                ctx.identifier = "tscene2";
                ctx.scenePath = "Assets/Framework/Core/LevelManager/Tests/Res/Scenes/TestScene2.unity";
                // ctx.levelPath = "TestScene2";
                ctx.additive = true;
                ctx.bundlePath = m_SceneBundlePath;

                LevelManager.Instance.LoadAsync(ctx);
            }

            if (GUI.Button(new Rect(500, 100, 300, 120), "Unload Scene2"))
            {
                LevelManager.Instance.UnloadLevelAsync("tscene2");
            }

            if (GUI.Button(new Rect(100, 300, 300, 120), "Load Scene3"))
            {
                LevelManager.LevelContext ctx = new LevelManager.LevelContext();
                ctx.identifier = "tscene3";
                ctx.scenePath = "Assets/Framework/Core/LevelManager/Tests/Res/Scenes/TestScene3.unity";
                ctx.additive = false;
                ctx.bundlePath = m_SceneBundlePath;

                LevelManager.Instance.LoadAsync(ctx);
            }

            if (GUI.Button(new Rect(500, 300, 300, 120), "Unload Scene3"))
            {
                LevelManager.Instance.UnloadLevelAsync("tscene3");
            }
        }
    }
}