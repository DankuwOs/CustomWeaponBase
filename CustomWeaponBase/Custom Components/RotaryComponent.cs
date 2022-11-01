using System;
using System.Collections;
using UnityEngine;

public class RotaryComponent : MonoBehaviour
{
    [Header("Rotary Options")]
    public Transform rotaryTransform;

    public Vector3 rotationAxis;

    public float rotation;

    public bool doSecondaryRotation;

    [Tooltip("This amount fired and above uses Secondary Rotation")]
    public int secondaryRotationNumber;

    public float secondaryRotation;

    public float rotationTime;

    public AnimationCurve rotationCurve;

    [HideInInspector] public int fireCount;

    private float _time;
    
    [ContextMenu("Test Rotation")]
    public void StartRotating() => StartCoroutine(Rotate());

    private void Update()
    {
        if (Input.GetKey(KeyCode.K) && Input.GetKeyDown(KeyCode.Alpha8))
            StartRotating();
    }

    private IEnumerator Rotate()
    {
        var rotationAmount = rotation;
        if (fireCount >= secondaryRotationNumber && doSecondaryRotation)
            rotationAmount = secondaryRotation;

        var targetRotation = Quaternion.Euler(rotaryTransform.localRotation.eulerAngles + rotationAxis * rotationAmount);
        
        float rotateTime = 0f;
        while (true)
        {
            rotaryTransform.localRotation = Quaternion.Euler(Vector3.Lerp(rotaryTransform.localRotation.eulerAngles, targetRotation.eulerAngles, rotationCurve.Evaluate(rotateTime / rotationTime)));
            if (rotateTime >= rotationTime)
                yield break;
            
            rotateTime += Time.deltaTime;
            yield return null;
        }
    }
}