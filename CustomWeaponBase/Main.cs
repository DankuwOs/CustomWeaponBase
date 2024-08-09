using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CustomWeaponBase.CWB_Utils;
using HarmonyLib;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;
using SteamQueries.Models;
using VTOLAPI;
using UnityEngine;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Linq;
using VTNetworking;

namespace CustomWeaponBase;

public class CWBSettings
{
    public class PackToLoad(string packName, bool loadPack = true)
    {
        public string name = packName;
        public bool loadThisPack = loadPack;
    }

    public List<PackToLoad> packs;
}

public class CWBPack
{
    public struct PackEquip
    {
        public List<string> resourcePaths;
        public string missileResourcePath;
    }
    public string name;
    
    public SteamItem item;

    public List<PackEquip> equips;
}


[ItemId("danku-cwb")]
public class Main : VtolMod
{
    // Some of this code is based on Temperz87's NotBDArmory: https://github.com/Temperz87/NotBDArmory

    /// <summary>
    /// Tuple(Item1 = WeaponName, Item2 = WeaponObject, Item3 = FileName), Compatability
    /// </summary>
    public static Dictionary<Tuple<string, GameObject, string>, object> weapons = new(); // realize now that tuples can have more than two im very smart
    

    //public List<Tuple<string, CWBPack>> assetBundles;

    public List<CWBPack> cwbPacks = new();

    public static bool allowWMDS = true;

    public static Main instance;

    public static GameObject nodeObj;

    public static int extraHps = 4;

    public string ModFolder;
    

    private List<SteamItem> _allItems;

    private static CWBSettings CWBSettings;
        
    public void Start()
    {
        ModFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        
        GetSettings();

        GameObject cwb = new GameObject("Custom Weapons Base", typeof(CustomWeaponsBase));
        DontDestroyOnLoad(cwb);
        instance = this;
        
        
        nodeObj = FileLoader.GetAssetBundleAsGameObject($"{ModFolder}/node.splooge", "NodeTemplate");
        Debug.Log($"Got nodeObj {nodeObj}");
        nodeObj.SetActive(false);
        DontDestroyOnLoad(nodeObj); // Fix game crashing after leaving and going back to the place
        
        VTAPI.SceneLoaded += delegate
        {
            CustomWeaponsBase.instance.CheckVehicleListChanged(VTResources.GetPlayerVehicleList());
        };

        GetSteamItems();
        
        GetCWBPacks();
    }

    public void ReloadPacks()
    {
        weapons?.Clear();
        
        foreach (var cwbPack in cwbPacks.ToArray())
        {
            UnloadPack(cwbPack);
        }
        
        GetSteamItems();
        
        GetCWBPacks();
    }


    private void GetSteamItems()
    {
        List<SteamItem> items = new List<SteamItem>();
        var steamItems = VTAPI.instance.FindSteamItems();
        foreach (var steamItem in steamItems)
        {
            Debug.Log($"[CWB]: Found SteamItem {steamItem.Title}");
        }
        items.AddRange(VTAPI.instance.FindSteamItems());
        var localItems = VTAPI.instance.FindLocalItems();
        foreach (var localItem in localItems)
        {
            Debug.Log($"[CWB]: Found Local SteamItem {localItem.Title}");
        }
        items.AddRange(localItems);
        
        _allItems = items;
    }

    private void GetCWBPacks(bool ignoreSetting = false)
    {
        
        Debug.Log($"[CWB]: Getting CWB mods..");

        foreach (var steamItem in _allItems)
        {
            var files = Directory.GetFiles(steamItem.Directory, "*.cwb");
            if (files.Length == 0)
                continue;
            Debug.Log($"[CWB]: Loading CWB Files from '{steamItem.Directory}'");
            LoadPack(steamItem, !ignoreSetting);
        }

        var streamingAssetsPath = Path.Combine(ModFolder, "StreamingAssets");
        var cwbFiles = Directory.GetFiles(streamingAssetsPath, "*.cwb");
        foreach (var cwbFile in cwbFiles)
        {
            Debug.Log($"[CWB]: Loading StreamingAssets CWB File '{Path.GetFileName(cwbFile)}'");
            LoadPack(new FileInfo(cwbFile), null, !ignoreSetting);
        }
        
        SaveSettings();
    }

