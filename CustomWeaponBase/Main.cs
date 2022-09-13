using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Linq;
using VTNetworking;

namespace CustomWeaponBase
{
    public class Main : VTOLMOD
    {
        private readonly string AH94 = "HPEquips/AH-94";
        private readonly string FA26 = "HPEquips/AFighter";
        private readonly string F45  = "HPEquips/F45A";
        private readonly string AV42 = "HPEquips/VTOL";

        public static Dictionary<Tuple<string, GameObject>, VehicleCompat> weapons = new Dictionary<Tuple<string, GameObject>, VehicleCompat>();
        
        public override void ModLoaded()
        {
            HarmonyInstance instance = HarmonyInstance.Create("danku.cwb");
            instance.PatchAll();
            
            base.ModLoaded();
            StartCoroutine(LoadCustomCustomBundlesAsync());
        }

        private IEnumerator LoadCustomCustomBundlesAsync() // Special thanks to https://github.com/THE-GREAT-OVERLORD-OF-ALL-CHEESE/Custom-Scenario-Assets/ for this code
        {
            DirectoryInfo info = new DirectoryInfo(Directory.GetCurrentDirectory());
            Debug.Log("Searching " + Directory.GetCurrentDirectory() + " for .nbda custom weapons");
            foreach (FileInfo file in info.GetFiles("*.nbda", SearchOption.AllDirectories))
            {
                Debug.Log("Found .nbda " + file.FullName);
                StartCoroutine(LoadStreamedWeapons(file));
            }
            yield break;
        }

        private IEnumerator LoadStreamedWeapons(FileInfo info)
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(info.FullName);
            yield return request;

            if (request.assetBundle != null)
            {
                AssetBundleRequest requestjson = request.assetBundle.LoadAssetAsync("manifest.json");
                yield return requestjson;
                if (requestjson.asset == null)
                {
                    Debug.LogError("Couldn't find manifest.json in " + info.Name);
                    yield break;
                }

                TextAsset manifest = requestjson.asset as TextAsset;
                JObject jManifest = JsonConvert.DeserializeObject<JObject>(manifest.text);

                #region Legacy

                if (jManifest["Weapons"] == null && jManifest["Missiles"] == null && jManifest["Dependency"] == null)
                {
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
                    Assembly.LoadFile($"{info.DirectoryName}/{dependency}");
                }
                else
                {
                    Debug.Log($"Dependency empty for {info.FullName}");
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

                Dictionary<string, string> jsonMissiles = jManifest["Missiles"]?.ToObject<Dictionary<string, string>>();
                if (jsonMissiles != null)
                    foreach (var missile in jsonMissiles)
                    {
                        Debug.Log($"Trying to add missile {missile.Key}");
                        AssetBundleRequest requestMissile = request.assetBundle.LoadAssetAsync(missile.Key + ".prefab");
                        yield return requestMissile;
                        if (requestMissile == null)
                        {
                            Debug.Log($"Couldn't load missile {missile.Key}");
                            continue;
                        }

                        GameObject requestMissileAsset = requestMissile.asset as GameObject;

                        RegisterMissile(requestMissileAsset, missile.Value);
                    }
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
            DontDestroyOnLoad(missile);
            VTResources.RegisterOverriddenResource(resourcePath, missile);
            VTNetworkManager.RegisterOverrideResource(resourcePath, missile);
            missile.SetActive(false);
        }
    }
}