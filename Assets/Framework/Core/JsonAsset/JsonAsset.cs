using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

namespace Framework.Core
{
    [Serializable]
    public class JsonAsset : ScriptableObject
    {
        public byte[] data;

        private object m_Asset;

        public T Require<T>(bool forceDeserialize = false) where T : class
        {
            if (m_Asset == null || forceDeserialize)
            {
                m_Asset = JsonConvert.DeserializeObject<T>(System.Text.Encoding.UTF8.GetString(data));
            }
            return (T)m_Asset;
        }
    }
}