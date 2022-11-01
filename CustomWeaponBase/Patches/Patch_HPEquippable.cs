using Harmony;
using UnityEngine;

[HarmonyPatch(typeof(HPEquippable), nameof(HPEquippable.Equip))]
public class Patch_HPEquippable_Equip
{
    [HarmonyPostfix]
    public static void Equip(HPEquippable __instance)
    {
        var liveryMesh = __instance.GetComponent<LiveryMesh>();
        
        if (liveryMesh && __instance.weaponManager.liverySample)
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