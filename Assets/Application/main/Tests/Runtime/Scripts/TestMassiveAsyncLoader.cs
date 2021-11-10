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
            }
        }
        
        void OnCompleted(GameObject go)
        {
            Debug.Log($"{Time.frameCount}");
            go.transform.position = Random.insideUnitSphere * 5;
        }
    }
}