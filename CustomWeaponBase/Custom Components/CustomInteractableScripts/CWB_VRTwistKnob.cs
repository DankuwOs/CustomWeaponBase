using System.Collections;
using UnityEngine;

namespace CustomWeaponBase.Custom_Components.CustomInteractableScripts;

public class CWB_VRTwistKnob : VRTwistKnob
{
    public override IEnumerator Start()
    {
        vrint = GetComponent<VRInteractable>();
        vrint.OnStartInteraction += CWB_Vrint_OnStartInteraction;
        vrint.OnStopInteraction += CWB_Vrint_OnStopInteraction;
        clampedValue = Mathf.Clamp01(startValue);
        value = clampedValue;
        knobTransform.localRotation = Quaternion.Euler(0f, clampedValue * twistRange, 0f);
        lockTransform = new GameObject("knobLockTransform").transform;
        lockTransform.parent = knobTransform;
        yield return null;
        if (gameSettingsPersistent)
        {
            LoadFromGameSettings();
        }
        SetKnobValue(value);
    }

    public void CWB_Vrint_OnStartInteraction(VRHandController controller)
    {
        grabbed = true;
        ctrlr = controller;
        startUp = ctrlr.transform.InverseTransformDirection(knobTransform.parent.forward);
        StartCoroutine(GrabbedRoutine());
    }

    public void CWB_Vrint_OnStopInteraction(VRHandController controller)
    {
        grabbed = false;
        value = clampedValue;
    }
}