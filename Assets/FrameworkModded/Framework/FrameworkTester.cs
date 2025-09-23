using static Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

public class FrameworkTester : MonoBehaviour
{
    EventService EventService;
    PoolService PoolService;

    void Awake()
    {
        EventService = Game.GetService<EventService>();
        PoolService = Game.GetService<PoolService>();
    }

    void Start()
    {
        /*
        EventService.Connect("Event1", OnTestEvent1);
        EventService.Connect("Event2", OnTestEvent2);
        EventService.Fire("Event1");
        EventService.Fire("Event2", 123);
        EventService.Fire("Event2", "Hello world");
        EventService.Disconnect("Event1", OnTestEvent1);
        EventService.Disconnect("Event2", OnTestEvent2);
        EventService.Fire("Event1");
        EventService.Fire("Event2");
        */

        PoolService.CreatePool<SceneChangerComponent>(3);

        var test = PoolService.Get<SceneChangerComponent>();
        test.transform.SetParent(this.transform);
        test.gameObject.SetActive(true);
        test.gameObject.AddComponent<SceneChangerComponent>();

        PoolService.Release(test);

        /*
        for (int i = 0; i < 6; i++)
        {
            var test = PoolService.Get<SceneChangerComponent>();
            test.transform.SetParent(this.transform);
            test.gameObject.SetActive(true);
        }
        */
    }

    void OnTestEvent1()
    {
        Debug.Log("Event is great");
    }

    void OnTestEvent2(object data)
    {
        if (EventService.ReadValue(data, out string message))
        {
            Debug.Log($"Event2 is greater: {message}");
        }  
        else
        {
            Debug.Log("Failed");
        }
    }
}
