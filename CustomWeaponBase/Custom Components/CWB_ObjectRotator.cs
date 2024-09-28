using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using VTOLVR.DLC.Rotorcraft;

namespace CustomWeaponBase.Custom_Components;

public class CWB_ObjectRotator : MonoBehaviour
{
    public Transform rotationTf;
    public Vector3 rotationAxis = Vector3.forward;
    
    public float targetRPM = 3000f;
    [Tooltip("Applies a curve to the T value. 0-1 ( RPM = rpmCurve(t / accelTime) * targetRPM )")]
    public AnimationCurve rpmCurve;

    [Tooltip("Time it takes to get to full rpm")]
    public float accelTime = 5f;
    [Tooltip("Multiplier of time to decelerate (t -= dT * mult)")]
    public float decelMult = 1;

    [Header("Ignore names, custom structs / classes are impossible.")]
    [Header("Max RPM = RPM those objects will appear at.")]
    [Header("CommonObjects = Objects enabled at that RPM")]
    [Header("Don't use the other fields.")]
    public List<RotorRPMObjectSwitch.ObjectGroup> rpmGroups = [];

    [Header("Multiplies the current rpmgroup rpm by this")]
    public List<float> rpmMult;
    
    [Tooltip("Used with animations, can also use 'Start/Stop Rotating' methods from a unity event.")]
    public bool rotate;

    public float RPM => rpmCurve.Evaluate(_t / accelTime) * targetRPM;

    private float _t;

    private void Update()
    {
        var dT = Time.deltaTime;
        if (!rotate)
            dT = -dT * decelMult;
        var t = Mathf.MoveTowards(_t, rotate ? accelTime : 0, dT);
        _t = Mathf.Clamp(t, 0, accelTime);

        var rpm = RPM;
        float mult = 1;
        for (int i = 0; i < rpmGroups.Count; i++)
        {
            var rpmGroup = rpmGroups[i];
            var last = i + 1 >= rpmGroups.Count;
            if (rpm >= rpmGroup.maxRpm && (last || rpmGroups[i + 1].maxRpm > rpm))
            {
                foreach (var commonObject in rpmGroup.commonObjects)
                {
                    commonObject.SetActive(true);
                    mult = rpmMult[i];
                }
            }
            else
            {
                foreach (var commonObject in rpmGroup.commonObjects)
                {
                    commonObject.SetActive(false);
                }
            }
        }
        
        rotationTf.Rotate(rotationAxis, RPM * 6 * mult * Time.deltaTime);
    }
}