
using System;
using System.Collections.Generic;
using CustomWeaponBase;
using UnityEngine;

public class CustomWeaponsBase : MonoBehaviour
{
    public static CustomWeaponsBase instance;

    public WeaponManager MyWeaponManager = null;

    public Loadout Loadout = null;

    public List<PlayerVehicle> PlayerVehicles;

    private void Start()
    {
        Debug.Log("[CWB]: Initialized.");
        instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            RefreshAllWeapons();
        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            ReloadWeapons();
        }
    }

    public void ReloadWeapons()
    {
        if (Loadout == null)
        {
            Debug.LogError($"CWB ReloadWeapons() Loadout null");
            
            return;
        }

        if (!MyWeaponManager)
            MyWeaponManager = VTOLAPI.GetPlayersVehicleGameObject().GetComponent<WeaponManager>();

        if (MyWeaponManager == null)
        {
            Debug.LogError($"CWB ReloadWeapons() WM null");
            return;
        }

        MyWeaponManager.EquipWeapons(Loadout);
    }

    public void RefreshAllWeapons()
    {
        Debug.Log("[CWB]: REFRESHING WEAPONS, DO NOT FRET, NULLREFS ARE FUNNY");
        if (!Main.instance)
        {
            Debug.Log("what the fuck");
        }

        if (Main.weapons == null)
        {
            Debug.Log("why are the weapons null huh?!?");
        }

        Main.weapons?.Clear();

        Main.instance.ReloadBundles();
    }

    public Texture2D GetAircraftLivery(HPEquippable equip)
    {
        if (!equip)
            return null;
        
        var wm = equip.weaponManager;

        if (wm)
        {
            Texture2D livery;
                
            livery = wm.liverySample.material.GetTexture("_Livery") as Texture2D;
            
            var perBiome = wm.GetComponent<PerBiomeLivery>();
            MapGenBiome.Biomes currBiome = MapGenBiome.Biomes.Boreal;
            if (VTMapGenerator.fetch)
            {
                currBiome = VTMapGenerator.fetch.biome;
            }
            foreach (var biome in perBiome.liveries)
            {
                if (biome.biome == currBiome)
                {
                    livery = biome.livery;
                    break;
                }
            }

            if (livery)
                return livery;
            
            Debug.Log($"Couldn't find a livery sad");
            return null;
        }
        return null;
    }

    public static bool CompareCompat(string compat, string vehicle, string weapon = "")
    {
        var compats = compat.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);
        foreach (var s in compats)
        {
            if (vehicle.Contains(s))
                return true;
        }

        return false;
    }

    public void CheckVehicleListChanged(List<PlayerVehicle> newList)
    {
        PlayerVehicles ??= VTResources.GetPlayerVehicleList();

        if (newList.Count > PlayerVehicles.Count)
        {
            RefreshAllWeapons();
        }
    }
}