using System;
using Harmony;
using UnityEngine;

[HarmonyPatch(typeof(Missile), nameof(Missile.Detonate), new Type[] { typeof(Collider) })]
public class Patch_Missile
{
    public static void Postfix(Missile __instance, Vector3 ___explosionNormal)
    {
        var cwbExp = __instance.GetComponent<CWB_Explosion>();
        if (!cwbExp)
            return;


        foreach (var explosion in cwbExp.explosions) 
        {
            var scale = __instance.transform.localScale.x + __instance.transform.localScale.y + __instance.transform.localScale.z / 3;
            cwbExp.CreateExplosionEffect(explosion, __instance.transform.position, cwbExp.useNormal? ___explosionNormal : Vector3.up, scale);
        }
        if (!FlybyCameraMFDPage.instance || !FlybyCameraMFDPage.instance.isCamEnabled || !FlybyCameraMFDPage.instance.flybyCam || cwbExp.shake < 0.05f) return;
        float num2 = cwbExp.shake * cwbExp.shake / (FlybyCameraMFDPage.instance.flybyCam.transform.position - __instance.transform.position).sqrMagnitude;
        FlybyCameraMFDPage.instance.ShakeCamera(num2 * 2f);
    }
}