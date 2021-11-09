using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;

namespace Application.Runtime
{
    public class TestAsyncLoader : MonoBehaviour
    {
        private TestLoadCube m_Inst = new TestLoadCube();
        private IEnumerator m_Coroutine;
        private PrefabLoaderAsync m_Loader;

        // Start is called before the first frame update
        void Start()
        {
            // StartCoroutine(Load("assets/res/players/symbol.prefab"));

            m_Loader = AssetManager.InstantiatePrefabAsync("assets/res/players/symbol.prefab");

            // m_Coroutine = CoroutineA(1, "dd");
        }

        // Update is called once per frame
        void Update()
        {
            // if(Input.anyKeyDown)
            // {
            //     bool next = m_Coroutine.MoveNext();
            //     Debug.Log($"MoveNext: {next}");
            // }

            // if(Input.anyKeyDown)
            {
                bool next = m_Loader.MoveNext();
                Debug.Log($"MoveNext: {next}");
            }
        }

        void OnGUI()
        {

        }

        private IEnumerator Load(string assetPath)
        {
            PrefabLoaderAsync loader = AssetManager.InstantiatePrefabAsync(assetPath);
            // while(loader.MoveNext())
            // {
            //     Debug.Log(Time.frameCount);
            //     yield return null;
            // }

            // finish
            Debug.LogWarning("========== Finish    " + Time.frameCount);
            yield break;
        }

        public IEnumerator CoroutineA(int arg1, string arg2)
        {
            Debug.Log($"协程A被开启了");
            yield return null;
            Debug.Log("刚刚协程被暂停了一帧");
            yield return new WaitForSeconds(1.0f);
            Debug.Log("刚刚协程被暂停了一秒");
            yield return StartCoroutine(CoroutineB(arg1, arg2));
            Debug.Log("CoroutineB运行结束后协程A才被唤醒");
            yield return new WaitForEndOfFrame();
            Debug.Log("在这一帧的最后，协程被唤醒");
            Debug.Log("协程A运行结束");
        }

        public IEnumerator CoroutineB(int arg1, string arg2)
        {
            Debug.Log($"协程B被开启了，可以传参数，arg1={arg1}, arg2={arg2}");
            yield return new WaitForSeconds(3.0f);
            Debug.Log("协程B运行结束");
        }
    }

    public class TestLoadCube
    {
        public GameObject cube;
        public void Load()
        {

        }

    }
}