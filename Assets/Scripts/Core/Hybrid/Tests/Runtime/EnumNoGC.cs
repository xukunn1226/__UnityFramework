using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnumNoGC : MonoBehaviour
{
    Dictionary<KeyEnum, KeyEnum> enumDict2 = new Dictionary<KeyEnum, KeyEnum>(new KeyEnumComparer())
        {
            {KeyEnum.K1,KeyEnum.K1},
            {KeyEnum.K2,KeyEnum.K1},
            {KeyEnum.K3,KeyEnum.K1},
            {KeyEnum.K4,KeyEnum.K1}
        };

    List<KeyEnum> list = new List<KeyEnum>();
    
    // Start is called before the first frame update
    void Start()
    {
        list.Add(KeyEnum.K1);
        list.Add(KeyEnum.K2);
        list.Add(KeyEnum.K3);
        list.Add(KeyEnum.K4);
    }

    // Update is called once per frame
    void Update()
    {
        UnityEngine.Profiling.Profiler.BeginSample("1111");
        enumDict2.ContainsKey(KeyEnum.K1);
        list.Contains(KeyEnum.K1);
        UnityEngine.Profiling.Profiler.EndSample();
    }
}

enum KeyEnum
{
    K1,
    K2,
    K3,
    K4,
}

struct KeyEnumComparer : IEqualityComparer<KeyEnum>
{
    public bool Equals(KeyEnum x, KeyEnum y)
    {
        return x == y;
    }

    public int GetHashCode(KeyEnum obj)
    {
        // you need to do some thinking here,
        return (int)obj;
    }
}
