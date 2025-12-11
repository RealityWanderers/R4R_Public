using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

[DefaultExecutionOrder(-20)]
public class PlayerComponentManager : MonoBehaviour
{
    [FoldoutGroup("PlayerTransforms")] public Transform transform_XRRig;
    [FoldoutGroup("PlayerTransforms")] public Transform transform_MainCamera;
    [FoldoutGroup("PlayerTransforms")] public Transform transform_MainCameraOffset;
    [FoldoutGroup("PlayerTransforms")] public Transform transform_Controller_L;
    [FoldoutGroup("PlayerTransforms")] public Transform transform_Controller_R;
    [FoldoutGroup("PlayerTransforms")] public Transform transform_PlayerFeet;
    [FoldoutGroup("PlayerTransforms")] public Transform transform_PlayerBodyCollision;

    //[FoldoutGroup("PlayerIK")] public Transform transform_PlayerIK;
    //[FoldoutGroup("PlayerIK")] public PlayerIKScale playerIKScale;

    [FoldoutGroup("Collision")] public PlayerCollider playerCollider;
    [FoldoutGroup("Collision")] public PlayerHandCollision playerHandCollision_L;
    [FoldoutGroup("Collision")] public PlayerHandCollision playerHandCollision_R;

    [FoldoutGroup("PlayerRB")] public Rigidbody playerRB;

    [FoldoutGroup("Reticle")] public PlayerReticleController playerReticleController;
    [FoldoutGroup("System")] public PlayerTeleporter playerTeleporter;
    [FoldoutGroup("System")] public PlayerRespawn playerRespawn;
    [FoldoutGroup("System")] public PlayerRecenter playerRecenter;
    [FoldoutGroup("System")] public PlayerCenterIndicator playerCenterIndicator;
    [FoldoutGroup("System")] public SetRefreshRate setRefreshRate;
    //[FoldoutGroup("System")] public PlayerAbilityManager pM;

    [FoldoutGroup("UI")] public PlayerUIOverclock playerUIOverclock;
    [FoldoutGroup("UI")] public PlayerUIActionChain playerUIActionChain;

    public static PlayerComponentManager Instance { get; private set; }

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
}