    public void LoadPackForName(string title, bool useSetting = true)
    {
        Debug.Log($"[CWB]: Trying to load pack for '{title}'");
        var item = _allItems.Find(item => item.Title == title);
        
        if (item == null)
        {
            Debug.Log($"[CWB]: Couldn't get SteamItem for '{title}'");
            return;
        }
        
        LoadPack(item, useSetting);
        SaveSettings();
    }

    private void LoadPack(SteamItem item, bool useSetting = false)
    {
        
        if (VTAPI.instance.IsItemLoaded(item.Directory))
            Debug.Log($"[CWB]: '{item.Title}' Is already loaded, ignoring dll.");
        else
            VTAPI.instance.LoadSteamItem(item);

        var files = Directory.GetFiles(item.Directory, "*.cwb");

        Debug.Log($"[CWB]: Found {files.Length} cwb files in '{item.Directory}'");
        foreach (var file in files)
        {
            LoadPack(new FileInfo(file), item, useSetting);
        }
    }

    private void LoadPack(FileInfo file, SteamItem item = null, bool useSetting = false)
    {

        if (CWBSettings.packs == null)
            throw new NullReferenceException("[CWB]: CWBSettings packs list is NULL!!!");
        
        if (CWBSettings.packs.All(pack => pack != null && pack.name != file.Name))
            CWBSettings.packs.Add(new CWBSettings.PackToLoad(file.Name)); // Add a new pack to the list 
        
        else if (useSetting && !CWBSettings.packs.Find(pack => pack.name == file.Name).loadThisPack)
        {
            Debug.Log($"[CWB]: Settings file says not to load '{item?.Title + (item == null ? "" : "\\")}{file.Name}'");
            return;
        }

        if (cwbPacks.Any(pack => pack.name == file.Name))
        {
            Debug.LogError($"[CWB]: Pack {file.Name} is already loaded, skipping.");
            return;
        }

        Debug.Log($"[CWB]: Loading CWB Pack @ '{item?.Title + (item == null ? "" : "\\")}{file.Name}'");
        
        var cwbPack = new CWBPack
        {
            item = item,
            name = file.Name,
            equips = new List<CWBPack.PackEquip>()
        };
        
        cwbPacks.Add(cwbPack);
        
        StartCoroutine(LoadStreamedWeapons(file, cwbPack));
    }

    private IEnumerator LoadStreamedWeapons(FileInfo info, CWBPack pack)
    {
        // Might happen who knows.
        var existingBundle = AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault(bundle => bundle.name == info.Name);
        if (existingBundle)
        {
            Debug.Log($"[CWB]: Unloading existing bundle '{existingBundle.name}'");
            //assetBundles.RemoveAll(a => a.Item1 == existingBundle.name);
            existingBundle.Unload(true);
        }
        
        
        var assetBundle = AssetBundle.LoadFromFile(info.FullName);
        
        Debug.Log($"[CWB]: Loaded asset bundle '{assetBundle.name}' for file '{info.Name}'");

        if (assetBundle)
        {
            var manifest = assetBundle.LoadAsset<TextAsset>("manifest.json");
            if (manifest == null)
            {
                Debug.LogError("[CWB]: Couldn't find manifest.json in " + info.Name);
                yield break;
            }
            
            JObject jManifest = JsonConvert.DeserializeObject<JObject>(manifest.text);
            
            if (jManifest["DevDependency"] != null)
            {
                string devDependency = (string)jManifest["DevDependency"];
                if (Directory.Exists(devDependency))
                {
                    Debug.Log($"[CWB]: Trying to load dev dependency @ {devDependency}");
                    try
                    {
                        var steamItem = _allItems.Find(item => Path.GetFullPath(item.Directory) == Path.GetFullPath(devDependency));
                        if (steamItem != null)
                        {
                            pack.item = steamItem;
                            if (!VTAPI.instance.IsItemLoaded(steamItem.Directory))
                                VTAPI.instance.LoadSteamItem(steamItem);
                        }
                        
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[CWB]: Couldn't load dev dependency @ {devDependency}. {e}");
                        throw;
                    }
                }
                else if (!string.IsNullOrEmpty(devDependency))
                {
                    Debug.Log($"[CWB]: Couldn't find dev dependency @ {devDependency} (Empty line, not an issue.)");
                }
            }
            
            Dictionary<string, object> jsonWeapons = jManifest["Weapons"]?.ToObject<Dictionary<string, object>>();
            if (jsonWeapons != null)
            {
                foreach (var weapon in jsonWeapons)
                {
                    GameObject requestWeapon = assetBundle.LoadAsset<GameObject>(weapon.Key + ".prefab");

                    if (requestWeapon == null)
                    {
                        Debug.Log(
                            $"[CWB]: Couldn't load asset {weapon.Key}, make sure the prefab is included in the AB and built.");
                        continue;
                    }

                    RegisterWeapon(requestWeapon, weapon.Key, weapon.Value, pack);
                }
            }

            assetBundle.Unload(false); // Unload the bundle so that you can overwrite the file.
        }
        else
        {
            Debug.Log($"[CWB]: Couldn't load streamed bundle {info.FullName}");
        }
    }


