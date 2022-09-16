﻿using System;
using System.Collections;
using System.Collections.Generic;
using Harmony;
using UnityEngine;

public class CombinedCannon : MonoBehaviour
{
    private List<HPEquipGun> salvoGuns = new List<HPEquipGun>();

    [Range(0f, 1f)]
    [Tooltip("Multiply the delay between guns (ex. 900 rpm / 60000 = 0.015) by this")]
    public float sequentialDelayFactor = 0.5f;

    public HPEquipGun gun;

    private WeaponManager _wm;

    private void Awake()
    {
        if (gun && gun.weaponManager)
        {
            _wm = gun.weaponManager;
        }
    }

    public void ShouldFire(bool fire)
    {
        if (!_wm)
            return;

        if (fire)
            StartFire();
        else
            StopFire();
    }
    
    public void StartFire()
    {
        this.salvoGuns.Clear();
        for (int i = 0; i < _wm.equipCount; i++)
        {
            HPEquippable equippable = _wm.GetEquip(i);
            if (equippable && equippable is HPEquipGun && equippable.GetCount() > 0 && equippable.shortName == gun.shortName)
            {
                salvoGuns.Add((HPEquipGun) equippable);
            }
        }

        if (salvoGuns.Count > 0)
        {
            StartCoroutine(StartFiringSequentially());
        }
    }

    public void StopFire()
    {
        for (int i = 0; i < salvoGuns.Count; i++)
        {
            salvoGuns[i].OnStopFire();
        }
    }

    public IEnumerator StartFiringSequentially()
    {
        if (salvoGuns.Count == 0)
            yield break;
        
        for (int i = 0; i < salvoGuns.Count; i++)
        {
            bool firing = Traverse.Create(salvoGuns[i]).Field("firing").GetValue<bool>();
            if (!firing)
                salvoGuns[i].OnStartFire();
            
            yield return new WaitForSeconds(salvoGuns[i].gun.rpm / 60000 * sequentialDelayFactor);
        }
        yield break;
    }
}