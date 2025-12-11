using Sirenix.OdinInspector;
using UnityEngine;

[DefaultExecutionOrder(-20)]
public class PlayerPassivesManager : MonoBehaviour
{
    [ReadOnly] public PlayerPassive[] passives;

    public static PlayerPassivesManager Instance { get; private set; }

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

        GetAllPassives();       
    }

    private void GetAllPassives()
    {
        PlayerObject playerObject = FindFirstObjectByType<PlayerObject>(); // Find the PlayerObject

        if (playerObject != null)
        {
            passives = playerObject.GetComponentsInChildren<PlayerPassive>(); // Get passives from player & children
        }
        else
        {
            //Debug.LogError("PlayerObject not found in the scene!");
        }
    }

    public T GetPassive<T>() where T : PlayerPassive
    {
        foreach (PlayerPassive passive in passives)
        {
            if (passive is T)
            {
                //Debug.Log("Found" + passive.name);
                return (T)passive;
            }
        }
        //Debug.Log("Can't Find");
        return null; // Default for reference types
    }

    public void ResetPassiveByType<T>() where T : PlayerPassive
    {
        foreach (PlayerPassive passive in passives)
        {
            if (passive is T)
            {
                passive.ResetPassive();
                return;
            }
        }
        //Debug.LogWarning("Passive not found of type: " + typeof(T).Name);
    }

    public void EnablePassiveByType<T>() where T : PlayerPassive
    {
        foreach (PlayerPassive passive in passives)
        {
            if (passive is T)
            {
                passive.EnablePassive();
                return;
            }
        }
    }

    public void DisablePassiveByType<T>() where T : PlayerPassive
    {
        foreach (PlayerPassive passive in passives)
        {
            if (passive is T)
            {
                passive.DisablePassive();
                return;
            }
        }
    }

    public void DisableAllPassives()
    {
        if (passives == null) return;
        foreach (PlayerPassive passive in passives)
        {
            passive.DisablePassive();
        }
    }

    public void EnableAllPassives()
    {
        if (passives == null) return;
        foreach (PlayerPassive passive in passives)
        {
            passive.EnablePassive();
        }
    }

    public void ResetAllPassives()
    {
        if (passives == null) return;
        foreach (PlayerPassive passive in passives)
        {
            passive.ResetPassive();
        }
    }
}
