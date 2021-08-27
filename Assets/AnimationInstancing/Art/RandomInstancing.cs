using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationInstancingModule.Runtime
{
    public class RandomInstancing : MonoBehaviour
    {
        public List<AnimationInstancing> prototypeList = new List<AnimationInstancing>();
        public Vector3 size;
        public int instancingCount = 100;

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
            int index = Random.Range(0, prototypeList.Count);
            Vector3 unit = Random.insideUnitSphere;
            Vector3 pos = new Vector3(unit.x * size.x, gameObject.transform.position.y, unit.z * size.z);

            AnimationInstancing inst = Instantiate<AnimationInstancing>(prototypeList[index]);
            inst.transform.position = pos;

            yield return new WaitForSeconds(1);
        }
    }
}