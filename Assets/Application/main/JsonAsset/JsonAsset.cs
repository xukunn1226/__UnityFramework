using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

[Serializable]
public class JsonAsset : ScriptableObject
{
    public byte[] data;

    private object m_Asset;

    public T Require<T>() where T : class
    {
        if (m_Asset == null)
        {
            m_Asset = JsonConvert.DeserializeObject<T>(System.Text.Encoding.UTF8.GetString(data));
        }
        return (T)m_Asset;
    }
}
