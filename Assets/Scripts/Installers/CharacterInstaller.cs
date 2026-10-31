using UnityEngine;
using Zenject;

public class CharacterInstaller : MonoInstaller
{
    [SerializeField] private Transform characterTransform;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    [SerializeField] private PropBones propBones;
    [SerializeField] private CameraRoot cameraRoot;
    
    public override void InstallBindings()
    {
        Container
            .Bind<Transform>()
            .FromInstance(characterTransform)
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<CharacterController>()
            .FromInstance(characterController)
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<Animator>()
            .FromInstance(animator)
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<PropBones>()
            .FromInstance(propBones)
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<CameraRoot>()
            .FromInstance(cameraRoot)
            .AsSingle()
            .NonLazy();
    }
}