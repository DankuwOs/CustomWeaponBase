
using Harmony;
using UnityEngine;

[HarmonyPatch(typeof(HPEquipOpticalML), nameof(HPEquipOpticalML.GetCount))]
public class Patch_HPEquipOML
{
    public static bool Prefix(HPEquipOpticalML __instance)
    {
        if (!__instance.oml)
        {
            __instance.oml = __instance.ml as OpticalMissileLauncher; // Fixes issue in mp where getcount doesnt know what the fuck an oml is
        }

        return true;
    }
}