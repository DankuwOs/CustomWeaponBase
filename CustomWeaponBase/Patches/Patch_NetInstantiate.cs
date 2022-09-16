using System.Collections.Generic;
using Harmony;
using UnityEngine;
using VTNetworking;

[HarmonyPatch(typeof(VTNetworkManager), "GetInstantiatePrefab")]
public class Patch_NetInstantiate
{
    public static bool Prefix(string resourcePath, ref Dictionary<string, GameObject> ___overriddenResources, ref GameObject __result)
    {
        // Most of this code is based on Temperz87's NotBDArmory: https://github.com/Temperz87/NotBDArmory
        GameObject gameObject;
        if (___overriddenResources.TryGetValue(resourcePath, out gameObject))
        {
            if (gameObject == null)
            {
                Debug.Log($"GameObject from {resourcePath} is null..");
            }
            else
            {
                Debug.Log($"NetInstantiated Object {gameObject.name}");
            }
            
            __result = gameObject;
            return false;
        }
        gameObject = Resources.Load<GameObject>(resourcePath);
        __result = gameObject;
        return false;
    }
}