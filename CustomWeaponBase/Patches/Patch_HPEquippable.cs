using System.Linq;
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(HPEquippable))]
[HarmonyPatch(nameof(HPEquippable.Equip))]
public class Patch_HPEquippable_Equip
{
    [HarmonyPrefix]
    public static void EquipPrefix(HPEquippable __instance) // Required to fix "Coroutine (GUpdateRoutine) could not be started"
    {
        if (!__instance)
            return;

        if (!__instance.gameObject.activeSelf)
        {
            __instance.gameObject.SetActive(true);
        }
        
        var extension = __instance.GetComponent<CWB_HPEquipExtension>();
        if (extension)
        {
            extension.hpEquip = __instance;
        }
    }

    [HarmonyPostfix]
    public static void EquipPostfix(HPEquippable __instance)
    {
        if (!__instance)
            return;
        
        // Just incase
        CustomWeaponsBase.ApplyLivery(__instance, __instance.weaponManager);
        
        CustomWeaponsBase.ToggleMeshHider(__instance, __instance.weaponManager);

        var extension = __instance.GetComponent<CWB_HPEquipExtension>();
        if (extension)
        {
            extension.Equip();
        }
    }
}

[HarmonyPatch(typeof(HPEquippable))]
[HarmonyPatch("OnEquip")]
public class Patch_HPEquippable_OnEquip
{
    [HarmonyPostfix]
    public static void OnEquip(HPEquippable __instance)
    {
        if (!__instance)
            return;

        var extension = __instance.GetComponent<CWB_HPEquipExtension>();
        if (extension)
            extension.OnEquip();
    }
}

[HarmonyPatch(typeof(HPEquippable))]
[HarmonyPatch("OnUnequip")]
public class Patch_HPEquippable_OnUnequip
{
    [HarmonyPostfix]
    public static void OnUnequip(HPEquippable __instance)
    {
        if (!__instance)
            return;
        
        CustomWeaponsBase.ToggleMeshHider(__instance, __instance.weaponManager, true);
        
        var extension = __instance.GetComponent<CWB_HPEquipExtension>();
        if (extension)
            extension.OnUnequip();
    }
}

[HarmonyPatch(typeof(HPEquippable))]
[HarmonyPatch(nameof(HPEquippable.OnConfigAttach))]
public class Patch_HPEquippable_OnConfigAttach
{
    [HarmonyPostfix]
    public static void OnConfigAttach(HPEquippable __instance, LoadoutConfigurator configurator)
    {
        if (!__instance)
            return;

        CustomWeaponsBase.ToggleMeshHider(__instance, configurator.wm);

        var extension = __instance.GetComponent<CWB_HPEquipExtension>();
        if (extension)
            extension.OnConfigAttach(configurator);
    }
}

[HarmonyPatch(typeof(HPEquippable))]
[HarmonyPatch(nameof(HPEquippable.OnConfigDetach))]
public class Patch_HPEquippable_OnConfigDetach
{
    [HarmonyPostfix]
    public static void OnConfigDetach(HPEquippable __instance, LoadoutConfigurator configurator)
    {
        if (!__instance)
            return;

        CustomWeaponsBase.ToggleMeshHider(__instance, configurator.wm, true);

        var extension = __instance.GetComponent<CWB_HPEquipExtension>();
        if (extension)
            extension.OnConfigDetach(configurator);
    }
}
[HarmonyPatch(typeof(HPEquippable))]
[HarmonyPatch(nameof(HPEquippable.Jettison))]
public class Patch_HPEquippable_Jettison
{
    [HarmonyPostfix]
    public static void Jettison(HPEquippable __instance)
    {
        if (!__instance)
            return;

        var extension = __instance.GetComponent<CWB_HPEquipExtension>();
        if (extension)
            extension.Jettison();
    }
}

[HarmonyPatch(typeof(HPEquippable))]
[HarmonyPatch("OnJettison")]
public class Patch_HPEquippable_OnJettison
{
    [HarmonyPostfix]
    public static void OnJettison(HPEquippable __instance)
    {
        if (!__instance)
            return;

        var extension = __instance.GetComponent<CWB_HPEquipExtension>();
        if (extension)
            extension.OnJettison();
    }
}

[HarmonyPatch(typeof(HPEquippable))]
[HarmonyPatch(nameof(HPEquippable.OnCycleWeaponButton))]
public class Patch_HPEquippable_OnCycleWeaponButton
{
    [HarmonyPostfix]
    public static void OnCycleWeaponButton(HPEquippable __instance)
    {
        if (!__instance)
            return;

        var extension = __instance.GetComponent<CWB_HPEquipExtension>();
        if (extension)
            extension.OnCycleWeaponButton();
    }
}

[HarmonyPatch(typeof(HPEquippable))]
[HarmonyPatch(nameof(HPEquippable.OnReleasedCycleWeaponButton))]
public class Patch_HPEquippable_OnReleasedCycleWeaponButton
{
    [HarmonyPostfix]
    public static void OnReleasedCycleWeaponButton(HPEquippable __instance)
    {
        if (!__instance)
            return;

        var extension = __instance.GetComponent<CWB_HPEquipExtension>();
        if (extension)
            extension.OnReleasedCycleWeaponButton();
    }
}

