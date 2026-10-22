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
            .BindInterfacesAndSelfTo<CharacterInputHandler>()
            .AsSingle()
            .NonLazy();
        
        Container
            .BindInterfacesAndSelfTo<AnimatorID>()
            .AsSingle()
            .NonLazy();
    }
}