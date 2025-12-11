using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[DefaultExecutionOrder(-20)]
public class ObjectRespawnManager : MonoBehaviour
{
    [ReadOnly] public static List<ResettableObject> resettableObjects = new List<ResettableObject>();

    public static ObjectRespawnManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }

    public static void RegisterResettable(ResettableObject obj)
    {
        if (!resettableObjects.Contains(obj))
            resettableObjects.Add(obj);  // Add the object to the list
    }

    public static void UnregisterResettable(ResettableObject obj)
    {
        if (resettableObjects.Contains(obj))
            resettableObjects.Remove(obj);  // Remove the object from the list when destroyed
    }

    [Button]
    public void DisableAllResettableObjects()
    {
        foreach (ResettableObject obj in resettableObjects)
        {
            obj.DisableObject();  // Disable each object
        }
    }

    [Button]
    public void ResetAllResettableObjects()
    {
        foreach (ResettableObject obj in resettableObjects)
        {
            obj.ResetObject();  // Reset (respawn) each object
        }
    }
}


