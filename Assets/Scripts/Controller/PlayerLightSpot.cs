using UnityEngine;
using Zenject;

public class PlayerLightSpot : MonoBehaviour
{
    [SerializeField] private GameObject lightSpot;
    
    [Inject] private readonly CharacterInputHandler _characterInputHandler;

    private bool _enabled;

    private void OnEnable()
    {
        _characterInputHandler.LightSpotAction += EnableSpot;
    }

    private void OnDisable()
    {
        _characterInputHandler.LightSpotAction -= EnableSpot;
    }

    private void EnableSpot()
    {
        lightSpot.SetActive(!_enabled);
        _enabled = !_enabled;
    }
}
