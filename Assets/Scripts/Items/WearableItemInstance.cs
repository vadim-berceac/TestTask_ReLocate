using System;
using UnityEngine;

public class WearableItemInstance : IDisposable
{
    private readonly PropBones _propBones;
    protected readonly WearableItemData Data;

    protected readonly GameObject[] Instances;
    private bool _disposed;
    
    public int CurrentPositionIndex { get; protected set; }

    public WearableItemInstance(PropBones propBones, WearableItemData data, int startPositionIndex = 0)
    {
        _propBones = propBones ?? throw new ArgumentNullException(nameof(propBones));
        Data = data ?? throw new ArgumentNullException(nameof(data));

        CurrentPositionIndex = startPositionIndex;

        var elements = Data.WearableElements;
        Instances = new GameObject[elements?.Length ?? 0];

        if (elements == null)
        {
            return;
        }

        for (var i = 0; i < elements.Length; i++)
        {
            var settings = elements[i];
            if (settings == null || settings.Prefab == null)
            {
                continue;
            }

            var instance = UnityEngine.Object.Instantiate(settings.Prefab);
            instance.name = settings.Prefab.name;
            Instances[i] = instance;

            ApplyPosition(instance, settings, CurrentPositionIndex);
        }
    }

   
    public virtual void SwitchPosition(int positionIndex)
    {
        if (_disposed)
        {
            return;
        }

        var elements = Data.WearableElements;
        if (elements == null)
        {
            return;
        }

        CurrentPositionIndex = positionIndex;

        for (var i = 0; i < elements.Length; i++)
        {
            var instance = Instances[i];
            if (instance == null)
            {
                continue;
            }

            ApplyPosition(instance, elements[i], positionIndex);
        }
    }
    
    public void SwitchToNextPosition()
    {
        var count = GetMaxPositionsCount();
        if (count == 0)
        {
            return;
        }

        SwitchPosition((CurrentPositionIndex + 1) % count);
    }

    private int GetMaxPositionsCount()
    {
        var elements = Data.WearableElements;
        if (elements == null)
        {
            return 0;
        }

        var max = 0;
        foreach (var element in elements)
        {
            var length = element?.Positions?.Length ?? 0;
            if (length > max)
            {
                max = length;
            }
        }

        return max;
    }

    private void ApplyPosition(GameObject instance, WearableItemSettings settings, int positionIndex)
    {
        var positions = settings.Positions;
        if (positions == null || positionIndex < 0 || positionIndex >= positions.Length)
        {
            Debug.LogWarning($"WearableItemInstance: position index {positionIndex} is out of range " +
                             $"(count: {positions?.Length ?? 0}) for '{instance.name}'.");
            return;
        }

        var propBoneSettings = positions[positionIndex];
        if (propBoneSettings == null)
        {
            Debug.LogWarning($"WearableItemInstance: missing settings for position {positionIndex} of '{instance.name}'.");
            return;
        }

        var bone = _propBones.GetPropBone(propBoneSettings.PropType);
        if (bone == null || bone.Prop == null)
        {
            Debug.LogWarning($"WearableItemInstance: no PropBone found for type '{propBoneSettings.PropType}' (item '{instance.name}').");
            return;
        }

        propBoneSettings.SetPropBone(instance.transform, _propBones);
    }

    public virtual void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        for (var i = 0; i < Instances.Length; i++)
        {
            var instance = Instances[i];
            if (!instance)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(instance);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }

            Instances[i] = null;
        }

        _disposed = true;
    }
}