    private void RegisterWeapon(GameObject equip, string weaponName, object compatability, CWBPack pack)
    {
        Debug.Log($"[CWB]: Registering weapon: {weaponName}");
        
        CWBPack.PackEquip thisEquip = new CWBPack.PackEquip();
        List<string> resourcePaths = new List<string>();

        equip.name = weaponName;
        DontDestroyOnLoad(equip);
        
        CWB_Weapon cwbWeapon = equip.GetComponent<CWB_Weapon>();
        if (!cwbWeapon)
            cwbWeapon = equip.AddComponent<CWB_Weapon>();
        cwbWeapon.bundleName = pack.name;

        foreach (AudioSource source in equip.GetComponentsInChildren<AudioSource>(true))
        {
            if (!source.outputAudioMixerGroup)
                source.outputAudioMixerGroup = VTResources.GetExteriorMixerGroup();
        }

        HPEquipMissileLauncher launcher = equip.GetComponent<HPEquipMissileLauncher>();

        if (launcher)
        {
            switch (launcher)
            {
                case HPEquipIRML launcherIrml:
                    launcherIrml.irml = launcherIrml.ml as IRMissileLauncher;
                    break;
                
                case HPEquipOpticalML launcherOpticalMl:
                    launcherOpticalMl.oml = launcherOpticalMl.ml as OpticalMissileLauncher;
                    break;
            }

            if (launcher.ml.missilePrefab && VTResources.Load<GameObject>(launcher.missileResourcePath) == null)
            {
                RegisterMissile(launcher.ml.missilePrefab, launcher.missileResourcePath, pack);
                
                thisEquip.missileResourcePath = launcher.missileResourcePath; // Add missile resource path to remove when unloading the pack.
            }
        }
        
        var pvList = VTResources.GetPlayerVehicleList();

        bool WeaponCompat(PlayerVehicle playerVehicle) // Made this a local function since doing it in the pvList.Where is kinda ugly
        {
            if (compatability is JArray)
            {
                return CustomWeaponsBase.CompareCompatNew(compatability, playerVehicle.vehicleName, equip.GetComponent<HPEquippable>());
            }
            return false;
        }

        foreach (var playerVehicle in pvList.Where(WeaponCompat))
        {
            VTResources.RegisterOverriddenResource($"{playerVehicle.equipsResourcePath}/{weaponName}", equip);
            VTNetworkManager.RegisterOverrideResource($"{playerVehicle.equipsResourcePath}/{weaponName}", equip);
            
            resourcePaths.Add($"{playerVehicle.equipsResourcePath}/{weaponName}");
            
        }

        weapons.Add(Tuple.Create(weaponName, equip, pack.name), compatability);
        
        equip.SetActive(false);

        pack.equips ??= new List<CWBPack.PackEquip>();
        thisEquip.resourcePaths = resourcePaths;
        pack.equips.Add(thisEquip);
    }

    public void RegisterMissile(GameObject missile, string resourcePath, CWBPack pack)
    {
        var missileComponent = missile.AddComponent<CWB_Weapon>();
        missileComponent.bundleName = pack.name;
        VTResources.RegisterOverriddenResource(resourcePath, missile);
        VTNetworkManager.RegisterOverrideResource(resourcePath, missile);
        missile.SetActive(false);
    }

