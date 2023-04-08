using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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

        SetupEvents(controller);

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
        
        RemoveEvents(controller);

        if (returnTransform)
            StartCoroutine(ReturnRoutine());
    }

    private IEnumerator ReturnRoutine()
    {
        while (true)
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

    #region Controller Events


    [Header("Controller Button Events")] 
    public UnityEvent<VRHandController> OnGripPressed = new();
    public UnityEvent<VRHandController> OnGripReleased = new();
    public UnityEvent<VRHandController> OnGripForcePressed = new();
    public UnityEvent<VRHandController> OnGripForceReleased = new();
    public UnityEvent<VRHandController, Vector2> OnStickAxis = new();
    public UnityEvent<VRHandController> OnStickPressed = new();
    public UnityEvent<VRHandController> OnStickUnpressed = new();
    public UnityEvent<VRHandController> OnStickTouched = new();
    public UnityEvent<VRHandController> OnStickUntouched = new();
    public UnityEvent<VRHandController, float> OnTriggerAxis = new();
    public UnityEvent<VRHandController> OnTriggerClicked = new();
    public UnityEvent<VRHandController> OnTriggerClickReleased = new();
    public UnityEvent<VRHandController> OnThumbButtonPressed = new();
    public UnityEvent<VRHandController> OnThumbButtonReleased = new();
    public UnityEvent<VRHandController> OnSecondaryThumbButtonPressed = new();
    public UnityEvent<VRHandController> OnSecondaryThumbButtonReleased = new();
    public UnityEvent<VRHandController> OnTriggerStageOnePressed = new();
    public UnityEvent<VRHandController> OnTriggerStageOneReleased = new();

    private void SetupEvents(VRHandController controller)
    {
        if (!controller)
            throw new NullReferenceException("[CWB_GrabInteractable]: Controller null!");

        controller.OnGripPressed += ControllerOnGripPressed;
        controller.OnGripReleased += ControllerOnGripReleased;
        controller.OnGripForcePressed += ControllerOnGripForcePressed;
        controller.OnGripForceReleased += ControllerOnGripForceReleased;
        controller.OnStickAxis += ControllerOnStickAxis;
        controller.OnStickPressed += ControllerOnStickPressed;
        controller.OnStickUnpressed += ControllerOnStickUnpressed;
        controller.OnStickTouched += ControllerOnStickTouched;
        controller.OnStickUntouched += ControllerOnStickUntouched;
        controller.OnTriggerAxis += ControllerOnTriggerAxis;
        controller.OnTriggerClicked += ControllerOnTriggerClicked;
        controller.OnTriggerClickReleased += ControllerOnTriggerClickReleased;
        controller.OnThumbButtonPressed += ControllerOnThumbButtonPressed;
        controller.OnThumbButtonReleased += ControllerOnThumbButtonReleased;
        controller.OnSecondaryThumbButtonPressed += ControllerOnSecondaryThumbButtonPressed;
        controller.OnSecondaryThumbButtonReleased += ControllerOnSecondaryThumbButtonReleased;
        controller.OnTriggerStageOnePressed += ControllerOnTriggerStageOnePressed;
        controller.OnTriggerStageOneReleased += ControllerOnTriggerStageOneReleased;
    }

    private void RemoveEvents(VRHandController controller)
    {
        if (!controller)
            throw new NullReferenceException("[CWB_GrabInteractable]: Controller null!");
        
        controller.OnGripPressed -= ControllerOnGripPressed;
        controller.OnGripReleased -= ControllerOnGripReleased;
        controller.OnGripForcePressed -= ControllerOnGripForcePressed;
        controller.OnGripForceReleased -= ControllerOnGripForceReleased;
        controller.OnStickAxis -= ControllerOnStickAxis;
        controller.OnStickPressed -= ControllerOnStickPressed;
        controller.OnStickUnpressed -= ControllerOnStickUnpressed;
        controller.OnStickTouched -= ControllerOnStickTouched;
        controller.OnStickUntouched -= ControllerOnStickUntouched;
        controller.OnTriggerAxis -= ControllerOnTriggerAxis;
        controller.OnTriggerClicked -= ControllerOnTriggerClicked;
        controller.OnTriggerClickReleased -= ControllerOnTriggerClickReleased;
        controller.OnThumbButtonPressed -= ControllerOnThumbButtonPressed;
        controller.OnThumbButtonReleased -= ControllerOnThumbButtonReleased;
        controller.OnSecondaryThumbButtonPressed -= ControllerOnSecondaryThumbButtonPressed;
        controller.OnSecondaryThumbButtonReleased -= ControllerOnSecondaryThumbButtonReleased;
        controller.OnTriggerStageOnePressed -= ControllerOnTriggerStageOnePressed;
        controller.OnTriggerStageOneReleased -= ControllerOnTriggerStageOneReleased;
    }
    
    
    
    private void ControllerOnGripPressed(VRHandController ctrlr)
    {
        OnGripPressed.Invoke(ctrlr);
    }

    private void ControllerOnGripReleased(VRHandController ctrlr)
    {
        OnGripReleased.Invoke(ctrlr);
    }

    private void ControllerOnGripForcePressed(VRHandController ctrlr)
    {
        OnGripForcePressed.Invoke(ctrlr);
    }

    private void ControllerOnGripForceReleased(VRHandController ctrlr)
    {
        OnGripForceReleased.Invoke(ctrlr);
    }

    private void ControllerOnStickAxis(VRHandController ctrlr, Vector2 vector)
    {
        OnStickAxis.Invoke(ctrlr, vector);
    }

    private void ControllerOnStickPressed(VRHandController ctrlr)
    {
        OnStickPressed.Invoke(ctrlr);
    }

    private void ControllerOnStickUnpressed(VRHandController ctrlr)
    {
        OnStickUnpressed.Invoke(ctrlr);
    }

    private void ControllerOnStickTouched(VRHandController ctrlr)
    {
        OnStickTouched.Invoke(ctrlr);
    }

    private void ControllerOnStickUntouched(VRHandController ctrlr)
    {
        OnStickUntouched.Invoke(ctrlr);
    }

    private void ControllerOnTriggerAxis(VRHandController ctrlr, float axis)
    {
        OnTriggerAxis.Invoke(ctrlr, axis);
    }

    private void ControllerOnTriggerClicked(VRHandController ctrlr)
    {
        OnTriggerClicked.Invoke(ctrlr);
    }

    private void ControllerOnTriggerClickReleased(VRHandController ctrlr)
    {
        OnTriggerClickReleased.Invoke(ctrlr);
    }

    private void ControllerOnThumbButtonPressed(VRHandController ctrlr)
    {
        OnThumbButtonPressed.Invoke(ctrlr);
    }

    private void ControllerOnThumbButtonReleased(VRHandController ctrlr)
    {
        OnThumbButtonReleased.Invoke(ctrlr);
    }

    private void ControllerOnSecondaryThumbButtonPressed(VRHandController ctrlr)
    {
        OnSecondaryThumbButtonPressed.Invoke(ctrlr);
    }

    private void ControllerOnSecondaryThumbButtonReleased(VRHandController ctrlr)
    {
        OnSecondaryThumbButtonReleased.Invoke(ctrlr);
    }

    private void ControllerOnTriggerStageOnePressed(VRHandController ctrlr)
    {
        OnTriggerStageOnePressed.Invoke(ctrlr);
    }

    private void ControllerOnTriggerStageOneReleased(VRHandController ctrlr)
    {
        OnTriggerStageOneReleased.Invoke(ctrlr);
    }
    
    #endregion

    public class ObjectPosition
    {
        public Vector3D position;
        public Quaternion rotation;
    }
}