﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CWBFLKMissile : MonoBehaviour
{
    public Missile missile;
    
    [Header("Optional")]
    public MissileFairing[] fairings;

    [Header("Optional")]
    public Gun gun;

    public float fireDelay;
    
    [Tooltip("Invoked before gun starts firing")]
    public UnityEvent OnFire = new UnityEvent();

    public UnityEvent OnEmpty = new UnityEvent();
    
    public void StartFLKRoutine()
    {
        StartCoroutine(FLKRoutinte());
    }

    private IEnumerator FLKRoutinte()
    {
        
        if (fairings.Length > 0)
            foreach (var missileFairing in fairings)
            {
                missileFairing.Jettison();
            }

        yield return new WaitForSeconds(fireDelay);
        
        OnFire.Invoke();
        if (!gun)
            yield break;
        
        gun.actor = missile.actor;
        
        gun.SetFire(true);

        while (gun.currentAmmo > 0)
        {
            yield return null;
        }
        
        OnEmpty.Invoke();
    }
}