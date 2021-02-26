using UnityEngine;
using System.Collections.Generic;
using Framework.Cache;

namespace Application.Runtime.Tests
{
    /// <summary>
    /// 发射器
    /// </summary>
    public class StuffSpawner : MonoBehaviour
    {
        [HideInInspector]
        public StuffSpawnerRing Owner;

        public FloatRange timeBetweenSpawns, scale, randomVelocity, angularVelocity;

        public float velocity;

        public Material stuffMaterial;

        float timeSinceLastSpawn;
        float currentSpawnDelay;

        void FixedUpdate()
        {
            if (Owner.bPause)
                return;

            timeSinceLastSpawn += Time.deltaTime;
            if (timeSinceLastSpawn >= currentSpawnDelay)
            {
                timeSinceLastSpawn -= currentSpawnDelay;
                currentSpawnDelay = timeBetweenSpawns.RandomInRange;
                SpawnStuff();
            }
        }

        void SpawnStuff()
        {
            PrefabObjectPool pool = Owner.PoolList[Random.Range(0, Owner.PoolList.Count)];
            Stuff spawn = (Stuff)pool.Get();

            spawn.transform.localPosition = transform.position;
            spawn.transform.localScale = Vector3.one * scale.RandomInRange;
            spawn.transform.localRotation = Random.rotation;

            spawn.Body.velocity = transform.up * velocity +
                Random.onUnitSphere * randomVelocity.RandomInRange;
            spawn.Body.angularVelocity =
                Random.onUnitSphere * angularVelocity.RandomInRange;

            spawn.SetMaterial(stuffMaterial);
        }
    }
}