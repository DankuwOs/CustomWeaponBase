﻿using Harmony;
using UnityEngine;

[HarmonyPatch(typeof(SMSInternalWeaponAnimator), nameof(SMSInternalWeaponAnimator.UpdateCurrentProfile))]
public class Patch_SMSInternalWhateverTheFuck
{
    public static bool Prefix(SMSInternalWeaponAnimator __instance, ref HPEquippable ___eq)
    {
        return !___eq.GetComponent<CWB_Weapon>();
    }
}