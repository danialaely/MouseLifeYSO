// Cage.cs (attached to the cage prefab)
using UnityEngine;
using System;

public class Cage : MonoBehaviour
{
    public static event Action<Transform> OnCageSpawned;

    private void Start()
    {
        // Notify that the cage has spawned
        OnCageSpawned?.Invoke(this.transform);
    }
}
