using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(AircraftLiveryApplicator), nameof(AircraftLiveryApplicator.ApplyLivery))]
public class Patch_AircraftLiveryApplicator
{
    public static void Postfix(AircraftLiveryApplicator __instance)
    {
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