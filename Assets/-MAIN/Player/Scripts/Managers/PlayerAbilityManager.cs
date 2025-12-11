using Sirenix.OdinInspector;
using UnityEngine;

[DefaultExecutionOrder(-20)]
public class PlayerAbilityManager : MonoBehaviour
{
    [ReadOnly] public PlayerAbility[] abilities;

    public static PlayerAbilityManager Instance { get; private set; }

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

        GetAllAbilities();
    }

    public void GetAllAbilities()
    {
        PlayerObject playerObject = FindFirstObjectByType<PlayerObject>(); // Find the PlayerObject

        if (playerObject != null)
        {
            abilities = playerObject.GetComponentsInChildren<PlayerAbility>(); // Get abilities from player & children
        }
        else
        {
            //Debug.LogError("PlayerObject not found in the scene!");
        }
    }

    public T GetAbility<T>() where T : PlayerAbility
    {
        foreach (PlayerAbility ability in abilities)
        {
            if (ability is T)
            {
                return (T)ability;
            }
        }
        return null; // Default for reference types
    }

    public void ResetAbilityByType<T>() where T : PlayerAbility
    {
        foreach (PlayerAbility ability in abilities)
        {
            if (ability is T)
            {
                ability.ResetAbility();
                return;
            }
        }
        //Debug.LogWarning("Ability not found of type: " + typeof(T).Name);
    }

    public void EnableAbilityByType<T>() where T : PlayerAbility
    {
        foreach (PlayerAbility ability in abilities)
        {
            if (ability is T)
            {
                ability.EnableAbility();
                return;
            }
        }
    }

    public void DisableAbilityByType<T>() where T : PlayerAbility
    {
        foreach (PlayerAbility ability in abilities)
        {
            if (ability is T)
            {
                ability.DisableAbility();
                return;
            }
        }
    }

    public void DisableAllAbilities()
    {
        if (abilities == null) return;
        foreach (PlayerAbility ability in abilities)
        {
            ability.DisableAbility();
        }
        //Debug.Log("Disable All Abilities");
    }

    public void EnableAllAbilities()
    {
        if (abilities == null) return;
        foreach (PlayerAbility ability in abilities)
        {
            ability.EnableAbility();
        }
        //Debug.Log("Enable All Abilities");
    }

    public void ResetAllAbilities()
    {
        if (abilities == null) return;
        foreach (PlayerAbility ability in abilities)
        {
            if (ability.enabled)
            {
                ability.ResetAbility();
            }
        }
    }
}
