using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerRespawn))]
public class PlayerTeleporter : MonoBehaviour
{
    [Header("Settings")]
    public bool teleportingEnabled = false;
    public bool teleportOnStart;
    [ReadOnly] public List<Transform> teleportLocations = new();

    [Header("Data")]
    [ReadOnly] public int locationInt;
    private Vector3 startPosition;
    private Vector3 startRotation;

    [Header("Refs")]
    private Rigidbody playerRB;
    private PlayerComponentManager cM;
    private PlayerInputManager pI;

    public static PlayerTeleporter Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        cM = PlayerComponentManager.Instance;
        pI = PlayerInputManager.Instance;
    }

    public void InitTeleportLocations()
    {
        teleportLocations.Clear();
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                teleportLocations.Add(child);
            }       
        }
    }

    public void Start()
    {
        playerRB = cM.playerRB;
        InitTeleportLocations();
        startPosition = teleportLocations[locationInt].position;
        startRotation = teleportLocations[locationInt].eulerAngles;

        if (teleportOnStart)
        {
            TeleportFromList(0);
        }
    }

    void Update()
    {
        //bool location_Previous = pI.playerInput.Left.StickClick.WasPressedThisFrame();
        bool location_Next = pI.playerInput.Right.StickClick.WasPressedThisFrame();

        bool location_Previous_Keyboard = pI.playerInput.Debug.DebugSpawnPrevious.WasPressedThisFrame();
        bool location_Next_Keyboard = pI.playerInput.Debug.DebugSpawnNext.WasPressedThisFrame();

        if (teleportingEnabled)
        {
            if (location_Next || location_Next_Keyboard)
            {
                if (locationInt + 1 < teleportLocations.Count)
                {
                    locationInt++;
                }

                TeleportFromList(locationInt);
            }
            if (/*location_Previous || */location_Previous_Keyboard)
            {
                if (locationInt - 1 >= 0)
                {
                    locationInt--;
                }

                TeleportFromList(locationInt);
            }
        }
    }

    [Button]
    public void TeleportFromList(int locationInt)
    {
        startPosition = teleportLocations[locationInt].position;
        startRotation = teleportLocations[locationInt].eulerAngles;
        cM.playerRespawn.ResetPlayer();
    }

    public void TeleportToTransform(Transform location)
    {
        startPosition = location.position;
        startRotation = location.eulerAngles;
        cM.playerRespawn.ResetPlayer();
    }

    public void ResetPlayerPosition() //Called by the respawn script when it finishes resetting the player.
    {
        playerRB.position = startPosition;
        playerRB.rotation = Quaternion.Euler(startRotation);
        cM.transform_MainCamera.rotation = Quaternion.Euler(startRotation);
    }
}
