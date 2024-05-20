using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomWeaponBase;
using Harmony;
using UnityEngine;
using Valve.Newtonsoft.Json;

public class CustomWeaponsBase : MonoBehaviour
{
    public static CustomWeaponsBase instance;

    // Debug
    public WeaponManager MyWeaponManager;

    public Loadout Loadout = null;

    public List<PlayerVehicle> PlayerVehicles;

    // Something
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

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.P))
        {
            HDRScreenshot(); // oops left this in but its there now
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
            Debug.Log("finM");
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
            Debug.Log("finN");

        }
    }

    public void HDRScreenshot()
    {
        int num = 3840;
        int num2 = num / 16 * 9;
        RenderTexture temporary = RenderTexture.GetTemporary(num, num2, 32, RenderTextureFormat.ARGBHalf);
        temporary.antiAliasing = 8;
        var cameraFollowMe = CameraFollowMe.instance;
        Camera camera = cameraFollowMe.cam;
        if (camera.gameObject.activeSelf)
        {
            RenderTexture targetTexture = camera.targetTexture;
            camera.targetTexture = temporary;
            camera.Render();
            camera.targetTexture = targetTexture;
        }

        Texture2D texture2D = new Texture2D(num, num2, TextureFormat.RGBAHalf, false);
        RenderTexture.active = temporary;
        texture2D.ReadPixels(new Rect(0f, 0f, num, num2), 0, 0);
        RenderTexture.ReleaseTemporary(temporary);
        string dir = Path.Combine(VTResources.gameRootDirectory, "Screenshots");
        byte[] png = texture2D.EncodeToPNG();
        
        Destroy(texture2D);
        
        File.WriteAllBytes(FlybyCameraMFDPage.GetNewScreenshotFilepath(dir), png);
    }

    public void ReloadWeapons()
    {
        if (Loadout == null)
        {
            Debug.LogError("CWB ReloadWeapons() Loadout null");
            
            return;
        }

        if (!MyWeaponManager)
            MyWeaponManager = VTOLAPI.GetPlayersVehicleGameObject().GetComponent<WeaponManager>();

        if (MyWeaponManager == null)
        {
            Debug.LogError("CWB ReloadWeapons() WM null");
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
            
            Debug.Log("Couldn't find a livery sad");
            return null;
        }
        return null;
    }

    [Obsolete]
    public static bool CompareCompat(string compat, string vehicle)
    {
        var compats = compat.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);
        foreach (var s in compats)
        {
            if (vehicle.Contains(s))
                return true;
        }

        return false;
    }

    public static bool CompareCompatNew(object compat, string vehicle, HPEquippable equippable)
    {
        if (!equippable)
            return false;
        var weaponCompat = JsonConvert.DeserializeObject<Dictionary<string, string>[]>(compat.ToString());
        foreach (var dictionary in weaponCompat)
        {
            foreach (var compatability in dictionary)
            {
                var compatVehicle = compatability.Key;
                var compatHardpoints = compatability.Value;
                
                if (!vehicle.Contains(compatVehicle))
                    continue;

                equippable.allowedHardpoints = compatHardpoints;

                return true;
            }
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

    public static void ApplyLivery(HPEquippable equippable, WeaponManager weaponManager)
    {
        var liveryMesh = equippable.GetComponent<LiveryMesh>();

        if (!weaponManager || !liveryMesh)
        {
            return;
        }

        MaterialPropertyBlock block;
        
        if (liveryMesh.copyMaterial)
        {
            // I know you can transform.find an entire path now, :~(
            var objectPaths = liveryMesh.materialPath.Split('/');

            var obj = objectPaths.Aggregate(weaponManager.transform, (current, path) => current.Find(path));

            var renderer = obj.GetComponent<Renderer>();

            block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);

            foreach (var mesh in liveryMesh.liveryMeshs)
            {
                mesh.material = renderer.materials[0];
                mesh.SetPropertyBlock(block);
            }

            return;
        }
        
        block = new MaterialPropertyBlock();
        
        if (weaponManager.liverySample)
            weaponManager.liverySample.GetPropertyBlock(block);
        else if (!string.IsNullOrEmpty(liveryMesh.materialPath))
        {
            var materialObj = weaponManager.transform.Find(liveryMesh.materialPath);
            if (!materialObj)
                return;
            
            var meshRenderer = materialObj.GetComponent<MeshRenderer>();
            if (!meshRenderer)
                return;
            meshRenderer.GetPropertyBlock(block);
        }

        var livery = block.GetTexture("_Livery");
        if (!livery)
        {
            Debug.Log($"[LiveryMesh]: Livery null");
            return;
        }

        foreach (var mesh in liveryMesh.liveryMeshs)
        {
            if (liveryMesh.useLivery)
            {
                mesh.material.SetTexture(liveryMesh.textureID, livery);
            }
            else
            {
                mesh.material.SetTexture("_DetailAlbedoMap", livery);
                mesh.material.EnableKeyword("_DETAIL_MULX2");
            }
        }
    }

    public static void ToggleMeshHider(HPEquippable equippable, WeaponManager weaponManager, bool enable = false)
    {
        var meshHider = equippable.GetComponent<MeshHider>();

        if (!meshHider) return;

        var tf = weaponManager.transform;

        foreach (var s in meshHider.hiddenMeshs)
        {
            var transform = tf;

            string[] subStrings = s.Split('/');

            if (subStrings.Length == 0) return;

            foreach (var subString in subStrings)
            {
                var tranny = transform.Find(subString);
                if (!tranny)
                {
                    Debug.Log($"[MeshHider]: Couldn't find {subString} in {transform}");
                    break;
                }

                transform = tranny;
            }

            if (meshHider.hideSubMeshs)
            {
                var meshs = transform.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in meshs)
                {
                    renderer.enabled = enable;
                }
            }
            else
            {
                var mesh = transform.GetComponent<Renderer>();
                mesh.enabled = enable;
            }
        }
    }
}