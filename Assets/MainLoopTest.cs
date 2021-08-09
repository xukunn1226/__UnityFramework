using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using LLH.SDK;
#endif

public class MainLoopTest : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(1);
        Debug.Log("11111111111111111111111111");
#if UNITY_ANDROID
        Debug.LogError("SDK Start");

        // 注册回调
        LLHSDK.OnLLHLoginCompleted += OnLLHLoginResult;

        // 初始化sdk
        LLHSDK.LLHStart();

        Debug.LogError("SDK Login");

        LLHSDK.LLHLogin();
#endif
    }

#if UNITY_ANDROID
    // error：错误，loginType：登录类型，appUid：用户uid，appToken：用户登录token
    static public void OnLLHLoginResult(LLHSDK.LLHError error, LLHSDK.LLHLoginTypeModel loginType, string appUid, string appToken)
    {
        if(error.success == 1)
        {
            Debug.LogError($"Login success: appUid:{appUid}     appToken:{appToken}");
        }
        else
        {
            Debug.LogError($"Login failed: {error.msg}  {error.code}");
        }
    }
#endif    
}
