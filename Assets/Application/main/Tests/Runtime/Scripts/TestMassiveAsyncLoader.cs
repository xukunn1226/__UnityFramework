using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public class TestMassiveAsyncLoader : MonoBehaviour
    {
        static private string[] s_AssetPathList = new string[] {"assets/res/players/zombie_01_variant.prefab",
                                                                "assets/res/players/zombie_02_variant.prefab"};

        void OnGUI()
        {
            if(GUI.Button(new Rect(100, 100, 300, 100), "Create ZOMBIE_01"))
            {
                for(int i = 0; i < 1; ++i)
                    AsyncLoaderManager.Instance.AsyncLoad(s_AssetPathList[0], OnCompleted);
                Destroy(gameObject);        // 测试发送异步加载请求的对象，如果在资源加载完成之前被销毁的情况，见OnCompleted
                Debug.Log($"1. {Time.frameCount}");
            }
        }
        
        void OnCompleted(GameObject go, System.Object userData)
        {
            Debug.Log($"2. {Time.frameCount}");
            if(this == null)
            { // 对象本身在资源加载完成之前可能已经销毁，需要判断
                Debug.Log($"发送异步加载请求的对象已销毁    {Time.frameCount}");
                return;
            }
            go.transform.position = Random.insideUnitSphere * 5;
        }
    }
}