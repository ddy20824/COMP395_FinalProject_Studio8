using UnityEngine;
using System;

public abstract class BaseEventChannel<T> : ScriptableObject
{
    private Action<T> eventRaised;
    public void Raise(T value) => eventRaised?.Invoke(value);
    public void Subscribe(Action<T> listener) => eventRaised += listener;
    public void Unsubscribe(Action<T> listener) => eventRaised -= listener;
}