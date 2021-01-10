using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    public class ConfigSection : Dictionary<string, string>             // key: ClassName.FieldName; value: "value".toString
    { }

    public class GlobalConfig : Dictionary<string, ConfigSection>       // key: namespace
    { }
}