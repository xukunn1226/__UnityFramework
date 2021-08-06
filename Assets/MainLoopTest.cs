using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LLH.SDK;

public class MainLoopTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 注册回调
        LLHSDK.OnLLHLoginCompleted += OnLLHLoginResult;

        // 初始化sdk
        LLHSDK.LLHStart();

        LLHSDK.LLHLogin();
    }

    // error：错误，loginType：登录类型，appUid：用户uid，appToken：用户登录token
    private void OnLLHLoginResult(LLHSDK.LLHError error, LLHSDK.LLHLoginTypeModel loginType, string appUid, string appToken)
    {
        if(error.success == 1)
        {
            Debug.Log($"Login success: appUid:{appUid}     appToken:{appToken}");
        }
        else
        {
            Debug.LogError($"Login failed: {error.msg}  {error.code}");
        }
    }
}
