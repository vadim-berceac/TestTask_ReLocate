using System;
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
    public bool LightSpot => _starterAssetsInputs.LightSpot;
    
    public bool AnalogMovement => _starterAssetsInputs.AnalogMovement;
    
    public Action LightSpotAction
    {
        get { return _starterAssetsInputs.LightSpotAction; }
        set { _starterAssetsInputs.LightSpotAction = value; }
    }

    public void ResetJump()
    {
        _starterAssetsInputs.ResetJump();
    }

    public void ResetLightSpot()
    {
        _starterAssetsInputs.ResetLightSpot();
    }
}
