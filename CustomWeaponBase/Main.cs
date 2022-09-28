using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony;
using UnityEngine;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Linq;
using VTNetworking;

namespace CustomWeaponBase
{
    public class Main : VTOLMOD
    {
        // Most of this code is based on Temperz87's NotBDArmory: https://github.com/Temperz87/NotBDArmory

        private readonly string AH94 = "HPEquips/AH-94";
        private readonly string FA26 = "HPEquips/AFighter";
        private readonly string F45  = "HPEquips/F45A";
        private readonly string AV42 = "HPEquips/VTOL";

        public static Dictionary<Tuple<string, GameObject>, VehicleCompat> weapons = new Dictionary<Tuple<string, GameObject>, VehicleCompat>();

        public List<AssetBundle> AssetBundles;
        

        public static bool allowWMDS = true;

        public static Main instance;

        public static GameObject nodeObj;

        public override void ModLoaded()
        {
            HarmonyInstance instance = HarmonyInstance.Create("danku.cwb");
            instance.PatchAll(Assembly.GetExecutingAssembly());
            
            base.ModLoaded();
            StartCoroutine(LoadCustomCustomBundlesAsync(false));

            GameObject cwb = new GameObject("Custom Weapons Base", typeof(CustomWeaponsBase));
            DontDestroyOnLoad(cwb);
            Main.instance = this;
            
            nodeObj = CWB_Utils.FileLoader.GetAssetBundleAsGameObject($"{ModFolder}/node.splooge", "NodeTemplate");
        }

        public void ReloadBundles()
        {
            var cwbWeapons = FindObjectsOfType<CWB_Weapon>(true);
            if (cwbWeapons.Length > 0)
                DestroyObjects(cwbWeapons);
            StartCoroutine(LoadCustomCustomBundlesAsync(true));
        }

        private IEnumerator LoadCustomCustomBundlesAsync(bool reload) // Special thanks to https://github.com/THE-GREAT-OVERLORD-OF-ALL-CHEESE/Custom-Scenario-Assets/ for this code
        {

            DirectoryInfo info = new DirectoryInfo(Directory.GetCurrentDirectory());
            Debug.Log("Searching " + Directory.GetCurrentDirectory() + " for .nbda custom weapons");
            foreach (FileInfo file in info.GetFiles("*.nbda", SearchOption.AllDirectories))
            {

                Debug.Log("Found .nbda " + file.FullName);
                StartCoroutine(LoadStreamedWeapons(file, true));
            }
            Debug.Log("Searching " + Directory.GetCurrentDirectory() + " for .cwb custom weapons");
            foreach (FileInfo file in info.GetFiles("*.cwb", SearchOption.AllDirectories))
            {
                
                Debug.Log("Found .nbda " + file.FullName);
                StartCoroutine(LoadStreamedWeapons(file, false));
            }
            
            yield break;
        }

        private IEnumerator LoadStreamedWeapons(FileInfo info, bool isNBDA)
        {
            if (AssetBundles != null)
            {
                foreach (var assetBundle in AssetBundles)
                {
                    assetBundle.Unload(true);
                }
            }

            AssetBundles = new List<AssetBundle>();
            
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(info.FullName);
            yield return request;

            if (request.assetBundle)
            {
                AssetBundles.Add(request.assetBundle);
                AssetBundleRequest requestJson = request.assetBundle.LoadAssetAsync("manifest.json");
                yield return requestJson;
                if (requestJson.asset == null)
                {
                    Debug.LogError("Couldn't find manifest.json in " + info.Name);
                    yield break;
                }

                TextAsset manifest = requestJson.asset as TextAsset;
                JObject jManifest = JsonConvert.DeserializeObject<JObject>(manifest.text);

                #region Legacy

                if (jManifest["Weapons"] == null && jManifest["Missiles"] == null && jManifest["Dependency"] == null)
                {
                    if (!isNBDA)
                    {
                        throw new NullReferenceException($"{info.Name} NEEDS TO UPDATE THEIR GODDAMN MANIFEST!!! DO IT!! FOR DEVELOPER: https://github.com/DankuwOs/CustomWeaponBase/blob/master/Builds/StreamingAssets/(Template)manifest.json");
                    }
                    
                    Debug.Log($"Loading legacy .nbda: {info.Name}");

                    Dictionary<string, string> jsonLines = jManifest.ToObject<Dictionary<string, string>>();
                    foreach (var weaponName in jsonLines)
                    {
                        AssetBundleRequest requestGun = request.assetBundle.LoadAssetAsync(weaponName.Key + ".prefab");
                        yield return requestGun;
                        if (requestGun.asset == null)
                        {
                            Debug.LogError("Couldn't load asset " + weaponName.Key);
                            continue;
                        }

                        string compat = weaponName.Value;

                        GameObject weapon = requestGun.asset as GameObject;
                        RegisterWeapon(weapon, weapon.name, compat);
                    }
                    yield break;
                }

                #endregion

                string dependency = (string) jManifest["Dependency"];

                if (File.Exists($"{info.DirectoryName}/{dependency}"))
                {
                    Debug.Log($"Trying to load dependency {dependency}");
                    LoadAssembly($"{info.DirectoryName}/{dependency}");
                }
                else
                {
                    Debug.Log($"Dependency empty for {info.FullName}");
                }

                string devDependency = (string) jManifest["DevDependency"];

                if (File.Exists(devDependency))
                {
                    Debug.Log($"Trying to load dev dependency @ {devDependency}");
                    LoadAssembly(devDependency);
                }
                else if (!string.IsNullOrEmpty(devDependency))
                {
                    Debug.Log($"Couldn't find dev dependency @ {devDependency}");
                }



                Dictionary<string, string> jsonWeapons = jManifest["Weapons"]?.ToObject<Dictionary<string, string>>();
                if (jsonWeapons != null)
                    foreach (var weapon in jsonWeapons)
                    {
                        Debug.Log($"Trying to add {weapon.Key}");
                        AssetBundleRequest requestWeapon = request.assetBundle.LoadAssetAsync(weapon.Key + ".prefab");
                        yield return requestWeapon;
                        if (requestWeapon == null)
                        {
                            Debug.LogError($"Couldn't load asset {weapon.Key}");
                            continue;
                        }

                        GameObject requestWeaponAsset = requestWeapon.asset as GameObject;

                        RegisterWeapon(requestWeaponAsset, weapon.Key, weapon.Value);
                    }
                
                // Removed missiles from json, they didn't do anything anyway..
            }
            else
            {
                Debug.Log("Couldn't load streamed bundle " + info.FullName);
            }

            yield break;
        }


