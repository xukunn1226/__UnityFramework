using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AppVersion : ScriptableObject, IComparable<AppVersion>
{
    public int      MainVersion;
    public int      MinorVersion;
    public int      Revision;
    public int      BuildNumber;

    new public string ToString()
    {
        return string.Format($"{MainVersion}.{MinorVersion}.{Revision}");
    }

    public int CompareTo(AppVersion other)
    {
        return 0;
    }
}
