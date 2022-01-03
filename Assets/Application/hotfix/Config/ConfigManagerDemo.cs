using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Application.Logic
{
    public class ConfigManagerDemo : MonoBehaviour
    {
        IEnumerator Start()
        {
            Debug.Log("----------");
            yield return new WaitForSeconds(1);

            Debug.developerConsoleVisible = false;

            PlayerConfig player = ConfigManager.Instance.GetPlayerConfigByID("1", "peter", "shanghai", 100);
                if(player != null)
                {
                    Debug.LogError(player.Building_ID);
                    Debug.LogWarning(player.Name);
                    Debug.Log(player.HP);
                    Debug.Log(player.MonsterDesc);
                    UnityEngine.Debug.LogError("33333333333");
                    UnityEngine.Debug.LogWarning("5555555555555");
                    UnityEngine.Debug.Log("777777777777");
                }
            player = ConfigManager.Instance.GetPlayerConfigByID("1", "peter", "shanghai", 0);
            player = ConfigManager.Instance.GetPlayerConfigByID("1", "peter", "nanchang", 2);
            
        }
    }
}
