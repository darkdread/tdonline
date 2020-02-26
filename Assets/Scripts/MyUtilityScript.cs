using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyUtilityScript
{
    public static bool IsInBounds(Bounds bounds1, Bounds bounds2){
        float bounds2CenterX = bounds2.center.x;
        
        if (bounds1.min.x > bounds2CenterX || bounds1.max.x < bounds2CenterX){
            Debug.Log("1");
            return false;
        }

        float bounds2CenterY = bounds2.center.y;
        
        if (bounds1.min.y > bounds2CenterY || bounds1.max.y < bounds2CenterY){
            return false;
        }

        return true;
    }

    public static float GetAnimationDuration(Animator animator, string clipName){
        AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;
        
        foreach(AnimationClip animationClip in animationClips){

            // We have to search here because the clip names have its
            // prefix of the game object.
            if (animationClip.name.Contains(clipName)){
                return animationClip.length;
            }
        }

        return 0;
    }
}
