using UnityEngine;
using System;
using System.Collections.Generic;

public sealed class Framework : MonoBehaviour
{
    private Framework() {}
    private static Framework _gameSource = null;
    private static readonly object _threadlock = new object();
    private Dictionary<Type, IFrameworkService> _services = new();

    public static Framework Game
    {
        get
        {
            lock (_threadlock) 
            {
                if (_gameSource == null)
                {
                   GameObject frameworkObject = new GameObject("Framework");
                   _gameSource = frameworkObject.AddComponent<Framework>();
                }

                return _gameSource;
            }  
        }
    }

    void Awake()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(this);
    }

    public T GetService<T>() where T : MonoBehaviour, IFrameworkService
    {
        Type serviceType = typeof(T);
        if (_services.TryGetValue(serviceType, out IFrameworkService service))
        {
            return (T) service;
        }
        
        // The Sevice hasn't been registered yet.
        return RegisterService<T>();
    }

    public T RegisterService<T>() where T : MonoBehaviour, IFrameworkService
    {
        Type serviceType = typeof(T);

        if (_services.ContainsKey(serviceType))
        {
            // Service is already registered
            return (T) _services[serviceType];
        }

        // Create the service
        GameObject serviceObject = new GameObject(serviceType.Name);
        serviceObject.transform.SetParent(this.transform);
        T newService = serviceObject.AddComponent<T>();
        newService.Setup();
        _services[serviceType] = newService;
        return newService;
    }

    public static Framework Instance => Game;
}
