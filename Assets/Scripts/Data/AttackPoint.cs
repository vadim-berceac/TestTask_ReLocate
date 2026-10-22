using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackPoint
{
    [field: SerializeField] public float Radius { get; private set; }
    [field: SerializeField] public float Damage { get; private set; }
    [field: SerializeField] public Vector3 Offset { get; private set; }
    [field: SerializeField] public Transform AttackRoot { get; private set; }

    public readonly List<Vector3> PreviousPositions = new();
}