using UnityEngine;

/// <summary>
/// 发射器
/// </summary>
public class StuffSpawner : MonoBehaviour
{
    public StuffSpawnerRing Owner;

    public FloatRange timeBetweenSpawns, scale, randomVelocity, angularVelocity;

    public float velocity;

    public Material stuffMaterial;

    public Stuff[] stuffPrefabs;

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
        Stuff prefabAsset = stuffPrefabs[Random.Range(0, stuffPrefabs.Length)];
        Stuff spawn = (Stuff)prefabAsset.Pool.Get();

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