using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

/*
 * 把*.lua脚本封装为ScriptableObject
*/
[Serializable]
public class LuaAsset : ScriptableObject
{
    public static string    LuaDecodeKey = "82bbd3e0-296a-42c7-9e5f-b953e787e1a7";    
    public bool             encode = true;
    public byte[]           data;

    public byte[] Require()
    {
        return encode ? Security.XXTEA.Decrypt(this.data, LuaAsset.LuaDecodeKey) : this.data;
    }    
}
