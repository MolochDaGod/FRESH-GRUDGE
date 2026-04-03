using System;
using UnityEngine;
using UnityEngine.Events;
[Serializable] public class UnityEventBool : UnityEvent<bool> { }
//[Serializable] public class UnityEventInt : UnityEvent<int> { }
//[Serializable] public class UnityEventString : UnityEvent<string> { }
public class GameEventListener : MonoBehaviour
{
    public GameEvent gameEvent;
    public UnityEvent onEventTriggered;
    public UnityEventBool onTriggeredBool;
    public UnityEventInt onTriggeredInt;
    public UnityEventString onTriggeredString;

    void OnEnable()
    {
        if (gameEvent == null)
        {
            Debug.LogWarning($"[GameEventListener] gameEvent is not assigned on '{gameObject.name}'. Assign a GameEvent ScriptableObject in the Inspector.", this);
            return;
        }
        gameEvent.AddListener(this);
    }
    void OnDisable()
    {
        if (gameEvent == null) return;
        gameEvent.RemoveListener(this);
    }
    public void OnEventTriggered()
    {
        onEventTriggered.Invoke();
    }

    public void OnEventTriggeredBool(bool value)
    {
        onTriggeredBool.Invoke(value);
    }
    public void OnEventTriggeredInt(int value)
    {
        onTriggeredInt.Invoke(value);
    }
    public void OnEventTriggeredString(string value)
    {
        onTriggeredString.Invoke(value);
    }
}