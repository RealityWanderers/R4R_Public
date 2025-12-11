using Sirenix.OdinInspector;
using UnityEngine;

[DefaultExecutionOrder(-20)]
public class PlayerUIManager : MonoBehaviour
{
    [ReadOnly] public PlayerUI[] playerUI;

    public static PlayerUIManager Instance { get; private set; }

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

    private void Start()
    {
        PlayerUIObject playerUIObject = FindFirstObjectByType<PlayerUIObject>(); // Find the PlayerObject

        if (playerUIObject != null)
        {
            playerUI = playerUIObject.GetComponentsInChildren<PlayerUI>(); // Get abilities from player & children
        }
        else
        {
           //Debug.LogError("UI Object not found in the scene!");
        }
    }

    public void DisableAllUI()
    {
        if (playerUI == null) return;
        foreach (PlayerUI UI in playerUI)
        {
            UI.DisableUI();
        }
    }

    public void EnableAllUI()
    {
        if (playerUI == null) return;
        foreach (PlayerUI UI in playerUI)
        {
            UI.EnableUI();
        }
    }

    public void ResetAllUI()
    {
        if (playerUI == null) return;
        foreach (PlayerUI UI in playerUI)
        {
            UI.ResetUI();
        }
    }
}
