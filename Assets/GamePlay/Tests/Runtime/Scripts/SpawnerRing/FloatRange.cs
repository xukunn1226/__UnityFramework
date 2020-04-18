using UnityEngine;

[System.Serializable]
public struct FloatRange
{

    public float min, max;

    public float RandomInRange
    {
        get
        {
            return Random.Range(min, max);
        }
    }

    public Vector3 RandomInRange3
    {
        get
        {
            Vector3 v;
            v.x = RandomInRange;
            v.y = RandomInRange;
            v.z = RandomInRange;
            return v;
        }
    }
}