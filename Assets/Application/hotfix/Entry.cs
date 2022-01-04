using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Application.Runtime;

namespace Logic
{
    static public class Entry
    {
        static public void Start()
        {
            Debug.Log("Entry.Start======");
            CodeLoader.Instance.Update += Update;
        }

        static public void Update()
        {
            // Debug.Log("Update");
        }
    }
}