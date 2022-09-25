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
            Debug.Log($"Trying to add {weapon.Key.Item1} to configurator.");
            var currentVehicle = PilotSaveManager.currentVehicle;
            if (VehicleCompatibility.CompareTo(weapon.Value, VTOLAPI.GetPlayersVehicleEnum()))
            {
                GameObject weaponPrefab = GameObject.Instantiate(weapon.Key.Item2);
                if (!Main.allowWMDS && weaponPrefab.GetComponent<CWB_Weapon>().WMD)
                {
                    weaponPrefab.SetActive(false);
                    continue;
                }

                
                
                var cwbWeapon = weaponPrefab.GetComponent<CWB_Weapon>();
                if (cwbWeapon)
                {
                    if (!Main.allowWMDS && cwbWeapon.WMD)
                        continue;
                }

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

    [HarmonyPrefix]
    public static void Prefix(LoadoutConfigurator __instance)
    {
        
        foreach (var weapon in Main.weapons)
        {
            if (VehicleCompatibility.CompareTo(weapon.Value, VTOLAPI.GetPlayersVehicleEnum()))
            {
                GameObject weaponPrefab = weapon.Key.Item2;
                if (!Main.allowWMDS && weaponPrefab.GetComponent<CWB_Weapon>().WMD)
                {
                    weaponPrefab.SetActive(false);
                    continue;
                }
                var cwbWeapon = weaponPrefab.GetComponent<CWB_Weapon>();
                if (cwbWeapon)
                {
                    if (!Main.allowWMDS && cwbWeapon.WMD)
                        continue;
                    
                    if (cwbWeapon.newHardpoints == "")
                        continue;
                    
                    var hps = cwbWeapon.newHardpoints.Split(new char[]
                    {
                        ','
                    }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var hp in hps)
                    {
                        int nodeCount = __instance.hpNodes.Length;
                        var hpInt = int.Parse(hp);
                        
                        if (hpInt < nodeCount || NewHardpoints.Contains(hpInt))
                            continue;

                        if (hpInt > nodeCount)
                        {
                            Debug.Log($"[Hardpoint Warning] hpInt for {weaponPrefab.name} was 2 or more than array length.. please fix.");
                            hpInt = nodeCount; // Any value 2 or more above current array length will be bad..
                            
                            var equippable = weaponPrefab.GetComponent<HPEquippable>();
                            if (equippable)
                                equippable.allowedHardpoints.Replace(hp, hpInt.ToString());
                        }

                        Debug.Log($"[Hardpoint] Trying to add new hardpoint {hpInt} to vehicle {__instance.wm.gameObject.name} for {weaponPrefab.name} with allowed hps {weaponPrefab.GetComponent<HPEquippable>().allowedHardpoints}");
                        CreateHardpoint(hpInt, __instance);
                    }
                }
            }
        }
    }

    public static void CreateHardpoint(int idx, LoadoutConfigurator configurator)
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
        
        

        /*if (!configurator.CanEquipIdx(idx))
        {  
            Debug.Log($"Cannot equip on idx: {idx}! Fix your shit!!");
        }*/

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
    }
}