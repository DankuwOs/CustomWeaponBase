namespace CustomWeaponBase.Custom_Components.CustomInteractableScripts;

public class CWB_VRTwistKnobInt : VRTwistKnobInt
{
    public override void Vrint_OnStartInteraction(VRHandController controller)
    {
        grabbed = true;
        ctrlr = controller;
        startUp = ctrlr.transform.InverseTransformDirection(knobTransform.parent.forward);

        if (canPushButton)
            ctrlr.OnThumbButtonPressed += Ctrlr_OnThumbButtonPressed;
        
        StartCoroutine(GrabbedRoutine());
    }

    public override void Vrint_OnStopInteraction(VRHandController controller)
    {
        grabbed = false;
        value = clampedValue;

        if (canPushButton)
            ctrlr.OnThumbButtonPressed -= Ctrlr_OnThumbButtonPressed;

        ctrlr = null;
        BeginUngrabRoutine();
    }
}