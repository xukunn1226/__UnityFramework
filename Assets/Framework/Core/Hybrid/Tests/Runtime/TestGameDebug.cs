using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Tests
{
    public class TestGameDebug : MonoBehaviour
    {
        IEnumerator Start()
        {
            GameDebug.LogError($"Error: 1111111, {Time.time}");
            GameDebug.Log("Log: 45698");
            GameDebug.LogWarning("Warning: 55555555553223424dsdsgt3453");
            for(int i = 0; i < 100; ++i)
                GameDebug.Log("Log: 0000000000");
            GameDebug.LogError("-------------");

            yield return null;
            GameDebug.LogError("1");
            yield return new WaitForSeconds(0.5f);
            GameDebug.LogError("2");
            for(int i = 0; i < 100; ++i)
            {
                GameDebug.LogError(i);
                yield return new WaitForSeconds(0.5f);
            }
            yield break;
        }
    }
}