using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;

namespace Application.Runtime
{
    public class TestCoroutine_IEnumerator : MonoBehaviour
    {
        private IEnumerator m_Coroutine;
        private PrefabLoaderAsync m_Loader;

        void OnGUI()
        {
            if(GUI.Button(new Rect(100, 100, 400, 150), "Start Test1"))
            {
                // StartCoroutine(CoroutineA(1, "dd"));
                m_Coroutine = CoroutineA(1, "dd");
                // CoroutineA(1, "").Start();
            }
            if(GUI.Button(new Rect(550, 100, 400, 150), "MoveNext"))
            {
                bool next = m_Coroutine.MoveNext();
                Debug.Log($"MoveNext: {next}    {m_Coroutine.Current}");
            }



            if(GUI.Button(new Rect(100, 250, 400, 150), "Start Test2"))
            {
                m_Loader = AssetManager.InstantiatePrefabAsync("assets/res/players/symbol.prefab");
            }
            if(GUI.Button(new Rect(550, 250, 400, 150), "MoveNext"))
            {
                bool next = m_Loader.MoveNext();
                Debug.Log($"MoveNext: {next}");
            }
        }

        public IEnumerator CoroutineA(int arg1, string arg2)
        {
            Debug.Log($"协程A被开启了");
            yield return 1;
            Debug.Log("刚刚协程被暂停了一帧");
            yield return NewWaitForSeconds(5);
            Debug.Log("===============刚刚协程被暂停了一秒");
            yield return StartCoroutine(CoroutineB(arg1, arg2));
            Debug.Log("CoroutineB运行结束后协程A才被唤醒");
            yield return new WaitForEndOfFrame();
            yield return 2;
            Debug.Log("在这一帧的最后，协程被唤醒");
            Debug.Log("协程A运行结束");
        }

        private WaitForSeconds NewWaitForSeconds(float seconds)
        {
            return new WaitForSeconds(seconds);
        }

        public IEnumerator CoroutineB(int arg1, string arg2)
        {
            Debug.Log($"协程B被开启了，可以传参数，arg1={arg1}, arg2={arg2}");
            yield return new WaitForSeconds(1.0f);
            Debug.Log("协程B运行结束");
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
    }
}