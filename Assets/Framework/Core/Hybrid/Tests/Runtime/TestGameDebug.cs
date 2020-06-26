using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Tests
{
    public class TestGameDebug : MonoBehaviour
    {
        void Start()
        {
            GameDebug.LogError($"Error: 1111111, {Time.time}");
            GameDebug.Log("Log: 45698");
            GameDebug.LogWarning("Warning: 5555555555");
            GameDebug.Log("Log: 0000000000");
        }
    }
}