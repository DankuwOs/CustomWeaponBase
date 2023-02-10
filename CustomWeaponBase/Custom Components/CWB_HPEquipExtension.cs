using Steamworks.Data;
using UnityEngine;

public class CWB_HPEquipExtension : MonoBehaviour
{
    [HideInInspector]
    public HPEquippable hpEquip;
    
    public virtual void Equip() {}
    
    public virtual void Jettison() {}
    
    public virtual void OnJettison() {}

    public virtual void OnConfigAttach(LoadoutConfigurator configurator) {}

    public virtual void OnConfigDetach(LoadoutConfigurator configurator) {}
    
    public virtual void OnCycleWeaponButton() {}
    
    public virtual void OnDisabledByPartDestroy() {}
    
    public virtual void OnDisableWeapon() {}
    
    public virtual void OnEnableWeapon() {}

    public virtual void OnEquip() {}

    public virtual void OnUnequip() {}
    
    public virtual void OnQuickloadEquip(ConfigNode node) {}
    
    public virtual void OnQuicksaveEquip(ConfigNode node) {}
    
    public virtual void OnReleasedCycleWeaponButton() {}
    
    public virtual void OnStartFire() {}
    
    public virtual void OnStopFire() {}
    
    public virtual void OnTriggerAxis(float axis) {}
    
    public virtual void UpdateWeaponType() {}
}