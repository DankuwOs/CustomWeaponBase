using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomWeaponBase.Custom_Components;

public class CWB_HardpointModifier : CWB_HPEquipExtension
{
    [Header("This component allows your equip to disable hardpoints from showing up and changing hardpoint locations.")]
    [Space]
    [Tooltip("Detach equips at these indexes and disable their nodes")]
    public int[] detachHpIdxs;

    public Transform[] hardpoints;
    
    
    private List<Tuple<Transform, Transform, Vector3, Quaternion>> _origHps = new ();

    private Dictionary<int, GameObject> _origOptHps = new ();
    
    
    
    

    public override void OnConfigAttach(LoadoutConfigurator configurator)
    {
        base.OnConfigAttach(configurator);

        foreach (var i in detachHpIdxs)
        {
            SetNodeEnabled(i, false, configurator);
        }
        
        InitCustomHps(configurator);
    }

    public override void OnConfigDetach(LoadoutConfigurator configurator)
    {
        base.OnConfigDetach(configurator);
        
        // Disabled the nodes.
        foreach (var i in detachHpIdxs)
        {
            SetNodeEnabled(i, true, configurator);
        }
        
        UndoCustomHps(configurator);
    }

    public override void OnEquip()
    {
        base.OnEquip();
        
        InitCustomHps();
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        
        UndoCustomHps();
    }

    private void InitCustomHps(LoadoutConfigurator configurator = null)
    {
        var wm = configurator ? configurator.wm : hpEquip.weaponManager;
        if (!wm)
            throw new NullReferenceException("[CWB_HardpointModifier]: WeaponManager null.");

        // Detach equips
        foreach (var idx in detachHpIdxs)
        {
            if (configurator)
            {
                configurator.DetachImmediate(idx);
            }
            else
            {
                if (wm.GetEquip(idx))
                    wm.JettisonEq(idx);
            }
        }
        
        if (configurator && configurator.uiOnly)
            return;

        var extOptHps = wm.GetComponentInChildren<ExternalOptionalHardpoints>();
        var extOptHpsTree = wm.GetComponentInChildren<ExternalOptionalHardpointTree>();

        // Setup custom hps
        if (hardpoints.Length > 0)
        {
            for (int i = 0; i < hardpoints.Length; i++)
            {
                var hp = hardpoints[i];
                var wmHp = wm.hardpointTransforms[i];

                if (!wmHp && !hp)
                    continue;
                
                _origHps.Add(new Tuple<Transform, Transform, Vector3, Quaternion>(wmHp, wmHp.parent, wmHp.localPosition, wmHp.localRotation));

                if (extOptHps && hardpoints[i])
                {
                    var hps = extOptHps.hardpoints;
                    foreach (var hardpoint in hps)
                    {
                        if (hardpoint.hpIdx != i)
                            continue;
                        
                        _origOptHps.Add(i, hardpoint.pylonModel);

                        hardpoint.pylonModel = hp.gameObject;
                    }
                    extOptHps.Refresh();
                }
                else if (extOptHpsTree)
                {
                    var hps = extOptHpsTree.hardpointObjects;
                    foreach (var hardpointObject in hps)
                    {
                        if (!hardpointObject.hardpoints.Contains(i))
                            continue;
                        
                        _origOptHps.Add(i, hardpointObject.gameObject);
                        hardpointObject.gameObject = hp.gameObject;
                    }
                    extOptHpsTree.Refresh();
                }

                wmHp.SetParent(hp);
                wmHp.localPosition = Vector3.zero;
                wmHp.localRotation = Quaternion.identity;
                
                if (!hp.GetComponent<VehiclePart>()) continue;

                var hpVp = hp.gameObject.GetComponent<HardpointVehiclePart>();
                hpVp ??= hp.gameObject.AddComponent<HardpointVehiclePart>();
                hpVp.wm = wm;
                hpVp.hpIdx = i;
            }
        }
    }

    private void UndoCustomHps(LoadoutConfigurator configurator = null)
    {
        var wm = configurator ? configurator.wm : hpEquip.weaponManager;
        if (!wm || (configurator && configurator.uiOnly))
            return;
        
        if (_origHps.Count > 0)
        {
            foreach (var origHp in _origHps)
            {
                if (!origHp.Item1 || !origHp.Item2)
                    continue;

                var tf = origHp.Item1;
                tf.SetParent(origHp.Item2);
                tf.localPosition = origHp.Item3;
                tf.localRotation = origHp.Item4;
            }
        }
        
        if (_origOptHps.Count == 0)
            return;
        
        var extOptHps = wm.GetComponentInChildren<ExternalOptionalHardpoints>();
        var extOptHpsTree = wm.GetComponentInChildren<ExternalOptionalHardpointTree>();

        if (extOptHps)
        {
            foreach (var hardpoint in extOptHps.hardpoints)
            {
                if (_origOptHps.TryGetValue(hardpoint.hpIdx, out var hp))
                    hardpoint.pylonModel = hp;
            }
            extOptHps.Refresh();
        }
        else if (extOptHpsTree)
        {
            foreach (var hp in extOptHpsTree.hardpointObjects)
            {
                foreach (var hpHardpoint in hp.hardpoints)
                {
                    if (_origOptHps.TryGetValue(hpHardpoint, out var value))
                    {
                        hp.gameObject = value;
                        break;
                    }
                }
            }
            extOptHpsTree.Refresh();
        }
    }

    private void SetNodeEnabled(int idx, bool enable, LoadoutConfigurator configurator)
    {
        var go = configurator.hpNodes[idx].gameObject;
        go.SetActive(enable);
    }
}