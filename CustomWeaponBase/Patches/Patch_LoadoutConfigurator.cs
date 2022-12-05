using System;
using System.Collections.Generic;
using System.Linq;
using CustomWeaponBase;
using Harmony;
using UnityEngine;
using UnityEngine.UI;
using VTOLVR.Multiplayer;
using Object = UnityEngine.Object;

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
        Traverse traverse = Traverse.Create(__instance);
        Dictionary<string, EqInfo> unlockedWeaponPrefabs = (Dictionary<string, EqInfo>)traverse.Field("unlockedWeaponPrefabs").GetValue();
        
        foreach (var weapon in Main.weapons)
        {
            Debug.Log($"[CWB]: Trying to add {weapon.Key.Item1} to configurator.");
            
            var currentVehicle = PilotSaveManager.currentVehicle;
            if (!CustomWeaponsBase.CompareCompat(weapon.Value, currentVehicle.vehicleName)) continue;

            GameObject weaponPrefab = GameObject.Instantiate(weapon.Key.Item2);

            var cwbWeapon = weaponPrefab.GetComponent<CWB_Weapon>();
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
        
        
        traverse.Field("unlockedWeaponPrefabs").SetValue(unlockedWeaponPrefabs);
        traverse.Field("allWeaponPrefabs").SetValue(unlockedWeaponPrefabs);
    }

    [HarmonyPrefix]
    public static void Prefix(LoadoutConfigurator __instance)
    {
        NewHardpoints = new List<int>();

        int hpCount = 3;
        Debug.Log($"[CWB]: Attempting to add {hpCount} hp's");
        for (int i = 0; i < hpCount; i++)
        {
            CreateHardpoint(__instance.hpNodes.Length + 1, __instance);
        }
        
    }

    private static void CreateHardpoint(int idx, LoadoutConfigurator configurator)
    {
        if (NewHardpoints.Contains(idx))
            return;

        var newTransform = new GameObject($"HP_{idx}").transform;
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