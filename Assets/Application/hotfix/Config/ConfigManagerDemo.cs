using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Application.Runtime
{
    public class ConfigManagerDemo : MonoBehaviour
    {
        IEnumerator Start()
        {
            Debug.Log("----------");
            yield return new WaitForSeconds(1);


            PlayerConfig player = ConfigManager.Instance.GetPlayerConfigByID("1", "peter", "shanghai", 100);
                if(player != null)
                {
                    Debug.Log(player.Building_ID);
                    Debug.Log(player.Name);
                    Debug.Log(player.HP);
                    Debug.Log(player.MonsterDesc);
                }
            player = ConfigManager.Instance.GetPlayerConfigByID("1", "peter", "shanghai", 0);
            player = ConfigManager.Instance.GetPlayerConfigByID("1", "peter", "nanchang", 2);
        }
    }
}
