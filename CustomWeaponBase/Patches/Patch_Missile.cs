using System;
using System.Runtime.CompilerServices;
using Harmony;
using UnityEngine;
using UnityEngine.Events;
using VTOLVR.Multiplayer;
using Object = UnityEngine.Object;

[HarmonyPatch(typeof(Missile), nameof(Missile.Detonate), new Type[] { typeof(Collider) })]
public class Patch_Missile
{
    public static bool Prefix(Missile __instance, Collider directHit, Vector3 ___explosionNormal, ref bool ___detonated, UnityAction<Missile> ___OnMissileDetonated)
    {
        var cwbExp = __instance.GetComponent<CWB_Explosion>();
        if (!cwbExp)
            return true;
        
        // Written by hand so its legally mine.

        if (___detonated)
        {
            return true;
        }

        ___detonated = true;

        Vector3 sourceVelocity = Vector3.one;
        if (__instance.rb)
        {
            sourceVelocity = __instance.rb.velocity;
        }
        else
        {
            Debug.Log($"Missile has no rigidbody attached!");
        }

        ___explosionNormal = sourceVelocity;

        for (int i = 0; i < __instance.colliders.Length; i++)
        {
            if (__instance.colliders[i])
            {
                __instance.colliders[i].enabled = false;
            }
        }

        __instance.OnDetonate.Invoke();

        ___OnMissileDetonated(__instance);

        if (__instance.exhaustTransform)
        {
            __instance.ps.SetEmission(false);
            __instance.exhaustTransform.parent = null;

            FloatingOriginTransform exhaust = __instance.exhaustTransform.gameObject.GetComponent<FloatingOriginTransform>();
            if (!exhaust)
            {
                exhaust = __instance.exhaustTransform.gameObject.AddComponent<FloatingOriginTransform>();
            }

            exhaust.shiftParticles = false;
            Object.Destroy(exhaust.gameObject, __instance.ps.GetLongestLife());
            if (__instance.exhaustLight)
            {
                __instance.exhaustLight.enabled = false;
            }
            
            
        }

        foreach (var explosion in cwbExp.explosions) 
        {
            var scale = __instance.transform.localScale.x + __instance.transform.localScale.y + __instance.transform.localScale.z / 3;
            cwbExp.CreateExplosionEffect(explosion, __instance.transform.position, cwbExp.useNormal? ___explosionNormal : Vector3.up, scale);
        }
        if (!FlybyCameraMFDPage.instance || !FlybyCameraMFDPage.instance.isCamEnabled || !FlybyCameraMFDPage.instance.flybyCam || cwbExp.shake < 0.05f) return false;
        float num2 = cwbExp.shake * cwbExp.shake / (FlybyCameraMFDPage.instance.flybyCam.transform.position - __instance.transform.position).sqrMagnitude;
        FlybyCameraMFDPage.instance.ShakeCamera(num2 * 2f);
        
        if (VTOLMPUtils.IsMultiplayer())
        {
            __instance.gameObject.SetActive(false);
            return false;
        }
        Object.Destroy(__instance.gameObject);
        return false;
    }
}