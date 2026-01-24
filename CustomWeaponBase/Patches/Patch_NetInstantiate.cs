using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using VTNetworking;


[HarmonyPatch(typeof(VTNetworkManager), "GetInstantiatePrefab")]
public class Patch_NetInstantiate
{
    public static bool Prefix(string resourcePath,  ref GameObject __result)
    {
        GameObject gameObject;
        if (VTNetworkManager.overriddenResources.TryGetValue(resourcePath, out gameObject))
        {
            if (gameObject == null)
            {
                Debug.Log($"[CWB]: GameObject from {resourcePath} is null..");
            }
            
            __result = gameObject;
            return false;
        }
        gameObject = Resources.Load<GameObject>(resourcePath);
        __result = gameObject;
        return false;
    }
}