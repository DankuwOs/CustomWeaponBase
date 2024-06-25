using HarmonyLib;


[HarmonyPatch(typeof(HPEquipIRML), "OnEquip")]
public class Patch_HPEquipIRML0
{
    public static void Prefix(HPEquipIRML __instance)
    {
        __instance.irml = __instance.ml as IRMissileLauncher;  // WHY DO I NEED TO DO THIS~!?!?!??!?!3/42/l34u,65iujw4njt5n
    }
}

[HarmonyPatch(typeof(HPEquipIRML), "Awake")]
public class Patch_HPEquipIRML1
{
    public static void Prefix(HPEquipIRML __instance)
    {
        __instance.irml = __instance.ml as IRMissileLauncher;
    }
}