using System;
using System.Linq;
using UnityEngine;

namespace CustomWeaponBase.CWB_Utils;

public static class HandAnimatorOverride
{
    public static Animator SetGloveAnimationPose(GloveAnimation gloveAnimation, AnimationClip clip, out AnimationClip outClip,
        bool hover = false)
    {
        // I lost too much time to this goddamn shit. i was using ng_point this whole time..
        var poseName = hover ? "ng_pinch" : "ng_pointRelaxed";

        outClip = gloveAnimation.animator.runtimeAnimatorController.animationClips.FirstOrDefault(e => e.name == poseName);
        if (outClip == null)
            throw new NullReferenceException("[CWB]: outClip is null!");

        if (gloveAnimation.animator.runtimeAnimatorController is not AnimatorOverrideController)
        {
            var animatorController = new AnimatorOverrideController(gloveAnimation.animator.runtimeAnimatorController);
            gloveAnimation.animator.runtimeAnimatorController = animatorController;
        }

        var overrideAnimController = gloveAnimation.animator.runtimeAnimatorController as AnimatorOverrideController;

        
        if (overrideAnimController == null)
        {
            Debug.Log("[CWB]: Override anim controller null!!");
            return null;
        }
        
        
        overrideAnimController[poseName] = clip;

        if (hover)
            gloveAnimation.SetPoseHover(GloveAnimation.Poses.Pinch);
        else
            gloveAnimation.SetPoseInteractable(GloveAnimation.Poses.Point);

        return gloveAnimation.animator;
    }

    public static void RevertGloveAnimationPose(GloveAnimation gloveAnimation, AnimationClip clip, bool hover = false)
    {
        var poseName = hover ? "ng_pinch" : "ng_pointRelaxed";
        
        AnimatorOverrideController overrideAnimController = null;

        if (gloveAnimation.animator.runtimeAnimatorController is not AnimatorOverrideController) // Convert runtimeAnimController to an override to do thing, why is it not?
        {
            var animatorController = new AnimatorOverrideController(gloveAnimation.animator.runtimeAnimatorController);
            gloveAnimation.animator.runtimeAnimatorController = animatorController;
        }

        overrideAnimController = gloveAnimation.animator.runtimeAnimatorController as AnimatorOverrideController;
        
        if (overrideAnimController == null)
        {
            Debug.Log("[CWB]: Override anim controller null!!");
            return;
        }
        
        overrideAnimController[poseName] = clip;
    }
}