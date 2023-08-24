using Steamworks.Data;
using UnityEngine;
using static CustomWeaponBase.CWB_Utils.HandAnimatorOverride;


public class CWB_VRIntGlovePoser : MonoBehaviour
{
    [Tooltip("Transform to lock the hand to whilst interacting.")]
    public Transform lockTransform;

    [Tooltip("If the animation is based on left hand use this I believe")]
    public bool referencedLeft;
    
    [Header("Left Hand Transform")]
    [Tooltip("Left hand local position, (1 = normal | -1 = invert)")]
    public Vector3 inversePosition = new Vector3(-1, 1, 1);

    [Tooltip("Left hand local angles, (1 = normal | -1 = invert)")]
    public Vector3 inverseEulerAngles = new Vector3(1, -1, -1);
    
    [Header("Custom Animations")]
    [Tooltip("Optional")]
    public AnimationClip hoverPoseOverride;

    [Tooltip("Optional")]
    public AnimationClip interactionPoseOverride;
    
    [Header("Fallback Poses")]
    public GloveAnimation.Poses hoverPose;
    
    public GloveAnimation.Poses interactionPose;

    private AnimationClip vanillaHoverClip;
    
    private AnimationClip vanillaInteractClip;
    
    [HideInInspector]
    public Transform leftLockTransform;

    public void Start()
    {
        if (lockTransform)
        {
            leftLockTransform = new GameObject(lockTransform.name + "_LEFT").transform;
            leftLockTransform.parent = lockTransform.parent;
            leftLockTransform.localScale = lockTransform.localScale;
            
            // Inverting position, angles with your own things, if the parent for the lockTf was rotated 90° or so you'd invert the positions y instead of x or something like that.

            Vector3 localPosition = lockTransform.localPosition;
            localPosition.x *= inversePosition.x;
            localPosition.y *= inversePosition.y;
            localPosition.z *= inversePosition.z;
            
            leftLockTransform.localPosition = localPosition;
            
            Vector3 localEulerAngles = lockTransform.localEulerAngles;
            localEulerAngles.x *= inverseEulerAngles.x;
            localEulerAngles.y *= inverseEulerAngles.y;
            localEulerAngles.z *= inverseEulerAngles.z;

            leftLockTransform.localEulerAngles = localEulerAngles;
            
            
            if (referencedLeft)
            {
                Transform transform = lockTransform;
                lockTransform = leftLockTransform;
                leftLockTransform = transform;
            }
        }
        VRInteractable component = GetComponentInChildren<VRInteractable>();
        component.OnStartInteraction += Vrint_OnStartInteraction;
        component.OnStopInteraction += Vrint_OnStopInteraction;
        if (!component.poseBounds)
        {
            component.OnHover += Vrint_OnHover;
            component.OnUnHover += Vrint_OnUnHover;
        }
    }

    public void Vrint_OnHover(VRHandController controller)
    {
        if (!controller.gloveAnimation)
            return;
        
        if (!hoverPoseOverride)
        {
            controller.gloveAnimation.SetPoseHover(hoverPose);
            return;
        }

        SetGloveAnimationPose(controller.gloveAnimation, hoverPoseOverride, out vanillaHoverClip, true);
    }

    
    public void Vrint_OnStartInteraction(VRHandController controller)
    {
        if (!controller.gloveAnimation)
            return;
        
        
        if (!interactionPoseOverride)
            controller.gloveAnimation.SetPoseInteractable(interactionPose);
        else
        {
            Debug.Log($"[CWB_VRIntGlovePoser.OnStartinteraction({gameObject.name})]: SetGloveAnimationPose for instance {controller.GetInstanceID()} / {controller.gameObject.name}");
            SetGloveAnimationPose(controller.gloveAnimation, interactionPoseOverride, out vanillaInteractClip);
        }

        if (lockTransform)
        {
            controller.gloveAnimation.SetLockTransform(controller.isLeft ? leftLockTransform : lockTransform);
        }
    }

    public void Vrint_OnStopInteraction(VRHandController controller)
    {
        if (!controller.gloveAnimation)
            return;
        
        controller.gloveAnimation.ClearInteractPose();

        RevertGloveAnimationPose(controller.gloveAnimation, vanillaInteractClip);
    }

    public void Vrint_OnUnHover(VRHandController controller)
    {
        if (!controller.gloveAnimation)
            return;

        if (!interactionPoseOverride)
        {
            controller.gloveAnimation.SetPoseHover(GloveAnimation.Poses.Idle);
            return;
        }

        RevertGloveAnimationPose(controller.gloveAnimation, vanillaHoverClip, true);
    }
    
    
    public void SetGloveInteract(GloveAnimation gloveAnimation)
    {
        if (!gloveAnimation)
            return;

        if (!interactionPoseOverride)
            gloveAnimation.SetPoseInteractable(interactionPose);
        else
        {
            Debug.Log($"[CWB_VRIntGlovePoser.SetGloveInteract({gameObject.name})]: SetGloveAnimationPose for instance {gloveAnimation.GetInstanceID()} / {gloveAnimation.gameObject.name}");
            SetGloveAnimationPose(gloveAnimation, interactionPoseOverride, out vanillaInteractClip);
        }
    }

    public void ClearGloveInteract(GloveAnimation gloveAnimation)
    {
        if (!gloveAnimation)
            return;
        
        gloveAnimation.ClearInteractPose();

        RevertGloveAnimationPose(gloveAnimation, vanillaInteractClip);
    }
}