using UnityEngine;
using Zenject;

public class FootstepsController : MonoBehaviour
{
    [field: SerializeField] public AudioClip LandingAudioClip { get; private set; }
    [field: SerializeField] public AudioClip[] FootstepAudioClips { get; private set; }
    [field: SerializeField, Range(0, 1)] public float FootstepAudioVolume { get; private set; }= 0.5f;
    
    [Inject] private readonly Transform _root;
    [Inject] private readonly CharacterController _controller;
    
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight <= 0.5f)
        {
            return;
        }
        
        if (FootstepAudioClips.Length == 0)
        {
           return;
        }
        
        var index = Random.Range(0, FootstepAudioClips.Length);
        AudioSource.PlayClipAtPoint(FootstepAudioClips[index], _root.TransformPoint(_controller.center), FootstepAudioVolume);
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f && LandingAudioClip != null)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, _root.TransformPoint(_controller.center), FootstepAudioVolume);
        }
    }
}
