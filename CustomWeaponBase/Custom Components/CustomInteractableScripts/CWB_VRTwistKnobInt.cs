using UnityEngine;

namespace CustomWeaponBase.Custom_Components.CustomInteractableScripts;

public class CWB_VRTwistKnobInt : VRTwistKnobInt
{
    public override void Initialize()
    {
        if (initialized)
            return;
        
        initialized = true;
        
        vrint = GetComponent<VRInteractable>();
        vrint.OnStartInteraction += CWB_Vrint_OnStartInteraction;
        vrint.OnStopInteraction += CWB_Vrint_OnStopInteraction;
        
        lockTransform = new GameObject("knobLockTransform").transform;
        lockTransform.parent = knobTransform;
        
        state = initialState;
        angleInterval = twistRange / (states - 1);
        
        rotations = new Quaternion[states];
        rotations[0] = Quaternion.identity;
        
        var angle = 0f;
        for (int i = 1; i < states; i++)
        {
            angle += angleInterval;
            rotations[i] = Quaternion.Euler(0f, angle, 0f);
        }
        
        knobTransform.localRotation = rotations[state];
        value = state / (float)(states - 1);
        
        if (reverse)
            value = 1f - value;
        
        SetState(state);
    }

    public void CWB_Vrint_OnStartInteraction(VRHandController controller)
    {
        grabbed = true;
        ctrlr = controller;
        startUp = ctrlr.transform.InverseTransformDirection(knobTransform.parent.forward);

        if (canPushButton)
            ctrlr.OnThumbButtonPressed += Ctrlr_OnThumbButtonPressed;
        
        StartCoroutine(GrabbedRoutine());
    }

    public void CWB_Vrint_OnStopInteraction(VRHandController controller)
    {
        grabbed = false;
        value = clampedValue;

        if (canPushButton)
            ctrlr.OnThumbButtonPressed -= Ctrlr_OnThumbButtonPressed;

        ctrlr = null;
        BeginUngrabRoutine();
    }
}