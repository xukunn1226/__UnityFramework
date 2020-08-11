using UnityEngine;
using System.Collections;
using Framework.Cache;
using System.Collections.Generic;

public class StuffSpawnerRing : MonoBehaviour
{
    public int numberOfSpawners;

    public float radius, tiltAngle;

    public Material[] stuffMaterials;

    public StuffSpawner spawnerPrefab;

    public List<string> PoolPathList = new List<string>();      // bundle name list
    public List<string> AssetNameList = new List<string>();     // asset name list
    
    [HideInInspector]
    public List<PrefabObjectPool> PoolList = new List<PrefabObjectPool>();

    [HideInInspector]
    public bool bPause;

    IEnumerator Start()
    {
        if(PoolPathList.Count != AssetNameList.Count)
            throw new System.Exception("PoolPathList.Count != AssetNameList.Count");

        for(int i = 0; i < PoolPathList.Count; ++i)
        {
            GameObject go = ResourceManager.Instantiate(PoolPathList[i], AssetNameList[i]).asset;
            PrefabObjectPool pool = go.GetComponent<PrefabObjectPool>();
            if(pool != null)
            {
                PoolList.Add(pool);
            }
        }
        for (int i = 0; i < numberOfSpawners; ++i)
        {
            CreateSpawner(i);
            yield return new WaitForSeconds(1);
        }
    }

    void CreateSpawner(int index)
    {
        Transform rotater = new GameObject("Rotater").transform;
        rotater.SetParent(transform, false);
        rotater.localRotation =
            Quaternion.Euler(0f, index * 360f / numberOfSpawners, 0f);

        StuffSpawner spawner = Instantiate<StuffSpawner>(spawnerPrefab);
        spawner.Owner = this;
        spawner.transform.SetParent(rotater, false);
        spawner.transform.localPosition = new Vector3(0f, 0f, radius);
        spawner.transform.localRotation = Quaternion.Euler(tiltAngle, 0f, 0f);

        spawner.stuffMaterial = stuffMaterials[index % stuffMaterials.Length];
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(100, 100, 120, 80), "Pause"))
        {
            bPause = !bPause;
        }
    }
}