using System;
using System.Collections.Generic;
using Harmony;
using UnityEngine;

public class CWB_Explosion : MonoBehaviour
{
    
    public GameObject[] explosions;
    
    public float shake;

    public bool useNormal;

    public bool scaleHierarchy;

    private void Awake()
    {
        if (!scaleHierarchy)
            return;
        foreach (var explosion in explosions)
        {
            var explosionFx = explosion.GetComponent<ExplosionFX>();
            if (!explosionFx)
                return;
            foreach (var ps in explosionFx.particleSystems)
            {
                SetScalingMode(ps);
            }
        }
    }

    public void CreateExplosionEffect(GameObject explosion, Vector3 position, Vector3 normal, float scale = 1f)
    {
        var explosionParent = new GameObject("Exploisiosne!")
        {
            transform =
            {
                position = position,
                rotation = Quaternion.Euler(normal),
                parent = null,
                localScale = Vector3.one * scale
            }
        };
        
        
        explosionParent.AddComponent<FloatingOriginTransform>();
        
        var obj = Instantiate(explosion, explosionParent.transform);
        
        CustomWeaponsBase.instance.AddObject(obj);
    }

    public void SetScalingMode(ParticleSystem ps)
    {
        var main = ps.main;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
    }
}