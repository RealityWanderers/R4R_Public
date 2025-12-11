using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraSteer : PlayerPassive
{
    [Header("Settings")]
    [PropertyRange(0, 5)]
    public float cameraContribution = 0.995f; // 0 to 1 (blend factor)
    [PropertyRange(0, 1)]
    public float groundSteeringStrength = 1f;
    [PropertyRange(0, 5)]
    public float airSteeringStrength = 0.8f;

    [Header("Data")]
    private bool isGrounded;
    private Vector3 debugFinalVelocity;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerAbilityManager pA; 
    private PlayerPassivesManager pP; 
    [Header("Refs")]
    private Rigidbody playerRB;
    private Transform cameraTransform;
    private ModularSlopeDetector slopeDetector;
    private ModularGroundedDetector groundedDetector;
    private PlayerSoftSpeedCap playerSoftSpeedCap;
    private PlayerRailGrind railGrind; 

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pA = PlayerAbilityManager.Instance; 
        pP = PlayerPassivesManager.Instance; 
    }

    private void Start()
    {        
        playerRB = cM.playerRB;
        cameraTransform = cM.transform_MainCamera;
        railGrind = pA.GetAbility<PlayerRailGrind>(); 
        slopeDetector = pP.GetPassive<ModularSlopeDetector>();
        groundedDetector = pP.GetPassive<ModularGroundedDetector>();
        playerSoftSpeedCap = pP.GetPassive<PlayerSoftSpeedCap>(); 
    }

    void FixedUpdate()
    {
        isGrounded = groundedDetector.isGrounded;
        float steeringStrength = GetSteeringStrength();
        Vector3 cameraDirection = GetHorizontalCameraDirection();

        Vector3 adjustedVelocity = CalculateAdjustedVelocity(cameraDirection, steeringStrength);

        // Align velocity with the ground if grounded
        if (isGrounded)
        {
            adjustedVelocity = AdjustForGround(adjustedVelocity, slopeDetector.groundNormal);
        }

        // Apply the final velocity to the Rigidbody
        ApplyFinalVelocity(adjustedVelocity);

        DebugVisualSteering(adjustedVelocity, cameraDirection);
    }

    float GetSteeringStrength()
    {
        return isGrounded ? groundSteeringStrength : airSteeringStrength;
    }

    public Vector3 GetHorizontalCameraDirection()
    {
        Vector3 horizontalDirection = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        bool isRailGrinding = railGrind.isRailGrinding; 
        if (isGrounded && !isRailGrinding)
        {
            horizontalDirection = Vector3.ProjectOnPlane(horizontalDirection, slopeDetector.groundNormal).normalized;
        }
        return horizontalDirection;
    }

    Vector3 CalculateAdjustedVelocity(Vector3 cameraDirection, float steeringStrength)
    {
        //float currentSpeedPercentage = cM.playerSoftSpeedCap.speedPercentage;
        float currentHorizontalSpeed = playerSoftSpeedCap.GetHorizontalSpeed();
        if (currentHorizontalSpeed < 0.01f)
        {
            currentHorizontalSpeed = 0f;
        }


        if (currentHorizontalSpeed < 0.03f && isGrounded)
        {
            //Debug.Log("LowSpeed");
            //float horizontalSpeed = Vector3.ProjectOnPlane(playerRB.linearVelocity, Vector3.up).magnitude;
            //return cameraDirection * horizontalSpeed;

            //return BlendVelocity(playerRB.linearVelocity, cameraDirection, steeringStrength, cameraContribution);

            // At low speeds, snap to camera direction
            return cameraDirection * playerRB.linearVelocity.magnitude;
        }
        if (!isGrounded)
        {
            // Replace horizontal component but keep the vertical velocity unchanged
            Vector3 verticalVelocity = Vector3.up * playerRB.linearVelocity.y;
            Vector3 adjusted = cameraDirection * currentHorizontalSpeed + verticalVelocity;
            return BlendVelocity(playerRB.linearVelocity, adjusted.normalized, steeringStrength, cameraContribution);
        }
        else
        {
            // Blend velocity and camera direction
            return BlendVelocity(playerRB.linearVelocity, cameraDirection, steeringStrength, cameraContribution);
            // return BlendVelocity(playerRB.linearVelocity, cameraDirection, steeringStrength, cameraContribution);
        }
    }

    Vector3 BlendVelocity(Vector3 currentVelocity, Vector3 targetDirection, float strength, float contribution)
    {
        if (currentVelocity.sqrMagnitude < 0.01f)
            return Vector3.zero; // Ensure no momentum is added at very low speeds

        Vector3 targetVelocity = targetDirection * currentVelocity.magnitude; // Match speed
        return Vector3.Lerp(currentVelocity, targetVelocity, contribution * strength * Time.fixedDeltaTime);
    }

    //Vector3 AdjustForGround(Vector3 velocity, Vector3 groundNormal)
    //{
    //    return Vector3.ProjectOnPlane(velocity, groundNormal).normalized * velocity.magnitude;
    //}

    Vector3 AdjustForGround(Vector3 velocity, Vector3 groundNormal)
    {
        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(velocity, groundNormal); // Align horizontal movement to the slope
        return new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z); // Keep original vertical movement
    }

    void ApplyFinalVelocity(Vector3 finalVelocity)
    {
        if (isGrounded)
        {
            // Going downhill? Preserve gravity for smooth rolling
            if (slopeDetector.slopeType == ModularSlopeDetector.SlopeType.DownHill)
            {
                finalVelocity.y = playerRB.linearVelocity.y;
            }
            // Going uphill? Allow natural climbing momentum
            else if (slopeDetector.slopeType == ModularSlopeDetector.SlopeType.UpHill)
            {
                finalVelocity.y = Mathf.Max(playerRB.linearVelocity.y, 0); // Preserve upward motion
            }
            // Flat ground? Zero out vertical velocity
            else
            {
                //finalVelocity.y = 0;
            }
        }
        else
        {
            finalVelocity.y = playerRB.linearVelocity.y; // Preserve normal falling behavior in air
        }

        playerRB.linearVelocity = finalVelocity;
    }

    //void ApplyFinalVelocity(Vector3 finalVelocity)
    //{
    //    if (isGrounded)
    //    {
    //        finalVelocity.y = 0; // Keep grounded movement from interfering
    //    }
    //    else
    //    {
    //        finalVelocity.y = playerRB.linearVelocity.y; // Preserve gravity
    //    }

    //    playerRB.linearVelocity = finalVelocity;
    //}

    void DebugVisualSteering(Vector3 adjustedVelocity, Vector3 cameraDirection)
    {
        debugFinalVelocity = adjustedVelocity;
        Debug.DrawRay(transform.position + new Vector3(0, 0.1f, 0), cameraDirection * 3, Color.blue); // Camera direction
        Debug.DrawRay(transform.position, adjustedVelocity.normalized * 3, Color.green);              // Final velocity
    }

    public void TempDisable(float duration)
    {
        StartCoroutine(DoTempDisable(duration)); 
    }

    private IEnumerator DoTempDisable(float duration)
    {
        DisablePassive();
        yield return new WaitForSeconds(duration);   
        EnablePassive(); 
    }

    public override void ResetPassive()
    {
        base.ResetPassive();
    }
}

