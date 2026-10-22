using StarterAssets;
using UnityEngine;
using Zenject;

public class CharacterInputHandler
{
    [Inject] private readonly StarterAssetsInputs _starterAssetsInputs;
    public Vector2 Move => _starterAssetsInputs.Move;
    public Vector2 Look => _starterAssetsInputs.Look;
    public bool Jump => _starterAssetsInputs.Jump;
    public bool Sprint => _starterAssetsInputs.Sprint;
    public bool Attack => _starterAssetsInputs.Attack;
    
    public bool AnalogMovement => _starterAssetsInputs.AnalogMovement;
    
    public void ResetJump()
    {
        _starterAssetsInputs.ResetJump();
    }
}
