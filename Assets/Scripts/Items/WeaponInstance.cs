using UnityEngine;

public class WeaponInstance : WearableItemInstance
{
    private readonly AudioSource _weaponAudioSource;
    private readonly WeaponData _weaponData;
    
    public WeaponInstance(PropBones propBones, WeaponData data)
        : base(propBones, data)
    {
        _weaponData = data;
        
        if (Instances != null && Instances.Length > 0)
        {
            _weaponAudioSource = Instances[0].AddComponent<AudioSource>();
            _weaponAudioSource.playOnAwake = false;
        }
    }

    public void PlaySwitchPositionSound()
    {
        if (!_weaponData.SwitchPositionSound)
        {
            return;
        }
        PlayAudio(_weaponData.SwitchPositionSound);
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
}
