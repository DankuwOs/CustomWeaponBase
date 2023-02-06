using System.Collections;
using UnityEngine;
using static CustomWeaponBase.CWB_Utils.HandAnimatorOverride;

public class CWB_HandAnimation : MonoBehaviour
{
    public AnimationClip handAnimation;

    public VRInteractable interactable;

    private VRHandController _controller;

    private AnimationClip _oldAnimation;

    private void Start()
    {
        if (!interactable)
            interactable = GetComponent<VRInteractable>();

        interactable.OnStartInteraction += Vrint_OnStartInteraction;
        interactable.OnStopInteraction += Vrint_OnStopInteraction;
    }

    private void Vrint_OnStartInteraction(VRHandController controller)
    {
        _controller = controller;
    }
    
    private void Vrint_OnStopInteraction(VRHandController controller)
    {
        _controller = null;
    }

    public void PlayAnimation()
    {
        StartCoroutine(AnimatingRoutine());
    }

    private IEnumerator AnimatingRoutine()
    {
        var animator = SetGloveAnimationPose(_controller.gloveAnimation, handAnimation, out _oldAnimation);
        var controller = animator.runtimeAnimatorController as AnimatorOverrideController;
        
        var clip = controller?["ng_pointRelaxed"];

        var t = 0.0f;

        while (t <= clip.length)
        {
            t += Time.deltaTime;
        }
        
        RevertGloveAnimationPose(_controller.gloveAnimation, _oldAnimation);
        
        yield break;
    }
}