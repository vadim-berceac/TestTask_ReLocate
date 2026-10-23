using Tiny;
using UnityEngine;

public class WeaponInstance : WearableItemInstance
{
    private readonly AudioSource _weaponAudioSource;
    private readonly WeaponData _weaponData;
    private readonly WeaponDamagePoints _weaponDamagePoints;
    private readonly Trail _trail;
    
    public WeaponInstance(PropBones propBones, WeaponData data)
        : base(propBones, data)
    {
        _weaponData = data;
        
        if (Instances != null && Instances.Length > 0)
        {
            _weaponAudioSource = Instances[0].AddComponent<AudioSource>();
            _weaponAudioSource.playOnAwake = false;

            Instances[0].TryGetComponent(out _weaponDamagePoints);
            Instances[0].TryGetComponent(out _trail);

            if (_trail)
            {
                _trail.enabled = false;
            }

            if (!_weaponDamagePoints)
            {
                Debug.LogWarning("No damage points script on weapon instance!");
                return;
            }
            _weaponDamagePoints.OnHitStop += OnHitStop;
            _weaponDamagePoints.OnDamageDealt +=  OnOnDamage;
        }
    }
    
    public override void Dispose()
    {
        base.Dispose();
        
        _weaponDamagePoints.OnHitStop -= OnHitStop;
        _weaponDamagePoints.OnDamageDealt -=  OnOnDamage;
    }

    public void BeginAttack()
    {
        _weaponDamagePoints?.BeginAttack();
    }

    public void EndAttack()
    {
        _weaponDamagePoints?.EndAttack();
    }

    public void ShowTrail(bool show)
    {
        _trail.enabled = show;
    }

    public bool HasDealtDamageThisAttack => _weaponDamagePoints?.HasDealtDamageThisAttack ?? false;
    public bool HasHitColliderThisAttack => _weaponDamagePoints?.HasHitColliderThisAttack ?? false;

    public void SetDamageableLayer(LayerMask layer)
    {
        _weaponDamagePoints?.SetDamageableLayer(layer);
    }

    private void PlayHitStopSound()
    {
        if (_weaponData.HitStopSound)
        {
            PlayAudio(_weaponData.HitStopSound);
        }
    }

    public void PlayRandomSwingSound()
    {
        if (_weaponData.SwingSounds == null || _weaponData.SwingSounds.Length == 0)
        {
            return;
        }
        var randomIndex = Random.Range(0, _weaponData.SwingSounds.Length);
        PlayAudio(_weaponData.SwingSounds[randomIndex]);
    }

    private void PlayAudio(AudioClip clip)
    {
        _weaponAudioSource.clip = clip;
        _weaponAudioSource.Play();
    }

    private void OnOnDamage(HitData hitData)
    {
       
    }

    private void OnHitStop(HitData hitData)
    {
        PlayHitStopSound();
        if (!_weaponData.HitStopVFX)
        {
            return;
        }
        
        _weaponData.HitStopVFX.PlayAtHitData(hitData);
    }
}
