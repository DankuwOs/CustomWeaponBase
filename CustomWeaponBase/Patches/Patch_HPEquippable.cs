using System.Linq;
using Harmony;
using UnityEngine;

[HarmonyPatch(typeof(HPEquippable), nameof(HPEquippable.Equip))]
public class Patch_HPEquippable_Equip
{
    [HarmonyPostfix]
    public static void Equip(HPEquippable __instance)
    {
        var liveryMesh = __instance.GetComponent<LiveryMesh>();
        if (liveryMesh && liveryMesh.copyMaterial && __instance.weaponManager)
        {
            var objectPaths = liveryMesh.materialPath.Split('/');
            
            var obj = objectPaths.Aggregate(__instance.weaponManager.transform, (current, path) => current.Find(path));

            var renderer = obj.GetComponent<Renderer>();

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);

            foreach (var mesh in liveryMesh.liveryMeshs)
            {
                mesh.material = renderer.materials[0];
                mesh.SetPropertyBlock(block);
            }

            return;
        }

        if (!liveryMesh || !__instance.weaponManager.liverySample) return;
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            __instance.weaponManager.liverySample.GetPropertyBlock(block);

            var livery = block.GetTexture("_Livery");
            if (!livery)
                return;
            
            foreach (var mesh in liveryMesh.liveryMeshs)
            {
                mesh.material.SetTexture("_DetailAlbedoMap", livery);
                mesh.material.EnableKeyword("_DETAIL_MULX2");
            }
        }
    }
}

[HarmonyPatch(typeof(HPEquippable), nameof(HPEquippable.OnConfigAttach))]
public class Patch_HPEquippable_OnConfigAttach
{
    [HarmonyPostfix]
    public static void OnConfigAttach(HPEquippable __instance, LoadoutConfigurator configurator)
    {
        var liveryMesh = __instance.GetComponent<LiveryMesh>();
        if (liveryMesh && liveryMesh.copyMaterial && configurator.wm)
        {
            var objectPaths = liveryMesh.materialPath.Split('/');
            
            var obj = objectPaths.Aggregate(configurator.wm.transform, (current, path) => current.Find(path));

            var renderer = obj.GetComponent<Renderer>();

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);

            foreach (var mesh in liveryMesh.liveryMeshs)
            {
                mesh.material = renderer.materials[0];
                mesh.SetPropertyBlock(block);
            }

            return;
        }
        
        if (liveryMesh && configurator.wm.liverySample)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            configurator.wm.liverySample.GetPropertyBlock(block);

            var livery = block.GetTexture("_Livery");
            if (!livery)
                return;
            
            foreach (var mesh in liveryMesh.liveryMeshs)
            {
                mesh.material.SetTexture("_DetailAlbedoMap", livery);
                mesh.material.EnableKeyword("_DETAIL_MULX2");
            }
        }
    }
}