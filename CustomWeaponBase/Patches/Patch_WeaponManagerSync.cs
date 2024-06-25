using System.Linq;
using CustomWeaponBase;
using HarmonyLib;
using UnityEngine;
using VTOLVR.Multiplayer;


[HarmonyPatch(typeof(WeaponManagerSync), "OnNetInitialized")]
public class Patch_WeaponManagerSync
{
    public static void Postfix(WeaponManagerSync __instance)
    {
        if (!__instance.wm || __instance.isMine)
            return;
        
        var weaponManager = __instance.wm;
        
        var wm = Traverse.Create(weaponManager);
        if (weaponManager.hardpointTransforms.Any(e => e.transform.parent.gameObject.name.Contains("CWBHB")))
            return;

        int hpCount = Main.extraHps;
        
        Debug.Log($"[CWB MP]: Compare hp loadout to hptfs: {weaponManager.hardpointTransforms.Length + 3} | {weaponManager.hardpointTransforms.Length}");
        for (int i = 0; i < hpCount; i++)
        {
            var newTransform = new GameObject($"CWBHP_{weaponManager.hardpointTransforms.Length + 1}").transform;
            newTransform.SetParent(__instance.transform);
            newTransform.localPosition = Vector3.zero;
            newTransform.localRotation = Quaternion.identity;
            
            var tfList = weaponManager.hardpointTransforms.ToList();
            tfList.Add(newTransform);
            weaponManager.hardpointTransforms = tfList.ToArray();

            wm.Field("equips").SetValue(new HPEquippable[weaponManager.hardpointTransforms.Length]);
        }
    }
}