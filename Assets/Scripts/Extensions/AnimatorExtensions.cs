using UnityEngine;

public static class AnimatorExtensions
{
    public static bool IsInTag(this Animator animator, string tag, int layer = 0)
    {
        if (animator.GetCurrentAnimatorStateInfo(layer).IsTag(tag))
            return true;

        return animator.IsInTransition(layer)
               && animator.GetNextAnimatorStateInfo(layer).IsTag(tag);
    }
}