using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationInstancingModule.Runtime
{
    public class RandomInstancing : MonoBehaviour
    {
        public List<AnimationInstancing> prototypeList = new List<AnimationInstancing>();
        public float interval = 0.1f;
        public Vector3 size;
        public int instancingCount = 100;
        public int count;

        void Awake()
        {
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            size = renderer.bounds.size;
        }

        void Start()
        {
            StartCoroutine(RandomInstantiate());
        }
        
        IEnumerator RandomInstantiate()
        {
            while(count++ < instancingCount)
            {
                int index = Random.Range(0, prototypeList.Count);
                Vector3 unit = Random.insideUnitSphere;
                Vector3 pos = new Vector3(unit.x * size.x * 0.5f, gameObject.transform.position.y, unit.z * size.z * 0.5f);

                AnimationInstancing inst = Instantiate<AnimationInstancing>(prototypeList[index]);
                inst.transform.position = pos;
                inst.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);

                yield return new WaitForSeconds(interval);
            }
            Debug.Log("Done.............");
        }
    }
}