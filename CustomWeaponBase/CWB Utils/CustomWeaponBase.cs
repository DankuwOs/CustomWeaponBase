
using System;
using System.Collections.Generic;
using System.Linq;
using CustomWeaponBase;
using Harmony;
using UnityEngine;

public class CustomWeaponsBase : MonoBehaviour
{
    public static CustomWeaponsBase instance;

    public WeaponManager MyWeaponManager = null;

    public Loadout Loadout = null;

    public List<PlayerVehicle> PlayerVehicles;

    public static List<GameObject> DetachedObjects = new List<GameObject>();

    public GameObject FindWeaponObject;

    private void Start()
    {
        Debug.Log("[CWB]: Initialized.");
        instance = this;
    }

    public void AddObject(GameObject gameObject) => DetachedObjects.Add(gameObject);
    public void AddObjects(GameObject[] gameObjects) => DetachedObjects.Add(gameObjects);

    public void DestroyDetachedObjects() => DetachedObjects.ForEach(o => Destroy(o));

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

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.L))
        {
            if (FindWeaponObject)
                Destroy(FindWeaponObject);
            else
            {
                var playersVehicle = VTOLAPI.GetPlayersVehicleGameObject();
                if (!playersVehicle)
                    return;
                var wm = playersVehicle.GetComponent<WeaponManager>();
                if (!wm)
                    return;
                

                var currentEquip = wm.currentEquip;
                if (!currentEquip)
                    return;

                FindWeaponObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                FindWeaponObject.transform.SetParent(currentEquip.transform);
                FindWeaponObject.transform.localPosition = Vector3.zero;
                FindWeaponObject.transform.localScale = new Vector3(1, 10, 1);
            }
        }

        if (Input.GetKey(KeyCode.L) && Input.GetKeyDown(KeyCode.M) && CameraFollowMe.instance)
        {
            var idx = Traverse.Create(CameraFollowMe.instance).Field("idx");
            var wm = VTOLAPI.GetPlayersVehicleGameObject().GetComponent<WeaponManager>();
            if (!wm)
                return;

            var lastMissile = wm.lastFiredMissile;
            if (CameraFollowMe.instance.targets.Contains(lastMissile.actor))
            {
                var lastMissileIdx = CameraFollowMe.instance.targets.IndexOf(lastMissile.actor);
                CameraFollowMe.instance.SetTargetDebug(false);
                idx.SetValue(lastMissileIdx);
                CameraFollowMe.instance.SetTargetDebug(true);
            }
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