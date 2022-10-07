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

        private readonly string CWBPath = "CustomWeaponBase";

        public static Dictionary<Tuple<string, GameObject>, string> weapons = new Dictionary<Tuple<string, GameObject>, string>();

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
            
            VTOLAPI.SceneLoaded += delegate
            {
                CustomWeaponsBase.instance.CheckVehicleListChanged(VTResources.GetPlayerVehicleList());
            };
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
            Debug.Log("[CWB]: Searching " + Directory.GetCurrentDirectory() + " for .nbda custom weapons");
            foreach (FileInfo file in info.GetFiles("*.nbda", SearchOption.AllDirectories))
            {

                Debug.Log("[CWB]: Found .nbda " + file.FullName);
                StartCoroutine(LoadStreamedWeapons(file, true));
            }
            Debug.Log("[CWB]: Searching " + Directory.GetCurrentDirectory() + " for .cwb custom weapons");
            foreach (FileInfo file in info.GetFiles("*.cwb", SearchOption.AllDirectories))
            {
                
                Debug.Log("[CWB]: Found .nbda " + file.FullName);
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
                    
                    Debug.Log($"[CWB]: Loading legacy .nbda: {info.Name}");

                    Dictionary<string, string> jsonLines = jManifest.ToObject<Dictionary<string, string>>();
                    foreach (var weaponName in jsonLines)
                    {
                        AssetBundleRequest requestGun = request.assetBundle.LoadAssetAsync(weaponName.Key + ".prefab");
                        yield return requestGun;
                        if (requestGun.asset == null)
                        {
                            Debug.LogError("[CWB]: Couldn't load asset " + weaponName.Key);
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
                    Debug.Log($"[CWB]: Trying to load dependency {dependency}");
                    LoadAssembly($"{info.DirectoryName}/{dependency}");
                }
                else
                {
                    Debug.Log($"[CWB]: Dependency empty for {info.FullName}");
                }

                string devDependency = (string) jManifest["DevDependency"];

                if (File.Exists(devDependency))
                {
                    Debug.Log($"[CWB]: Trying to load dev dependency @ {devDependency}");
                    LoadAssembly(devDependency);
                }
                else if (!string.IsNullOrEmpty(devDependency))
                {
                    Debug.Log($"[CWB]: Couldn't find dev dependency @ {devDependency}");
                }



                Dictionary<string, string> jsonWeapons = jManifest["Weapons"]?.ToObject<Dictionary<string, string>>();
                if (jsonWeapons != null)
                    foreach (var weapon in jsonWeapons)
                    {
                        AssetBundleRequest requestWeapon = request.assetBundle.LoadAssetAsync(weapon.Key + ".prefab");
                        yield return requestWeapon;
                        if (requestWeapon == null)
                        {
                            Debug.LogError($"[CWB]: Couldn't load asset {weapon.Key}, make sure the prefab is included in the AB and built.");
                            continue;
                        }

                        GameObject requestWeaponAsset = requestWeapon.asset as GameObject;

                        RegisterWeapon(requestWeaponAsset, weapon.Key, weapon.Value);
                    }
                
                // Removed missiles from json, they didn't do anything anyway..
            }
            else
            {
                Debug.Log("[CWB]: Couldn't load streamed bundle " + info.FullName);
            }

            yield break;
        }


        public void RegisterWeapon(GameObject equip, string weaponName, string compatability)
        {
            Debug.Log($"[CWB]: Registering {weaponName}");
            if (!equip)
            {
                Debug.Log($"[CWB]: {weaponName} is null!");
                return;
            }

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
            
            var pvList = VTResources.GetPlayerVehicleList();

            foreach (var playerVehicle in pvList.Where(playerVehicle => CustomWeaponsBase.CompareCompat(compatability, playerVehicle.vehicleName, weaponName)))
            {
                VTResources.RegisterOverriddenResource($"{playerVehicle.equipsResourcePath}/{weaponName}", equip);
                VTNetworkManager.RegisterOverrideResource($"{playerVehicle.equipsResourcePath}/{weaponName}", equip);
            }
            /*foreach (var playerVehicle in pvList)
            {
                if (!CustomWeaponsBase.CompareCompat(compatability, playerVehicle.vehicleName, weaponName))
                {
                    Debug.Log($"{weaponName} failed with {playerVehicle.vehicleName}");
                    continue;
                }
                Debug.Log($"[CWB]: Adding {weaponName} to '{playerVehicle.equipsResourcePath}/{weaponName}'");
                VTResources.RegisterOverriddenResource($"{playerVehicle.equipsResourcePath}/{weaponName}", equip);
                VTNetworkManager.RegisterOverrideResource($"{playerVehicle.equipsResourcePath}/{weaponName}", equip);
            }*/
            
            weapons.Add(Tuple.Create<string, GameObject>(weaponName, equip), compatability);
            
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
            var source =
                from t in file.GetTypes()
                where t.IsSubclassOf(typeof(VTOLMOD))
                select t;
            if (source == null || source.Count() != 1) return;
            
            var go = new GameObject("a mod :~)", source.First());
            DontDestroyOnLoad(go);
            go.GetComponent<VTOLMOD>().ModLoaded();
        }

        public void DestroyObjects(CWB_Weapon[] gameObjects)
        {
            foreach (var o in gameObjects)
            {
                Debug.Log($"[CWB]: Destroying obj: {o.gameObject.name}");
                Destroy(o.gameObject);
            }
        }
    }
}