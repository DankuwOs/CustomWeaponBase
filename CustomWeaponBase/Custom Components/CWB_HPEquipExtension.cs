using Steamworks.Data;
using UnityEngine;
using UnityEngine.Events;

public class CWB_HPEquipExtension : MonoBehaviour
{
    [HideInInspector]
    public HPEquippable hpEquip;

    public UnityEvent OnHPEquip = new();

    public UnityEvent OnHPUnEquip = new();
    
    public virtual void Equip() {}
    
    public virtual void Jettison() {}
    
    public virtual void OnJettison() {}

    public virtual void OnConfigAttach(LoadoutConfigurator configurator) {}

    public virtual void OnConfigDetach(LoadoutConfigurator configurator) {}
    
    public virtual void OnCycleWeaponButton() {}
    
    public virtual void OnDisabledByPartDestroy() {}
    
    public virtual void OnDisableWeapon() {}
    
    public virtual void OnEnableWeapon() {}

    public virtual void OnEquip() { OnHPEquip.Invoke(); }

    public virtual void OnUnequip() { OnHPUnEquip.Invoke(); }
    
    public virtual void OnQuickloadEquip(ConfigNode node) {}
    
    public virtual void OnQuicksaveEquip(ConfigNode node) {}
    
    public virtual void OnReleasedCycleWeaponButton() {}
    
    public virtual void OnStartFire() {}
    
    public virtual void OnStopFire() {}
    
    public virtual void OnTriggerAxis(float axis) {}
    
    public virtual void UpdateWeaponType() {}
}