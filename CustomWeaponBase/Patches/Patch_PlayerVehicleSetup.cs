
using System.Linq;
using CustomWeaponBase;
using Harmony;
using UnityEngine;

[HarmonyPatch(typeof(PlayerVehicleSetup), nameof(PlayerVehicleSetup.SetupForFlight))]
public class Patch_PlayerVehicleSetup
{
    public static void Postfix(PlayerVehicleSetup __instance)
    {
        if (VehicleEquipper.loadoutSet)
        {
            CustomWeaponsBase.instance.Loadout = VehicleEquipper.loadout;
        }
    }

    public static void Prefix(PlayerVehicleSetup __instance)
    {
        var weaponManager = __instance.GetComponent<WeaponManager>();
        
        var wm = Traverse.Create(weaponManager);
        if (wm.Field("equips").GetValue<HPEquippable[]>().Any(e => e.gameObject.name.Contains("CWBHB")))
            return;
        
        Debug.Log($"[CWB]: Compare hp loadout to hptfs: {VehicleEquipper.loadout.hpLoadout.Length} | {weaponManager.hardpointTransforms.Length}");
        for (int i = 0; i < 3; i++)
        {
            var newTransform = new GameObject($"CWBHP_{weaponManager.hardpointTransforms.Length + 1}").transform;
            newTransform.SetParent(__instance.transform);
            newTransform.localPosition = Vector3.zero;
            newTransform.localRotation = Quaternion.identity;

            var tfList = weaponManager.hardpointTransforms.ToList();
            tfList.Add(newTransform);
            weaponManager.hardpointTransforms = tfList.ToArray();

            
            wm.Field("equips").SetValue(new HPEquippable[weaponManager.hardpointTransforms.Length]);
        }
    }
}