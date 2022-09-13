using System.Collections.Generic;
using CustomWeaponBase;
using Harmony;
using UnityEngine;

[HarmonyPatch(typeof(LoadoutConfigurator), nameof(LoadoutConfigurator.Initialize))]
public class Patch_LoadoutConfigurator
{
    [HarmonyPostfix]
    public static void Postfix(LoadoutConfigurator __instance)
    {
        // Most of this code is based on Temperz87's NotBDArmory: https://github.com/Temperz87/NotBDArmory
        Traverse traverse = Traverse.Create(__instance);
        Dictionary<string, EqInfo> unlockedWeaponPrefabs = (Dictionary<string, EqInfo>)traverse.Field("unlockedWeaponPrefabs").GetValue();
        foreach (var weapon in Main.weapons)
        {
            Debug.Log($"Trying to add {weapon.Key.Item1} to configurator.");
            var currentVehicle = PilotSaveManager.currentVehicle;
            if (VehicleCompatibility.CompareTo(weapon.Value, VTOLAPI.GetPlayersVehicleEnum()))
            {
                GameObject weaponPrefab = GameObject.Instantiate(weapon.Key.Item2);
                weaponPrefab.name = weapon.Key.Item1;
                EqInfo weaponInfo = new EqInfo(weaponPrefab, $"{currentVehicle.equipsResourcePath}/{weapon.Key.Item1}");
                weaponPrefab.SetActive(false);

                unlockedWeaponPrefabs.Add(weapon.Key.Item1, weaponInfo);
                __instance.availableEquipStrings.Add(weapon.Key.Item1);
            }
        }
        
        traverse.Field("unlockedWeaponPrefabs").SetValue(unlockedWeaponPrefabs);
        traverse.Field("allWeaponPrefabs").SetValue(unlockedWeaponPrefabs);
    }
}