    public void DestroyObjects(CWB_Weapon[] gameObjects)
    {
        foreach (var o in gameObjects)
        {
            try
            {
                Debug.Log($"[CWB]: Destroying obj: {o.gameObject.name}");

                var equip = o.gameObject.GetComponent<HPEquippable>();
                if (equip && equip.isActiveAndEnabled)
                    equip.OnUnequip();
                Destroy(o.gameObject);
            }
            catch (Exception e)
            {
                Debug.LogError($"[CWB]: Got an error when destroying an object {e}");
            }
        }
    }
    
    public void UnloadPack(CWBPack pack, bool fromDll = false)
    {
        foreach (var equip in pack.equips)
        {
            if (equip.missileResourcePath == null && equip.resourcePaths == null)
                Debug.LogError($"[CWB]: '{pack.name}' Contains an empty equip, whats happening?");
            
            if (equip.resourcePaths != null)
            {
                foreach (var eqPath in equip.resourcePaths)
                {
                    VTResources.ResetOverriddenResource(eqPath);
                    VTNetworkManager.overriddenResources.Remove(eqPath);
                }
            }
            if (equip.missileResourcePath != null)
            {
                VTResources.ResetOverriddenResource(equip.missileResourcePath);
                VTNetworkManager.overriddenResources.Remove(equip.missileResourcePath);
            }
        }
        if (pack.item != null && VTAPI.instance.IsItemLoaded(pack.item.Directory) && !fromDll)
            VTAPI.instance.DisableSteamItem(pack.item); // Unload possible dll.

        var weaponsToRemove = weapons.Where(weaponPack => weaponPack.Key.Item3 == pack.name);

        foreach (var keyValuePair in weaponsToRemove.ToArray())
        {
            Debug.Log($"[CWB]: Removing '{keyValuePair.Key.Item1}' from the weapons list.");
            Destroy(keyValuePair.Key.Item2);
            weapons.Remove(keyValuePair.Key);
        }

        var cwbWeapons = FindObjectsOfType<CWB_Weapon>(true);

        cwbWeapons.DoIf(weapon => weapon.bundleName == pack.name,
            weapon =>
            {
                var equip = weapon.GetComponent<HPEquippable>();
                if (equip && equip.isActiveAndEnabled && equip.weaponManager)
                    equip.OnUnequip();
                Destroy(weapon);
            });
        
        
        if (cwbPacks.Contains(pack))
            cwbPacks.Remove(pack);
    }

    public void UnloadPackForName(string title) // Used by DLL mods to tell CWB to unload the pack(s)
    {
        var item = _allItems.Find(item => item.Title == title);
        var packs = cwbPacks.ToArray().Where(pack => pack.item != null && pack.item.Directory == item.Directory);
        
        foreach (var cwbPack in packs)
        {
            UnloadPack(cwbPack, true);
        }
    }

    public override void UnLoad()
    {
        SaveSettings();
        
        if (cwbPacks != null)
        {
            foreach (var cwbPack in cwbPacks.ToArray())
            {
                UnloadPack(cwbPack);
            }
        }

        if (nodeObj)
            Destroy(nodeObj);
        
        if (CustomWeaponsBase.instance.gameObject)
            Destroy(CustomWeaponsBase.instance.gameObject);
        
    }

    public void GetSettings()
    {
        var filePath = Path.Combine(ModFolder, "loaditems.json");

        if (!File.Exists(filePath))
            SaveSettings();

        try
        {

            var contents = File.ReadAllText(Path.Combine(ModFolder, "loaditems.json"));
            
            if (string.IsNullOrEmpty(contents))
            {
                Debug.LogError($"[CWB]: Contents of loaditems.json is empty, creating an empty one.");
                SaveSettings();
            }

            CWBSettings = JsonConvert.DeserializeObject<CWBSettings>(contents);
            CWBSettings.packs ??= new List<CWBSettings.PackToLoad>();
        }
        catch (Exception e)
        {
            Debug.LogError($"[CWB]: Error when getting settings.. {e}");
            throw;
        }
    }

    public void SaveSettings()
    {
        CWBSettings ??= new CWBSettings()
        {
            packs = new List<CWBSettings.PackToLoad>(){}
        };
        
        File.WriteAllText(Path.Combine(ModFolder, "loaditems.json"), JsonConvert.SerializeObject(CWBSettings, Formatting.Indented));
    }

    public string GetDirectoryForName(string name)
    {
        return _allItems.Find(item => item.Title == name).Directory;
    }
}