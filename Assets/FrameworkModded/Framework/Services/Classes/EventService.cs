using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EventService : MonoBehaviour, IFrameworkService
{
    private Dictionary<string, List<Action<object>>> _eventListeners = new();
    private Dictionary<Action, Action<object>> _wrappedListener = new();

    public void Setup()
    {
        
    }

    public void Connect(string eventName, Action<object> callback)
    {
        if (!_eventListeners.ContainsKey(eventName))
        {
            _eventListeners[eventName] = new List<Action<object>>();
        }

        _eventListeners[eventName].Add(callback);
    }

    public void Connect(string eventName, Action callback)
    {
        Action<object> wrappedCallback = (object _) => callback();
        _wrappedListener[callback] = wrappedCallback;
        Connect(eventName, wrappedCallback);
    }

    public void Disconnect(string eventName, Action<object> callback)
    {
        if (_eventListeners.TryGetValue(eventName, out List<Action<object>> eventListeners))
        {
            eventListeners.Remove(callback);

            if (eventListeners.Count == 0)
            {
                _eventListeners.Remove(eventName);
            }
        }
        else
        {
            // Event doesn't exist
        }
    }

    public void Disconnect(string eventName, Action callback)
    {
        if (_wrappedListener.TryGetValue(callback, out Action<object> wrappedCallback))
        {
            Disconnect(eventName, wrappedCallback);
            _wrappedListener.Remove(callback);
        }
    }

    public void Fire(string eventName, object argument = default)
    {
        if (_eventListeners.TryGetValue(eventName, out List<Action<object>> eventListeners))
        {
            foreach (Action<object> listener in eventListeners)
            {
                try
                {
                    listener(argument);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"Exception in event '{eventName}': {exception}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"Event '{eventName}' was fired but is not registered.");
        }
    }  

    public bool ReadValue<T>(object data, out T value)
    {
        value = default;
        try
        {
            T cast = (T) data;
            value = cast;
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Invalid data: {exception}");
        }
        return false;
    }
}
