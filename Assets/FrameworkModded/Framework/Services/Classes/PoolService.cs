using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoolService : MonoBehaviour, IFrameworkService
{
    private Dictionary<(System.Type, string), (Component Prefab, Stack<Component> Stack, int Capacity)> pools = new();
    /*
        Data Structure
        Dictionary
            Key: Tuple(System.Type, string)
            Value: Tuple(Stack<Component> Stack, int Capacity)

        How to access
            Dictionary[Key].Prefab (do not directly edit)
            Dictionary[Key].Stack (contains the objects)
            Dictionary[Key].Capacity
    */

    public void Setup() { }

    public void CreatePool<T>(T prefab, string variant = "", int capacity = 32) where T : Component
    {
        System.Type type = typeof(T);
        (System.Type, string) key = (type, variant);

        if (!pools.ContainsKey(key))
        {
            // Create a copy of the prefab given
            T prefabClone = Instantiate(prefab.gameObject).GetComponent<T>();
            prefabClone.gameObject.SetActive(false);
            prefabClone.transform.SetParent(this.transform);
            prefabClone.name = $"{key.Item1.ToString().Replace("Component", "")}(source)";;

            pools[key] = (prefabClone, new Stack<Component>(), capacity);
            Stack<Component> stack = pools[key].Stack;
            Component stackPrefab = pools[key].Prefab;

            for (int i = 0; i < capacity; i++)
            {
                T clone = Instantiate(stackPrefab.gameObject).GetComponent<T>();
                clone.name = $"clone({i})";
                clone.gameObject.SetActive(false);
                clone.transform.SetParent(this.transform);

                stack.Push(clone);
            }
        }
    }

    public void CreatePool<T>(string variant = "", int capacity = 32) where T : Component
    {
        GameObject obj = new GameObject("prefab");
        obj.AddComponent<T>();

        CreatePool<T>(obj.GetComponent<T>(), variant, capacity);
    }

    public void CreatePool<T>(int capacity = 32) where T : Component
    {
        CreatePool<T>("", capacity);
    }

    public T Get<T>(string variant = "") where T : Component
    {
        System.Type type = typeof(T);
        (System.Type, string) key = (type, variant);

        if (pools.TryGetValue(key, out var pool))
        {
            T poolObject;

            if (pool.Stack.Count > 0)
            {
                poolObject = (T) pool.Stack.Pop();
                poolObject.gameObject.name = $"{key.Item1.ToString().Replace("Component", "")}({pool.Capacity - pool.Stack.Count - 1})";
            }
            else
            {
                // The pool is empty, create a copy of the prefab
                poolObject = Instantiate(pool.Prefab.gameObject).GetComponent<T>();
                poolObject.gameObject.name = $"{key.Item1.ToString().Replace("Component", "")}(+)";
            }
            return poolObject;
        }

        return null;
    }

    private void Release(Component Component)
    {
        Component.gameObject.SetActive(false);
        Component.transform.SetParent(this.transform);
    }

    private static bool HaveSameComponents(GameObject gameObject1, GameObject gameObject2)
    {
        Component[] components1 = gameObject1.GetComponents<Component>();
        Component[] components2 = gameObject2.GetComponents<Component>();

        if (components1.Length != components2.Length)
        {
            return false;
        }

        // Get component types for comparison
        var types1 = components1.Select(c => c.GetType());
        var types2 = components2.Select(c => c.GetType());

        // Check if types are equal
        return types1.SequenceEqual(types2);
    }

    public void Release<T>(T releasedPrefab, string variant = "") where T : Component
    {
        System.Type type = typeof(T);
        (System.Type, string) key = (type, variant);

        if (pools.TryGetValue(key, out var pool))
        {
            // Validate released prefab
            if (!HaveSameComponents(releasedPrefab.gameObject, pool.Prefab.gameObject))
            {
                Debug.LogWarning("Cannot be released to pool due to mismatched components");
                Destroy(releasedPrefab.gameObject);
            }

            if (pool.Stack.Count < pool.Capacity)
                {
                    Release((Component)releasedPrefab);
                    pool.Stack.Push(releasedPrefab);
                }
                else
                {
                    Destroy(releasedPrefab.gameObject);
                }
        }
        else
        {
            Debug.LogWarning($"A pool does not exist for this [Component: {key.Item1} | Variant: {(key.Item2.Equals("") ? "NULL" : key.Item2)}]");
            Destroy(releasedPrefab.gameObject);
        }
    }
}