[HarmonyPatch(typeof(HPEquippable))]
[HarmonyPatch(nameof(HPEquippable.OnDisabledByPartDestroy))]
public class Patch_HPEquippable_OnDisabledByPartDestroy
{
    [HarmonyPostfix]
    public static void OnDisabledByPartDestroy(HPEquippable __instance)
    {
        if (!__instance)
            return;

        var extension = __instance.GetComponent<CWB_HPEquipExtension>();
        if (extension)
            extension.OnDisabledByPartDestroy();
    }
}

[HarmonyPatch(typeof(HPEquippable))]
[HarmonyPatch(nameof(HPEquippable.OnDisableWeapon))]
public class Patch_HPEquippable_OnDisableWeapon
{
    [HarmonyPostfix]
    public static void OnDisableWeapon(HPEquippable __instance)
    {
        if (!__instance)
            return;

        var extension = __instance.GetComponent<CWB_HPEquipExtension>();
        if (extension)
            extension.OnDisableWeapon();
    }
}

[HarmonyPatch(typeof(HPEquippable))]
[HarmonyPatch(nameof(HPEquippable.OnEnableWeapon))]
public class Patch_HPEquippable_OnEnableWeapon
{
    [HarmonyPostfix]
    public static void OnEnableWeapon(HPEquippable __instance)
    {
        if (!__instance)
            return;

        var extension = __instance.GetComponent<CWB_HPEquipExtension>();
        if (extension)
            extension.OnEnableWeapon();
    }
}

[HarmonyPatch(typeof(HPEquippable))]
[HarmonyPatch(nameof(HPEquippable.OnQuickloadEquip))]
public class Patch_HPEquippable_OnQuickloadEquip
{
    [HarmonyPostfix]
    public static void OnQuickloadEquip(HPEquippable __instance, ConfigNode eqNode)
    {
        if (!__instance)
            return;

        var extension = __instance.GetComponent<CWB_HPEquipExtension>();
        if (extension)
            extension.OnQuickloadEquip(eqNode);
    }
}

[HarmonyPatch(typeof(HPEquippable))]
[HarmonyPatch(nameof(HPEquippable.OnQuicksaveEquip))]
public class Patch_HPEquippable_OnQuicksaveEquip
{
    [HarmonyPostfix]
    public static void OnQuicksaveEquip(HPEquippable __instance, ConfigNode eqNode)
    {
        if (!__instance)
            return;

        var extension = __instance.GetComponent<CWB_HPEquipExtension>();
        if (extension)
            extension.OnQuicksaveEquip(eqNode);
    }
}

[HarmonyPatch(typeof(HPEquippable))]
[HarmonyPatch(nameof(HPEquippable.OnStartFire))]
public class Patch_HPEquippable_OnStartFire
{
    [HarmonyPostfix]
    public static void OnStartFire(HPEquippable __instance)
    {
        if (!__instance)
            return;

        var extension = __instance.GetComponent<CWB_HPEquipExtension>();
        if (extension)
            extension.OnStartFire();
    }
}

[HarmonyPatch(typeof(HPEquippable))]
[HarmonyPatch(nameof(HPEquippable.OnStopFire))]
public class Patch_HPEquippable_OnStopFire
{
    [HarmonyPostfix]
    public static void OnStopFire(HPEquippable __instance)
    {
        if (!__instance)
            return;

        var extension = __instance.GetComponent<CWB_HPEquipExtension>();
        if (extension)
            extension.OnStopFire();
    }
}

[HarmonyPatch(typeof(HPEquippable))]
[HarmonyPatch(nameof(HPEquippable.OnTriggerAxis))]
public class Patch_HPEquippable_OnTriggerAxis
{
    [HarmonyPostfix]
    public static void OnTriggerAxis(HPEquippable __instance, float axis)
    {
        if (!__instance)
            return;
        
        var extension = __instance.GetComponent<CWB_HPEquipExtension>();
        if (extension)
            extension.OnTriggerAxis(axis);
    }
}

[HarmonyPatch(typeof(HPEquippable))]
[HarmonyPatch(nameof(HPEquippable.UpdateWeaponType))]
public class Patch_HPEquippable_UpdateWeaponType
{ 
    [HarmonyPostfix]
    public static void UpdateWeaponType(HPEquippable __instance)
    {
        if (!__instance)
            return;

        var extension = __instance.GetComponent<CWB_HPEquipExtension>();
        if (extension)
            extension.UpdateWeaponType();
    }
}


[HarmonyPatch(typeof(HPEquippable))]
[HarmonyPatch(nameof(HPEquippable.ApplyLivery))]
public class Patch_HPEquippable_ApplyLivery
{
    [HarmonyPrefix]
    public static void Prefix(HPEquippable __instance)
    {
        if (__instance.matchLiveries.Any(l => l == null))
        {
            Debug.LogError($"[CWB]: The HPEquippable on '{__instance}' has a null / empty mesh renderer in the Match Liveries list!");
        }
    }
    
    [HarmonyPostfix]
    public static void ApplyLivery(HPEquippable __instance, WeaponManager wm)
    {
        if (!__instance)
            return;
        
        
        CustomWeaponsBase.ApplyLivery(__instance, wm);
    }
}




