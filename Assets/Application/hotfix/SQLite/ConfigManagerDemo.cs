using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Application.Runtime;

namespace SQLite
{
    public class ConfigManagerDemo : MonoBehaviour
    {
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.T))
            {
                ConfigManager.Instance.GetPlayerByID(1);
            }
        }
    }
}
