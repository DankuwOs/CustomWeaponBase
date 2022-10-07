
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

    public Texture2D GetAircraftLivery(GameObject baseObject)
    {
        var wm = baseObject.GetComponentInParent<WeaponManager>();

        if (!wm)
            return null;

        var aircraft = wm.gameObject;

        if (aircraft.name.Contains("FA-26B"))
            return aircraft.transform.Find("aFighter2").Find("body").GetComponent<MeshRenderer>().material.GetTexture("_Livery") as Texture2D;
        if (aircraft.name.Contains("SEVTF"))
            return aircraft.transform.Find("sevtf_layer_2").Find("body.001").GetComponent<MeshRenderer>().material.GetTexture("_Livery") as Texture2D;
        if (aircraft.name.Contains("AH-94"))
            return aircraft.transform.Find("sevtf_layer_2").Find("body.001").GetComponent<MeshRenderer>().material.GetTexture("_Livery") as Texture2D;
        if (aircraft.name.Contains("VTOL4"))
            return aircraft.transform.Find("VT4Body(new)").Find("body_main").GetComponent<MeshRenderer>().material.GetTexture("_Livery") as Texture2D;

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