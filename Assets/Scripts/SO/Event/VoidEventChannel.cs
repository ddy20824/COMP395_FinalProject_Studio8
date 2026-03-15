using UnityEngine;
using System;

[CreateAssetMenu(fileName = "VoidEventChannel", menuName = "Events/Void Event Channel")]
public class VoidEventChannel : ScriptableObject
{
    private event Action eventRaised;

    private void OnEnable() => eventRaised = null;

    public void Subscribe(Action listener) => eventRaised += listener;
    public void Unsubscribe(Action listener) => eventRaised -= listener;

    public void Raise() => eventRaised?.Invoke();
}