using UnityEngine;

namespace CustomWeaponBase.Custom_Components;

public class CWB_FuelTank : CWB_HPEquipExtension
{
    public float fuelCapacity;
    
    private bool _addedFuel;

    public override void OnEquip()
    {
        base.OnEquip();
        Debug.Log($"[CWB_FuelTank]: OnEquip");
        if (!_addedFuel)
        {
            hpEquip.weaponManager.GetComponent<FuelTank>().maxFuel += fuelCapacity;
            _addedFuel = true;
        }
    }

    public override void OnConfigAttach(LoadoutConfigurator configurator)
    {
        base.OnConfigAttach(configurator);
        Debug.Log($"[CWB_FuelTank]: OnConfigAttach");
        if (!_addedFuel)
        {
            float num;
            if (configurator.uiOnly)
            {
                num = configurator.fuelKnob.currentValue;
                configurator.ui_maxFuel += fuelCapacity;
            }
            else
            {
                FuelTank component = configurator.wm.GetComponent<FuelTank>();
                float fuel = component.fuel;
                num = component.fuelFraction;
                component.maxFuel += fuelCapacity;
            }
            configurator.fullInfo.SetNormFuel(num);
            configurator.fuelKnob.SetKnobValue(num);
            _addedFuel = true;
        }
    }

    public override void OnConfigDetach(LoadoutConfigurator configurator)
    {
        base.OnConfigDetach(configurator);
        Debug.Log($"[CWB_FuelTank]: OnConfigDetach");
        if (_addedFuel)
        {
            float num;
            if (configurator.uiOnly)
            {
                num = configurator.fuelKnob.currentValue;
                configurator.ui_maxFuel -= fuelCapacity;
            }
            else
            {
                FuelTank component = configurator.wm.GetComponent<FuelTank>();
                float fuel = component.fuel;
                num = component.fuelFraction;
                component.maxFuel -= fuelCapacity;
            }
            configurator.fullInfo.SetNormFuel(num);
            configurator.fuelKnob.SetKnobValue(num);
            _addedFuel = false;
        }
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        Debug.Log($"[CWB_FuelTank]: OnUnequip");
        if (_addedFuel)
        {
            hpEquip.weaponManager.GetComponent<FuelTank>().maxFuel -= fuelCapacity;
            _addedFuel = false;
        }
    }
}