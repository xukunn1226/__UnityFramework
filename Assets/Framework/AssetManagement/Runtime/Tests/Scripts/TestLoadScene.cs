using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime.Tests
{
    public class TestLoadScene : MonoBehaviour
    {
        private void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 200, 80), "Load Scene1"))
            {
                Debug.Log("1111: " + Time.frameCount);
                StartCoroutine(LoadSceneAsync("testscene2", UnityEngine.SceneManagement.LoadSceneMode.Additive));
                Debug.Log("2222: " + Time.frameCount);
            }

            if (GUI.Button(new Rect(100, 280, 200, 80), "Unload"))
            {
            }
        }

        IEnumerator LoadSceneAsync(string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            yield return AssetManager.LoadSceneAsync(sceneName, mode);
        }
    }
}