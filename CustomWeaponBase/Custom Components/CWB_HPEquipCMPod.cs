using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CWB_HPEquipCMPod : HPEquippable, IMassObject
{
    public float mass;
    
    [Header("CM Pod")]
    [Tooltip("Put FlareCountermeasure and ChaffCountermeasure components here")]
    public List<Countermeasure> cms = new();

    private CountermeasureManager _countermeasureManager;

    protected override void OnEquip()
    {
        base.OnEquip();
        
        Initialize();
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        
        UnEquip();
    }

    private void Initialize()
    {
        _countermeasureManager = weaponManager.GetComponent<CountermeasureManager>();

        _countermeasureManager.countermeasures.AddRange(cms);
        foreach (var countermeasure in cms)
        {
            var type = countermeasure.GetType();
            if (type == typeof(FlareCountermeasure))
            {
                _countermeasureManager.flareCMs.Add(countermeasure as FlareCountermeasure);
            }
            else if (type == typeof(ChaffCountermeasure))
            {
                _countermeasureManager.chaffCMs.Add(countermeasure as ChaffCountermeasure);
            }
        }
    }

    private void UnEquip()
    {
        foreach (var countermeasure in cms)
        {
            _countermeasureManager.countermeasures.Remove(countermeasure);
            var type = countermeasure.GetType();
            if (type == typeof(FlareCountermeasure))
            {
                _countermeasureManager.flareCMs.Remove(countermeasure as FlareCountermeasure);
            }
            else if (type == typeof(ChaffCountermeasure))
            {
                _countermeasureManager.chaffCMs.Remove(countermeasure as ChaffCountermeasure);
            }
        }
    }
    
    public override int GetCount()
    {
        var count = 0;
        foreach (var countermeasure in cms)
        {
            count += countermeasure.count;
        }

        return count;
    }

    public override int GetMaxCount()
    {
        var count = 0;
        foreach (var countermeasure in cms)
        {
            count += countermeasure.maxCount;
        }

        return count;
    }

    public float GetMass()
    {
        return mass;
    }
}