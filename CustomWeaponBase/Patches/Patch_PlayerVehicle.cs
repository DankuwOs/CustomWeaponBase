using Harmony;
using UnityEngine;

[HarmonyPatch(typeof(PlayerVehicle), nameof(PlayerVehicle.GetEquipPrefab))]
public class Patch_PlayerVehicle
{
    public static void Postfix(PlayerVehicle __instance, string equipName, ref GameObject __result)
    {
        __result = __result == null ? VTResources.Load<GameObject>($"{__instance.equipsResourcePath}/{equipName}") : __result;
    }
}