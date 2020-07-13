using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LoadCharacter()
    {
        // step 1. load manifest
        AssetBundle manifestBundle = AssetBundle.LoadFromFile("windows");
        AssetBundleManifest manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        // step 2. load main bundle
        AssetBundle mainBundle = AssetBundle.LoadFromFile("assets/res/character.ab");

        // step 3. load dependency bundle
        string[] dependencies = manifest.GetAllDependencies("assets/res/character.ab");
        foreach(var dep in dependencies)
        {
            AssetBundle.LoadFromFile(dep);
        }

        // step 4. load asset
        GameObject character = mainBundle.LoadAsset<GameObject>("male.prefab");

        // step 5. instantiate
        GameObject inst = Instantiate(character);
    }

    void LoadCharacterEx()
    {


        GameObjectLoader loader = AssetManager.Instantiate("assets/res/character/male.prefab");






        if(loader.asset)
        {
            // ...
        }
    }

    IEnumerator LoadCharacterAsync()
    {
        GameObjectLoaderAsync loaderAsync = AssetManager.InstantiateAsync("assets/res/character/male.prefab");
        yield return loaderAsync;

        if(loaderAsync.asset != null)
        {
            // ...
        }
    }
}
