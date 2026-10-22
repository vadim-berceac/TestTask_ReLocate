using UnityEngine;

public class AnimatorID
{
    public readonly int AnimIDSpeed;
    public readonly int AnimIDGrounded;
    public readonly int AnimIDJump;
    public readonly int AnimIDFreeFall;
    public readonly int AnimIDMotionSpeed;
    public readonly int AnimIDAttack;
    public readonly int AnimIDHitStopTrigger;
    public readonly string AttackStaticTag = "AttackStaticTag";
    
    private AnimatorID()
    {
        AnimIDSpeed = Animator.StringToHash("Speed");
        AnimIDGrounded = Animator.StringToHash("Grounded");
        AnimIDJump = Animator.StringToHash("Jump");
        AnimIDFreeFall = Animator.StringToHash("FreeFall");
        AnimIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        AnimIDAttack = Animator.StringToHash("Attack");
        AnimIDHitStopTrigger = Animator.StringToHash("HitStopTrigger");
    }
}
