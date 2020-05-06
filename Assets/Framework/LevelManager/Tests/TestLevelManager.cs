using UnityEngine;
using Framework.LevelManager;

namespace Tests
{
    public class TestLevelManager : MonoBehaviour
    {
        private const string m_SceneBundlePath = "assets/framework/levelmanager/tests/res/scenes.ab";
        
        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            LevelManager.levelCommandBegin += LevelCommandBegin;
            LevelManager.levelCommandEnd += LevelCommandEnd;

            LevelManager.LoadLevelContext ctx = new LevelManager.LoadLevelContext();
            ctx.identifier = "tscene1";
            ctx.fromBundle = true;
            ctx.levelPath = "Assets/Framework/LevelManager/Tests/Res/Scenes/TestScene1.unity";
            ctx.additive = true;
            ctx.bundlePath = m_SceneBundlePath;

            LevelManager.Instance.LoadAsync(ctx);
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
            if (GUI.Button(new Rect(100, 100, 150, 60), "Load Scene2"))
            {
                LevelManager.LoadLevelContext ctx = new LevelManager.LoadLevelContext();
                ctx.identifier = "tscene2";
                ctx.fromBundle = true;
                ctx.levelPath = "Assets/Framework/LevelManager/Tests/Res/Scenes/TestScene2.unity";
                ctx.additive = true;
                ctx.bundlePath = m_SceneBundlePath;

                LevelManager.Instance.LoadAsync(ctx);
            }

            if (GUI.Button(new Rect(300, 100, 150, 60), "Unload Scene2"))
            {
                LevelManager.Instance.UnloadLevelAsync("tscene2");
            }

            if (GUI.Button(new Rect(100, 200, 150, 60), "Load Scene3"))
            {
                LevelManager.LoadLevelContext ctx = new LevelManager.LoadLevelContext();
                ctx.identifier = "tscene3";
                ctx.fromBundle = true;
                ctx.levelPath = "Assets/Framework/LevelManager/Tests/Res/Scenes/TestScene3.unity";
                ctx.additive = false;
                ctx.bundlePath = m_SceneBundlePath;

                LevelManager.Instance.LoadAsync(ctx);
            }

            if (GUI.Button(new Rect(300, 200, 150, 60), "Unload Scene3"))
            {
                LevelManager.Instance.UnloadLevelAsync("tscene3");
            }
        }
    }
}