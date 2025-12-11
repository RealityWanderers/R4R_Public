using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

[DefaultExecutionOrder(-20)]
public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance { get; private set; }
    [HideInInspector] public XRIDefaultInputActions playerInput;

    [FoldoutGroup("Position")] [ReadOnly] public Vector3 controllerPos_L;
    [FoldoutGroup("Position")] [ReadOnly] public Vector3 controllerPos_R;
    [Space]
    [FoldoutGroup("Grip")] [ReadOnly] public float gripValue_L;
    [FoldoutGroup("Grip")] [ReadOnly] public float gripValue_R;
    [Space]
    [FoldoutGroup("Trigger")] [ReadOnly] public float triggerValue_L;
    [FoldoutGroup("Trigger")] [ReadOnly] public float triggerValue_R;
    [Space]
    [FoldoutGroup("Velocity")] [ReadOnly] public Vector3 controllerVelocity_L;
    [FoldoutGroup("Velocity")] [ReadOnly] public Vector3 controllerVelocity_R;
    [Space]
    [FoldoutGroup("Axis")] [ReadOnly] public Vector2 stickAxis_L;
    [FoldoutGroup("Axis")] [ReadOnly] public float stickAxis_X_L;
    [FoldoutGroup("Axis")] [ReadOnly] public float stickAxis_Y_L;
    [Space]
    [FoldoutGroup("Axis")] [ReadOnly] public Vector2 stickAxis_R;
    [FoldoutGroup("Axis")] [ReadOnly] public float stickAxis_X_R;
    [FoldoutGroup("Axis")] [ReadOnly] public float stickAxis_Y_R;
    [Space]
    [FoldoutGroup("Haptic")] public HapticImpulsePlayer playerHaptic_L;
    [FoldoutGroup("Haptic")] public HapticImpulsePlayer playerHaptic_R;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }

        playerInput = new XRIDefaultInputActions();
        playerInput.Enable(); // Enables all input actions at once
    }

    void Update()
    {
        controllerPos_L = playerInput.Left.Position.ReadValue<Vector3>();
        controllerPos_R = playerInput.Right.Position.ReadValue<Vector3>();

        gripValue_L = playerInput.Left.GripValue.ReadValue<float>();
        gripValue_R = playerInput.Right.GripValue.ReadValue<float>();

        triggerValue_L = playerInput.Left.TriggerValue.ReadValue<float>();
        triggerValue_R = playerInput.Right.TriggerValue.ReadValue<float>();

        stickAxis_L = playerInput.Left.Stick.ReadValue<Vector2>();
        stickAxis_X_L = stickAxis_L.x;
        stickAxis_Y_L = stickAxis_L.y;

        stickAxis_R = playerInput.Right.Stick.ReadValue<Vector2>();
        stickAxis_X_R = stickAxis_R.x;
        stickAxis_Y_R = stickAxis_R.y;

        controllerVelocity_L = playerInput.Left.Velocity.ReadValue<Vector3>();
        controllerVelocity_R = playerInput.Right.Velocity.ReadValue<Vector3>();
    }
}

