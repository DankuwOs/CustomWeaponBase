﻿using Harmony;
using UnityEngine;

[HarmonyPatch(typeof(AircraftLiveryApplicator), nameof(AircraftLiveryApplicator.ApplyLivery))]
public class Patch_AircraftLiveryApplicator
{
    public static void Postfix(AircraftLiveryApplicator __instance)
    {
        // This makes sure that whenever a livery is applied to the aircraft any liverymesh equips currently on will have the livery applied.
        
        WeaponManager wm = __instance.GetComponent<WeaponManager>();

        if (!wm)
            return;

        for (int i = 0; i < wm.equipCount; i++)
        {
            var equip = wm.GetEquip(i);
            if (!equip)
                continue;

            var liveryMesh = equip.GetComponent<LiveryMesh>();

            if (!liveryMesh)
                continue;

            CustomWeaponsBase.ApplyLivery(equip, wm);
        }
    }
}