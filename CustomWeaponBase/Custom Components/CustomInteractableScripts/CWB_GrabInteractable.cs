using System;
using System.Collections;
using UnityEngine;

public class CWB_GrabInteractable : MonoBehaviour
{
    public float lerpSpeed;

    public float rotationLerpSpeed;

    public VRInteractable interactable;

    [Header("Pivot")]
    [Tooltip("Moves the pivot point to the controllers center instead of your fingertip.")]
    public bool controllerCenter = true;

    public bool controllerRotation = true;

    [Tooltip("Adds this rotation to your object, only used while Controller Center is true.")]
    public Vector3 grabbedRotation;

    [Header("Return")] 
    public Transform returnTransform;

    public float returnSpeed;

    public event Action<ObjectPosition> OnSetPosition;

    private Transform _controllerTf;
	
    private Vector3 _localPos;

    private Quaternion _localRot;

    private bool _grabbed;
    
    // Relative to newGlove. I don't know what im doing :~)
    private static readonly Vector3 LocalPalmPosition = new Vector3(0.000797271729f,-0.0196437836f,0.112915039f);
    
    public virtual void Start()
    {
        if (!interactable)
        {
            interactable = GetComponent<VRInteractable>();
        }
        controllerRotation = controllerRotation || controllerCenter;
        

        interactable.OnStartInteraction += Ir_OnStartInteraction;
        interactable.OnStopInteraction += Ir_OnStopInteraction;
    }

    public virtual void Ir_OnStartInteraction(VRHandController controller)
    {
        _grabbed = true;

        _controllerTf = controller.transform;

        var centerPosition = Vector3.zero;
        var centerRotation = Quaternion.identity;
        
        var centerTf = _controllerTf.Find("newGlove");

        if (controllerCenter)
        {
            centerPosition = centerTf.TransformPoint(LocalPalmPosition);
        }

        if (controllerRotation)
        {
            centerRotation = Quaternion.Inverse(_controllerTf.rotation) * centerTf.rotation;
            centerRotation *= Quaternion.Euler(grabbedRotation);
        }

        _localPos = controllerCenter ? _controllerTf.InverseTransformPoint(centerPosition) : _controllerTf.InverseTransformPoint(transform.position);
        _localRot = controllerRotation ? centerRotation : Quaternion.Inverse(_controllerTf.rotation) * transform.rotation;
        
        
        StartCoroutine(HeldRoutine());
    }

    private IEnumerator HeldRoutine()
    {
        while (_grabbed)
        {
            transform.position = Vector3.Lerp(transform.position, _controllerTf.TransformPoint(_localPos), lerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, _controllerTf.rotation * _localRot, rotationLerpSpeed * Time.deltaTime);

            if (OnSetPosition != null)
            {
                var objectPosition = new ObjectPosition()
                {
                    position = VTMapManager.WorldToGlobalPoint(transform.position),
                    rotation = transform.rotation
                };
                OnSetPosition.Invoke(objectPosition);
            }
            
            yield return null;
        }
		
		
    }

    public void RemoteSetPosition(Vector3D position, Quaternion rotation)
    {
        transform.position = VTMapManager.GlobalToWorldPoint(position);
        transform.rotation = rotation;
    }

    public virtual void Ir_OnStopInteraction(VRHandController controller)
    {
        _grabbed = false;

        if (returnTransform)
            StartCoroutine(ReturnRoutine());
    }

    private IEnumerator ReturnRoutine()
    {
        while (_grabbed)
        {
            transform.position = Vector3.Lerp(transform.position, returnTransform.position, returnSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, returnTransform.rotation, returnSpeed / 2);
            
            if (OnSetPosition != null)
            {
                var objectPosition = new ObjectPosition()
                {
                    position = VTMapManager.WorldToGlobalPoint(transform.position),
                    rotation = transform.rotation
                };
                OnSetPosition.Invoke(objectPosition);
            }
            
            if (Vector3.Distance(transform.position, returnTransform.position) < 0.001)
                yield break;
            
            yield return null;
        }
    }

    public class ObjectPosition
    {
        public Vector3D position;
        public Quaternion rotation;
    }
}