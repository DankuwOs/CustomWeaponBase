using System.Linq;
using Harmony;
using UnityEngine;
using VTOLVR.Multiplayer;

[HarmonyPatch(typeof(WeaponManagerSync), "OnNetInitialized")]
public class Patch_WeaponManagerSync
{
    public static void Postfix(WeaponManagerSync __instance)
    {
        if (!__instance.wm)
            return;
        var weaponManager = __instance.wm;
        Debug.Log($"[CWB MP]: Compare hp loadout to hptfs: {weaponManager.hardpointTransforms.Length + 3} | {weaponManager.hardpointTransforms.Length}");
        for (int i = 0; i < 3; i++)
        {
            var newTransform = new GameObject($"HP_{weaponManager.hardpointTransforms.Length + 1}").transform;
            newTransform.SetParent(__instance.transform);
            newTransform.localPosition = Vector3.zero;
            newTransform.localRotation = Quaternion.identity;
            
            var tfList = weaponManager.hardpointTransforms.ToList();
            tfList.Add(newTransform);
            weaponManager.hardpointTransforms = tfList.ToArray();

            var wm = Traverse.Create(weaponManager);
            wm.Field("equips").SetValue(new HPEquippable[weaponManager.hardpointTransforms.Length]);
        }
    }
}