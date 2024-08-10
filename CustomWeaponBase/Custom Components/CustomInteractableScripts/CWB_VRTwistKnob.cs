using UnityEngine;

namespace CustomWeaponBase.Custom_Components.CustomInteractableScripts;

public class CWB_VRTwistKnob : VRTwistKnob
{
    public override void Vrint_OnStartInteraction(VRHandController controller)
    {
        grabbed = true;
        ctrlr = controller;
        startUp = ctrlr.transform.InverseTransformDirection(knobTransform.parent.forward);
        StartCoroutine(GrabbedRoutine());
    }

    public override void Vrint_OnStopInteraction(VRHandController controller)
    {
        grabbed = false;
        value = clampedValue;
    }
}