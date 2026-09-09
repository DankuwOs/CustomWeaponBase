using System;
using HarmonyLib;
using UnityEngine;
using VTOLVR.DLC.EW;

namespace CustomWeaponBase.Patches;

[HarmonyPatch(typeof(MulticrewMALDControlSync))]
public class Patch_MulticrewMALDControlSync
{
    // This packs the hpIdx to also contain information like missile count and if it does have multiple missiles (which might be redundant)
    [HarmonyPatch(nameof(MulticrewMALDControlSync.GetEqIdxFromDecoy))]
    [HarmonyPrefix]
    public static bool P_GetEqIdxFromDecoy(MulticrewMALDControlSync __instance, AirLaunchedDecoyGuidance decoy, ref int __result)
    {
        var hp = decoy.GetComponentInParent<HPEquipDecoyMissile>();
        
        
        if (hp.ml.missiles.Length <= 1)
            return false;
        
        int hpIdx = hp.hardpointIdx;
        int missileIdx = hp.ml.missiles.IndexOf(decoy.missile);
        uint bitMask = (uint)hpIdx;
        bitMask |= (uint)missileIdx << 8;
        bitMask |= 1 << 16;
        

        __result = (int)bitMask;
        return false;
    }
    
    // This checks if the eqIdx is a packed int for multiple missiles
    [HarmonyPatch(nameof(MulticrewMALDControlSync.GetDecoyFromEqIdx))]
    [HarmonyPrefix]
    public static bool P_GetDecoyFromEqIdx(MulticrewMALDControlSync __instance, int eqIdx, ref AirLaunchedDecoyGuidance __result)
    {
        
        int hpIdx = eqIdx & 0xFF;
        int missileIdx = eqIdx >> 8 & 0xFF;
        bool multipleMissiles = ((eqIdx >> 16) & 0xFF) == 1;

        if (!multipleMissiles)
            return true;
        
        HPEquippable equip = __instance.maldUI.wm.GetEquip(hpIdx);
        if (equip && equip is HPEquipDecoyMissile)
        {
            MissileLauncher ml = ((HPEquipDecoyMissile)__instance.maldUI.wm.GetEquip(hpIdx)).ml;
            if (ml && ml.missileCount > 0)
            {
                __result = (AirLaunchedDecoyGuidance)ml.missiles[missileIdx].guidanceUnit;
                return false;
            }
        }
        __result = null;
        return false;
    }
}