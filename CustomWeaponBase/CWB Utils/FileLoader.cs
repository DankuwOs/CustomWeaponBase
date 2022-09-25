using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomWeaponBase.CWB_Utils
{
    static class FileLoader
    {
        //PUBLIC LOADING METHODS
        //Thanks Baan/GentleLeviathan!!!!

        static Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>();

        public static GameObject GetAssetBundleAsGameObject(string path, string name)
        {
            if (assetBundles.ContainsKey(path))
            {
                try
                {
                    AssetBundle assetBundle;
                    assetBundles.TryGetValue(path, out assetBundle);

                    if (assetBundle != null)
                    {
                        Debug.Log("AssetBundleLoader: Found AssetBundle for " + path);
                        return assetBundle.LoadAsset<GameObject>(name);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Error loading gameobject from " + path + ". Exception: " + e.Message);
                }
            }

            Debug.Log("AssetBundleLoader: Attempting to load AssetBundle...");
            AssetBundle bundle = null;
            try
            {
                bundle = AssetBundle.LoadFromFile(path);
                assetBundles.Add(path, bundle);
                Debug.Log("AssetBundleLoader: Success.");
            }
            catch (Exception e)
            {
                Debug.Log("AssetBundleLoader: Couldn't load AssetBundle from path: '" + path +
                          "'. Exception details: e: " + e.Message);
            }

            Debug.Log("AssetBundleLoader: Attempting to retrieve: '" + name + "' as type: 'GameObject'.");
            try
            {
                var temp = bundle.LoadAsset(name, typeof(GameObject));
                Debug.Log("AssetBundleLoader: Success.");
                return (GameObject) temp;
            }
            catch (Exception e)
            {

                Debug.Log("AssetBundleLoader: Couldn't retrieve GameObject from AssetBundle.");
                return null;
            }
        }
    }
}