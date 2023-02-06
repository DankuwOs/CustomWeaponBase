using Harmony;


[HarmonyPatch(typeof(HPEquippable), nameof(HPEquippable.Equip))]
public class Patch_HPEquippable_Equip
{
    [HarmonyPrefix]
    public static void EquipPrefix(HPEquippable __instance) // Required to fix "Coroutine (GUpdateRoutine) could not be started"
    {
        if (!__instance.gameObject.activeSelf)
        {
            __instance.gameObject.SetActive(true);
        }
    }
    
    [HarmonyPostfix]
    public static void EquipPostfix(HPEquippable __instance)
    {
        CustomWeaponsBase.ApplyLivery(__instance, __instance.weaponManager);
        CustomWeaponsBase.ToggleMeshHider(__instance, __instance.weaponManager);
    }
}

[HarmonyPatch(typeof(HPEquippable), nameof(HPEquippable.OnConfigAttach))]
public class Patch_HPEquippable_OnConfigAttach
{
    [HarmonyPostfix]
    public static void OnConfigAttach(HPEquippable __instance, LoadoutConfigurator configurator)
    {
        CustomWeaponsBase.ApplyLivery(__instance, configurator.wm);
        CustomWeaponsBase.ToggleMeshHider(__instance, configurator.wm);
    }
}

[HarmonyPatch(typeof(HPEquippable), nameof(HPEquippable.OnConfigDetach))]
public class Patch_HPEquippable_OnConfigDetach
{
    [HarmonyPostfix]
    public static void OnConfigDetach(HPEquippable __instance, LoadoutConfigurator configurator)
    {
        CustomWeaponsBase.ToggleMeshHider(__instance, configurator.wm, true);
    }
}