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

            Player player = ConfigManager.Instance.GetPlayerByID("1");
                if(player != null)
                {
                    Debug.Log(player.Building_ID);
                    Debug.Log(player.Name);
                    Debug.Log(player.HP);
                    Debug.Log(player.Male);
                }
            // player = ConfigManager.Instance.GetPlayerByID("2", "xukun");
        }
    }
}
