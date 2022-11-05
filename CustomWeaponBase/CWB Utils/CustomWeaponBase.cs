
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
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

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.M))
        {
            if (!CameraFollowMe.instance)
            {
                Debug.Log("[CWB] Didn't find CameraFollowMe instance.");
            }
            
            Debug.Log("[CWB] LM");
            var idx = Traverse.Create(CameraFollowMe.instance).Field("idx");
            Debug.Log("idx");
            var wm = VTOLAPI.GetPlayersVehicleGameObject().GetComponent<WeaponManager>();
            if (!wm)
                return;

            Debug.Log("wm");
            var lastMissile = wm.lastFiredMissile;
            Debug.Log($"last missile: {lastMissile}");

            int lastMissileIdx = 0;
            if (CameraFollowMe.instance.targets.Contains(lastMissile.actor))
            {
                Debug.Log("contains lm");
                lastMissileIdx = CameraFollowMe.instance.targets.IndexOf(lastMissile.actor);
            }
            else
            {
                CameraFollowMe.instance.AddTarget(lastMissile.actor);
                lastMissileIdx = CameraFollowMe.instance.targets.IndexOf(lastMissile.actor);
            }

            Debug.Log($"last missile idx: {lastMissileIdx}");
            CameraFollowMe.instance.SetTargetDebug(false);
            idx.SetValue(lastMissileIdx);
            CameraFollowMe.instance.SetTargetDebug(true);
            Debug.Log($"fin");
        }
        
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.N))
        {
            if (!CameraFollowMe.instance)
            {
                Debug.Log("[CWB] Didn't find CameraFollowMe instance.");
            }

            var instance = Traverse.Create(CameraFollowMe.instance);
            
            Debug.Log("[CWB] LM");
            var idx = instance.Field("idx");
            Debug.Log($"idx {idx.GetValue()}");
            
            
            var wm = ((Transform)instance.Field("currentTarget").GetValue()).GetComponent<WeaponManager>();
            
            if (!wm)
                return;

            Debug.Log("wm");
            var lastMissile = wm.lastFiredMissile;
            Debug.Log($"last missile: {lastMissile}");

            int lastMissileIdx = 0;
            if (CameraFollowMe.instance.targets.Contains(lastMissile.actor))
            {
                Debug.Log("contains lm");
                lastMissileIdx = CameraFollowMe.instance.targets.IndexOf(lastMissile.actor);
            }
            else
            {
                CameraFollowMe.instance.AddTarget(lastMissile.actor);
                lastMissileIdx = CameraFollowMe.instance.targets.IndexOf(lastMissile.actor);
            }

            Debug.Log($"last missile idx: {lastMissileIdx}");
            CameraFollowMe.instance.SetTargetDebug(false);
            idx.SetValue(lastMissileIdx);
            CameraFollowMe.instance.SetTargetDebug(true);
            Debug.Log($"fin");
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.PageUp))
        {
            var playersVehicleGameObject = VTOLAPI.GetPlayersVehicleGameObject();
            if (!playersVehicleGameObject)
            {
                Debug.Log("No player vehicle found..");
                return;
            }

            var wm = playersVehicleGameObject.GetComponent<WeaponManager>();
            if (!wm)
            {
                Debug.Log("no wm");
                return;
            }
            
            for (int i = 0; i < wm.equipCount; i++)
            {
                var equip = wm.GetEquip(i);
                
                var liveryMesh = equip.GetComponent<LiveryMesh>();
                Debug.Log("a");
                if (liveryMesh && liveryMesh.copyMaterial && equip.weaponManager)
                {
                    Debug.Log("b");
                    var objectPaths = liveryMesh.materialPath.Split('/');
            Debug.Log("j");
                    var obj = objectPaths.Aggregate(equip.weaponManager.transform, (current, path) => current.Find(path));
Debug.Log("e");
                    var renderer = obj.GetComponent<Renderer>();
Debug.Log("AAAAAAAAAAAAA");
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(block);
Debug.Log("pssssssss");
                    foreach (var mesh in liveryMesh.liveryMeshs)
                    {
                        Debug.Log("im snaek");
                        mesh.material = renderer.materials[0];
                        mesh.SetPropertyBlock(block);
                    }

                    return;
                }
                Debug.Log($"Trying to match liveries for {equip.shortName}");
                Debug.Log(wm.liverySample);
                Debug.Log("ha");
                if (equip.matchLiveries != null && wm.liverySample != null)
                {
                    Debug.Log("1");
                    MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
                    Debug.Log("2");
                    wm.liverySample.GetPropertyBlock(materialPropertyBlock);
                    Debug.Log("3");
                    MeshRenderer[] array = equip.matchLiveries;
                    Debug.Log("4");
                    for (int i1 = 0; i1 < array.Length; i1++)
                    {
                        Debug.Log("5");
                        array[i1].SetPropertyBlock(materialPropertyBlock);
                    }
                }
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