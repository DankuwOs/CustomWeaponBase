using System.Collections;
using UnityEngine;

public class RotaryComponent : MonoBehaviour
{
    [Header("Rotary Options")]
    public Transform rotaryTransform;

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
    
    private IEnumerator Rotate()
    {
        Debug.Log("Trying to rotate");
        var rotationAmount = rotation;
        if (fireCount >= secondaryRotationNumber && doSecondaryRotation)
            rotationAmount = secondaryRotation;
        
        var targetRotation = Quaternion.Euler(rotaryTransform.localRotation.eulerAngles + new Vector3(0, 0, rotationAmount));
        
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