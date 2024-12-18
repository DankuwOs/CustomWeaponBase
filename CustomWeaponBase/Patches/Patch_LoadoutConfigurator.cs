using System.Collections.Generic;
using System.Linq;
using CustomWeaponBase;
using HarmonyLib;
using UnityEngine;
using Valve.Newtonsoft.Json.Linq;
using VTOLVR.Multiplayer;


[HarmonyPatch(typeof(LoadoutConfigurator), nameof(LoadoutConfigurator.Initialize))]
public class Patch_LoadoutConfigurator
{ 
    public static List<int> NewHardpoints = new List<int>();

    public static Vector3 spPosition = new Vector3(347.399994f, -41.7999992f, -2);

    public static Vector3 mpPosition = new Vector3(325.799988f, -108.5f, -2.4000001f);

    [HarmonyPostfix]
    public static void Postfix(LoadoutConfigurator __instance)
    {
        // Most of this code is based on Temperz87's NotBDArmory: https://github.com/Temperz87/NotBDArmory
        //Traverse traverse = Traverse.Create(__instance);
        Dictionary<string, EqInfo> unlockedWeaponPrefabs = __instance.unlockedWeaponPrefabs;
        
        foreach (var weapon in Main.weapons)
        {
            Debug.Log($"[CWB]: Trying to add {weapon.Key.Item1} to configurator.");

            if (unlockedWeaponPrefabs.ContainsKey(weapon.Key.Item1))
            {
                Debug.Log($"[CWB]: Cannot add {weapon.Key.Item1} to configurator as it already exists. '{weapon.Key.Item3}'");
                continue;
            }
            
            var currentVehicle = PilotSaveManager.currentVehicle;
            
            if (weapon.Value is JArray compatNew && !CustomWeaponsBase.CompareCompatNew(compatNew, currentVehicle.vehicleName, weapon.Key.Item2?.GetComponent<HPEquippable>()))
                continue;

            GameObject weaponPrefab = GameObject.Instantiate(weapon.Key.Item2);
            var cwbWeapon = weaponPrefab?.GetComponent<CWB_Weapon>();
            
            if (cwbWeapon && !Main.allowWMDS && cwbWeapon.WMD)
            {
                continue;
            }
 
            
            weaponPrefab.name = weapon.Key.Item1;
            
            if (__instance.IsMultiplayer()) // I THINK THIS IS WHY 0/27 NEED TO TEST OH WOW IM DUMB
            {
                foreach (MissileLauncher missileLauncher in weaponPrefab.GetComponentsInChildrenImplementing<MissileLauncher>(true))
                {
                    if (missileLauncher.loadOnStart)
                    {
                        missileLauncher.LoadAllMissiles();
                    }
                }
            }
            
            weaponPrefab.SetActive(false); // do need?
            
            EqInfo weaponInfo = new EqInfo(weaponPrefab, $"{currentVehicle.equipsResourcePath}/{weapon.Key.Item1}");

            unlockedWeaponPrefabs.Add(weapon.Key.Item1, weaponInfo);
            __instance.availableEquipStrings.Add(weapon.Key.Item1);
        }

        __instance.unlockedWeaponPrefabs = unlockedWeaponPrefabs;
        __instance.allWeaponPrefabs = unlockedWeaponPrefabs;
        if (__instance.recommendedEquipShortnames != null)
        {
            var recommendedEquipShortnames = __instance.recommendedEquipShortnames;
            
            // certainly a smaller or better way to do this but i do not care!
            __instance.recommendedEquipShortnames = new string[__instance.hpNodes.Length];
            for (int i = 0; i < recommendedEquipShortnames.Length; i++)
            {
                if (i < __instance.recommendedEquipShortnames.Length)
                    __instance.recommendedEquipShortnames[i] = recommendedEquipShortnames[i];
            }
        }
    }

    [HarmonyPrefix]
    public static void Prefix(LoadoutConfigurator __instance)
    {
        NewHardpoints = new List<int>();

        int hpCount = Main.extraHps;
        Debug.Log($"[CWB]: Attempting to add {hpCount} hp's");
        for (int i = 0; i < hpCount; i++)
        {
            CreateHardpoint(__instance.hpNodes.Length + 1, __instance);
        }
        
    }

    private static void CreateHardpoint(int idx, LoadoutConfigurator configurator)
    {
        if (configurator.wm.hardpointTransforms.Any(e => !e || !e.parent || e.parent.gameObject.name.Contains("CWBHP")) || NewHardpoints.Contains(idx))
            return;

        var newTransform = new GameObject($"CWBHP_{idx}").transform;
        newTransform.SetParent(configurator.wm.transform);
        newTransform.localPosition = Vector3.zero;
        newTransform.localRotation = Quaternion.identity;

        var tfList = configurator.wm.hardpointTransforms.ToList();
        tfList.Add(newTransform);
        configurator.wm.hardpointTransforms = tfList.ToArray();
        var hpObj = Object.Instantiate(Main.nodeObj, configurator.hpNodes[0].transform.parent);
        hpObj.name = $"HardpointInfo ({idx})";
        if (VTOLMPUtils.IsMultiplayer())
        {
            if (NewHardpoints.Count == 0)
                hpObj.transform.localPosition = mpPosition;
            else
                hpObj.transform.localPosition = mpPosition + new Vector3(0, -40 * NewHardpoints.Count, 0);
        }
        else
        {
            if (NewHardpoints.Count == 0)
                hpObj.transform.localPosition = spPosition;
            else
                hpObj.transform.localPosition = spPosition + new Vector3(0, -40 * NewHardpoints.Count, 0);
        }
        
        hpObj.SetActive(true);
        NewHardpoints.Add(idx);
        
        var nodeList = configurator.hpNodes.ToList();
        
        nodeList.Add(hpObj.GetComponent<HPConfiguratorNode>());
        
        configurator.hpNodes = nodeList.ToArray();
        
        Debug.Log($"[CWB]: Added custom HP {idx}");
    }
}