        public void RegisterWeapon(GameObject equip, string weaponName, string compatability)
        {
            equip.name = weaponName;
            if(!equip.GetComponent<CWB_Weapon>())
                equip.AddComponent<CWB_Weapon>();
            DontDestroyOnLoad(equip);
            
            foreach (AudioSource source in equip.GetComponentsInChildren<AudioSource>(true))
            {
                source.outputAudioMixerGroup = VTResources.GetExteriorMixerGroup();
            }
            
            HPEquipMissileLauncher launcher = equip.GetComponent<HPEquipMissileLauncher>();

            if (launcher)
            {
                if (launcher.ml.missilePrefab && VTResources.Load<GameObject>(launcher.missileResourcePath) == null)
                {
                    RegisterMissile(launcher.ml.missilePrefab, launcher.missileResourcePath);
                }
                
                GameObject dummyEquipper = Resources.Load("hpequips/afighter/fa26_iris-t-x1") as GameObject;
                HPEquipIRML irml = dummyEquipper.GetComponent<HPEquipIRML>();

                if (launcher.ml.launchAudioClips == null)
                {
                    launcher.ml.launchAudioClips = new[]
                    {
                        Instantiate(irml.ml.launchAudioClips[0])
                    };
                }

                if (launcher.ml.launchAudioSources == null)
                {
                    AudioSource source = null;
                    if (launcher.GetComponent<AudioSource>())
                        source = launcher.GetComponent<AudioSource>();
                    else
                    {
                        source = launcher.gameObject.AddComponent<AudioSource>();
                        source.playOnAwake = false;
                        source.clip = irml.ml.launchAudioSources[0].clip;
                        source.outputAudioMixerGroup = irml.ml.launchAudioSources[0].outputAudioMixerGroup; // Don't know how much of this is required, but to be safe might as well.
                    }
                }
            }
            
            int mask = 0;
            if (compatability.Contains("42"))
                mask |= (int)VehicleCompat.AV42C;
            if (compatability.Contains("26"))
                mask |= (int)VehicleCompat.FA26B;
            if (compatability.Contains("45"))
                mask |= (int)VehicleCompat.F45A;
            if (compatability.Contains("94"))
                mask |= (int)VehicleCompat.AH94;
            
            VehicleCompat compat = (VehicleCompat)mask;
            
            if (VehicleCompatibility.CompareTo(compat, VTOLVehicles.AH94))
            {
                var resourceString = $"{AH94}/{weaponName}";
                VTResources.RegisterOverriddenResource(resourceString, equip);
                VTNetworkManager.RegisterOverrideResource(resourceString, equip);
            }

            if (VehicleCompatibility.CompareTo(compat, VTOLVehicles.F45A))
            {
                var resourceString = $"{F45}/{weaponName}";
                VTResources.RegisterOverriddenResource(resourceString, equip);
                VTNetworkManager.RegisterOverrideResource(resourceString, equip);
            }

            if (VehicleCompatibility.CompareTo(compat, VTOLVehicles.AV42C))
            {
                var resourceString = $"{AV42}/{weaponName}";
                VTResources.RegisterOverriddenResource(resourceString, equip);
                VTNetworkManager.RegisterOverrideResource(resourceString, equip);
            }

            if (VehicleCompatibility.CompareTo(compat, VTOLVehicles.FA26B))
            {
                var resourceString = $"{FA26}/{weaponName}";
                VTResources.RegisterOverriddenResource(resourceString, equip);
                VTNetworkManager.RegisterOverrideResource(resourceString, equip);
            }
            
            weapons.Add(Tuple.Create<string, GameObject>(weaponName, equip), compat);
            
            equip.SetActive(false);
        }

        public void RegisterMissile(GameObject missile, string resourcePath)
        {
            missile.AddComponent<CWB_Weapon>();
            DontDestroyOnLoad(missile);
            VTResources.RegisterOverriddenResource(resourcePath, missile);
            VTNetworkManager.RegisterOverrideResource(resourcePath, missile);
            missile.SetActive(false);
        }

        public void LoadAssembly(string path)
        {
            var file = Assembly.LoadFile(path);
            IEnumerable<Type> source =
                from t in file.GetTypes()
                where t.IsSubclassOf(typeof(VTOLMOD))
                select t;
            if (source != null && source.Count() == 1)
            {
                GameObject go = new GameObject("a mod :~)", source.First());
                DontDestroyOnLoad(go);
                go.GetComponent<VTOLMOD>().ModLoaded();
            }
        }

        public void DestroyObjects(CWB_Weapon[] gameObjects)
        {
            foreach (var o in gameObjects)
            {
                Destroy(o.gameObject);
                Debug.Log("CWB: Destroyed obj.");
            }
        }
    }
}