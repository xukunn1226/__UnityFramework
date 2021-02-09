using UnityEngine;

namespace Framework.LevelManager.Tests
{
    public class TestLevelManager : MonoBehaviour
    {
        private const string m_SceneBundlePath = "assets/framework/core/levelmanager/tests/res/scenes.ab";
        
        private LevelManager.LevelContext m_Ctx1;
        private LevelManager.LevelContext m_Ctx2;

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
                m_Ctx1 = new LevelManager.LevelContext();
                m_Ctx1.sceneName = "testscene2";
                m_Ctx1.scenePath = "assets/framework/core/levelmanager/tests/res/scenes/testscene2.unity";
                m_Ctx1.additive = true;
                m_Ctx1.bundlePath = m_SceneBundlePath;

                LevelManager.Instance.LoadAsync(m_Ctx1);
            }

            if (GUI.Button(new Rect(500, 100, 300, 120), "Unload Scene2"))
            {
                LevelManager.Instance.UnloadAsync(m_Ctx1);
            }

            if (GUI.Button(new Rect(100, 300, 300, 120), "Load Scene3"))
            {
                m_Ctx2 = new LevelManager.LevelContext();
                m_Ctx2.sceneName = "testscene3";
                m_Ctx2.scenePath = "assets/framework/core/levelmanager/tests/res/scenes/testscene3.unity";
                m_Ctx2.additive = false;
                m_Ctx2.bundlePath = m_SceneBundlePath;

                LevelManager.Instance.LoadAsync(m_Ctx2);
            }

            if (GUI.Button(new Rect(500, 300, 300, 120), "Unload Scene3"))
            {
                LevelManager.Instance.UnloadAsync(m_Ctx2);
            }
        }
    }
}