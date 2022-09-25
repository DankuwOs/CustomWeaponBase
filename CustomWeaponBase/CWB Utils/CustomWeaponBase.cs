
using System;
using CustomWeaponBase;
using UnityEngine;

public class CustomWeaponsBase : MonoBehaviour
{
    public static CustomWeaponsBase instance;

    public WeaponManager MyWeaponManager = null;

    public Loadout Loadout = null;

    private void Start()
    {
        Debug.Log("CWB: Initialized.");
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
        Debug.Log("CWB: REFRESHING WEAPONS, DO NOT FRET, NULLREFS ARE FUNNY");
        if (!Main.instance)
        {
            Debug.Log("what the fuck");
        }

        if (Main.weapons == null)
        {
            Debug.Log("why are the weapons null huh?!?");
        }
        Main.weapons.Clear();
        
        Main.instance.ReloadBundles();
    }
}