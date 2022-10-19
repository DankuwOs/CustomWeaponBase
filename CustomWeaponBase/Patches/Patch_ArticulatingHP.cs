using Harmony;
using UnityEngine;
using VTOLVR.DLC.Rotorcraft;

[HarmonyPatch(typeof(ArticulatingHardpoint), "Update")]
public class Patch_ArticulatingHardpointUpdate
{
    [HarmonyPrefix]
    public static void Prefix(ArticulatingHardpoint __instance)
    {
        if (!__instance.remoteOnly && __instance.autoMode)
        {
            for (int i = 0; i < __instance.hardpoints.Length; i++)
            {
                ArticulatingHardpoint.Hardpoint hardpoint = __instance.hardpoints[i];
                HPEquippable equip = __instance.wm.GetEquip(hardpoint.hpIdx);
                if (equip && equip.GetComponent<CWB_Weapon>().articulatingHP)
                {
                    UpdateEquip(hardpoint, equip, __instance);
                }
            }
        }
    }

    public static void UpdateEquip(ArticulatingHardpoint.Hardpoint hardpoint, HPEquippable equip, ArticulatingHardpoint articulatingHardpoint)
    {
        float t;
        
        if (articulatingHardpoint.wm.opticalTargeter && articulatingHardpoint.wm.opticalTargeter.locked)
        {
            var hardpointArticulationTf = hardpoint.articulationTf;
            Vector3 toDirection = Vector3.ProjectOnPlane(equip.GetAimPoint() - hardpointArticulationTf.position, hardpointArticulationTf.right);
            Vector3 toDirection2 = Vector3.ProjectOnPlane(articulatingHardpoint.wm.opticalTargeter.lockTransform.position - hardpointArticulationTf.position, hardpointArticulationTf.right);
            float num = VectorUtils.SignedAngle(hardpointArticulationTf.forward, toDirection, hardpointArticulationTf.up);
            float num2 = VectorUtils.SignedAngle(hardpointArticulationTf.parent.forward, toDirection2, hardpointArticulationTf.parent.up);
            t = num - num2;
        }
        else
        {
            t = 0f;
        }

        foreach (ArticulatingHardpoint.Hardpoint ahp in articulatingHardpoint.hardpoints)
        {
            HPEquippable equippable = articulatingHardpoint.wm.GetEquip(ahp.hpIdx);
            if (equippable && equippable.shortName == equip.shortName)
                articulatingHardpoint.SetTilt(ahp, t, false, true);
        }
    }
}