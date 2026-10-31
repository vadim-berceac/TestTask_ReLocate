using ReLocate.Debug;
using StarterAssets;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container
            .Bind<StarterAssetsInputs>()
            .FromComponentInNewPrefabResource("Input/PlayerInput")
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<FPSMonitor>()
            .FromComponentInNewPrefabResource("Utility/FPSMonitor")
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<DamageMonitor>()
            .FromComponentInNewPrefabResource("Utility/DamageMonitor")
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<CameraSystem>()
            .FromComponentInNewPrefabResource("Camera/CameraSystem")
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<DamageMask>()
            .FromScriptableObjectResource("Data/DamageMask")
            .AsSingle()
            .NonLazy();
        
        Container
            .BindInterfacesAndSelfTo<CharacterInputHandler>()
            .AsSingle()
            .NonLazy();
        
        Container
            .BindInterfacesAndSelfTo<AnimatorID>()
            .AsSingle()
            .NonLazy();
    }
}