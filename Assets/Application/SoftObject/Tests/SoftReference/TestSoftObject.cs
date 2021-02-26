using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Application.Runtime.Tests
{
    /// <summary>
    /// 
    /// </summary>
    [ExecuteInEditMode]
    public class TestSoftObject : MonoBehaviour
    {
        [SoftObject]
        public SoftObjectPath m_BuildingVillage;

        [SoftObject]
        public SoftObject m_Smoke;
    }
}