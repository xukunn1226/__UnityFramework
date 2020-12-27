using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices; //DllImport��Ҫ��namespace

#if UNITY_IOS
using XDelta = XDelta.IOS;
#elif UNITY_ANDROID
using XDelta = XDelta.XDelta;       // XDelta.Android
#elif UNITY_STANDALONE_WIN
using XDelta = XDelta.XDelta;   // XDelta.Windows;
#else
using XDelta = XDelta.XDelta;

#endif
namespace XDelta
{
    class XDelta
    {
    }
}


public class TestDll : MonoBehaviour
{
#if UNITY_IOS
    const string dllName = "__Internal";
#elif UNITY_ANDROID
    const string dllName = "TestNative";
#elif UNITY_STANDALONE_WIN
    const string dllName = "TestNative";
#else
    const string dllName = "TestNative";
#endif

    [DllImport(dllName)] //������ǵ��õ�dll����
    public static extern int MyAddFunc(int x, int y);

    // Use this for initialization
    IEnumerator Start()
    {
        yield return new WaitForSeconds(2);

        int ret = MyAddFunc(200, 200);
        Debug.Log($"--- ret:{ret}");
    }
}