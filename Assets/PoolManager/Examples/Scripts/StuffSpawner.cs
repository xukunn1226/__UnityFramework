using UnityEngine;

public class StuffSpawner : MonoBehaviour
{
    public FloatRange timeBetweenSpawns, scale, randomVelocity, angularVelocity;

    public float velocity;

    public Material stuffMaterial;

    public Stuff[] stuffPrefabs;

    float timeSinceLastSpawn;
    float currentSpawnDelay;

    void FixedUpdate()
    {
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
        Stuff prefab = stuffPrefabs[Random.Range(0, stuffPrefabs.Length)];
        Stuff spawn = (Stuff)prefab.Pool.Get();

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