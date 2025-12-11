using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class PlayerRailGrindCornerBehaviour : PlayerAbility
{
    [Header("Settings-Detach")]
    public float detachAngleThreshold = 45f;

    [Header("Settings-Boost")]
    public float boostAmount;
    public float freezeFrames = 0.1f;
    public bool isFreezeFramed;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerAbilityManager pA;
    private PlayerPassivesManager pP;
    [Header("Refs")]
    private PlayerActionChainData actionChainData;
    private PlayerRailGrindCornerDetector railGrindCornerDetector;
    private PlayerCameraSteer cameraSteer;
    private PlayerRailGrind playerRailGrind;
    private PlayerParticleController playerParticleController;
    public HashSet<float> processedCorners = new HashSet<float>(); // Track processed corners
    public float previousT; // Track the previous T value

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance;
        pA = PlayerAbilityManager.Instance;
    }

    private void Start()
    {
        railGrindCornerDetector = pP.GetPassive<PlayerRailGrindCornerDetector>();
        cameraSteer = pP.GetPassive<PlayerCameraSteer>();
        playerParticleController = pP.GetPassive<PlayerParticleController>(); 
        playerRailGrind = pA.GetAbility<PlayerRailGrind>();
        actionChainData = pA.GetAbility<PlayerActionChainData>();
    }

    void Update()
    {
        if (playerRailGrind.isRailGrinding && !playerRailGrind.isFirstFrame)
        {
            CheckForCornerInteraction();
        }
        else if (processedCorners.Count != 0)
        {
            ClearProcessedCorners();
        }
    }

    void CheckForCornerInteraction()
    {
        if (railGrindCornerDetector.cornerTValues.Count == 0)
        {
            return;
        }

        float currentT = playerRailGrind.t;
        bool isMovingForward = playerRailGrind.isMovingForward;

        // Skip processing if the player hasn't moved significantly
        if (Mathf.Approximately(previousT, currentT)) return;

        bool isLoopingSpline = playerRailGrind.splineContainer.Spline.Closed;

        if (isLoopingSpline)
        {
            if (currentT == 0) // When wrapping from near 1 to 0
            {
                ProcessCorner(0.05f);
            }

            if (currentT == 1)
            {
                ProcessCorner(0.95f);
            }
        }

        foreach (float cornerT in railGrindCornerDetector.cornerTValues)
        {
            bool hasPassedCorner = false;

            if (isLoopingSpline)
            {
                // Special case: Handling wraparound movement (e.g., moving from 0.98  0.01)
                if (previousT > 0.95f && currentT < 0.05f && cornerT > 0.95f)
                {
                    hasPassedCorner = true;
                }
                else if (previousT < 0.05f && currentT > 0.95f && cornerT < 0.05f)
                {
                    hasPassedCorner = true;
                }
            }

            // New fix: Check if we **skipped over** the corner in one frame
            if (!hasPassedCorner)
            {
                float prevToCurrent = currentT - previousT;
                float prevToCorner = cornerT - previousT;
                float currentToCorner = cornerT - currentT;

                // Check if the sign of the movement direction changes relative to the corner
                hasPassedCorner = Mathf.Sign(prevToCurrent) == Mathf.Sign(prevToCorner) &&
                                  Mathf.Sign(prevToCurrent) != Mathf.Sign(currentToCorner);
            }

            if (hasPassedCorner)
            {
                ProcessCorner(cornerT);
                break; // Process one corner at a time
            }
        }

        previousT = currentT; // Update previous T for the next frame.
    }



    private Vector3 worldPosition_1;
    private Vector3 worldPosition_2;
    public Vector3 GetDetachDirection()
    {
        // Offset to sample points before detaching.
        float offsetAmount = 0.0125f; // Default offset for regular cases

        // Get the current T value
        float tValue = playerRailGrind.t;

        // Debug: Check the current T value
        //Debug.Log("Current T value: " + tValue);

        // Check if the rail loops
        bool isLoopingRail = playerRailGrind.splineContainer.Spline.Closed;

        // Special handling for cases where tValue is exactly 0 or 1
        float offsetT_1, offsetT_2;

        if (tValue == 0 || tValue < 0.03)
        {
            if (isLoopingRail) //If near end / start of rail
            {
                // If it's a looping rail, sample from the end of the rail
                offsetT_1 = playerRailGrind.isMovingForward ? 0.98f : 0.99f;
                offsetT_2 = playerRailGrind.isMovingForward ? 0.99f : 0.98f;
            }
            else
            {
                // If it's a non-looping rail, use small offsets near the start
                offsetT_1 = playerRailGrind.isMovingForward ? 0.01f : 0.02f;
                offsetT_2 = playerRailGrind.isMovingForward ? 0.02f : 0.01f;
            }
        }
        else if (tValue > 0.97) //If near end / start of rail
        {
            if (isLoopingRail)
            {
                // If it's a looping rail, sample from the start of the rail
                offsetT_1 = playerRailGrind.isMovingForward ? 0.01f : 0.02f;
                offsetT_2 = playerRailGrind.isMovingForward ? 0.02f : 0.01f;
            }
            else
            {
                // If it's a non-looping rail, sample normally near the end
                offsetT_1 = playerRailGrind.isMovingForward ? 0.98f : 0.99f;
                offsetT_2 = playerRailGrind.isMovingForward ? 0.99f : 0.98f;
            }
        }
        else  //If anywhere else on the rail 
        {
            // For all other T values, calculate normally
            offsetT_1 = playerRailGrind.isMovingForward ? tValue - offsetAmount : tValue + offsetAmount;
            offsetT_2 = playerRailGrind.isMovingForward ? tValue - (offsetAmount * 0.5f) : tValue + (offsetAmount * 0.5f);
        }

        // Debug: Log the offsets
        //Debug.Log("OffsetT_1: " + offsetT_1);
        //Debug.Log("OffsetT_2: " + offsetT_2);

        // Get the local positions at those T values.
        Vector3 position_1 = playerRailGrind.splineContainer.Spline.EvaluatePosition(offsetT_1);
        Vector3 position_2 = playerRailGrind.splineContainer.Spline.EvaluatePosition(offsetT_2);

        // Get the immediate parent of the rail container
        Transform parent = playerRailGrind.splineContainer.transform.parent;

        if (parent == null)
        {
            //Debug.Log("Rail container has no parent! Using local positions directly.");
            worldPosition_1 = position_1;
            worldPosition_2 = position_2;
        }
        else
        {
            // Apply the parent's position and rotation to the local positions
            worldPosition_1 = parent.position + parent.rotation * position_1;
            worldPosition_2 = parent.position + parent.rotation * position_2;
        }

        // Debug: Check what the world positions are
        // Debug.Log("Position 1 (World Space): " + worldPosition_1);
        // Debug.Log("Position 2 (World Space): " + worldPosition_2);

        // Calculate the direction between the two world positions.
        Vector3 preDetachDirection = (worldPosition_2 - worldPosition_1).normalized;


        // Debug log to check direction
        //Debug.Log("Pre-Detach Direction: " + preDetachDirection);

        return preDetachDirection;
    }

    private void OnDrawGizmos()
    {
        if (worldPosition_1 != Vector3.zero && worldPosition_2 != Vector3.zero)
        {
            // Draw spheres at the sampled positions, also add a small offset so it does not spawn in the rail.
            Gizmos.color = Color.green;
            Vector3 offsetPos_1 = new Vector3(worldPosition_1.x, worldPosition_1.y + 0.15f, worldPosition_1.z);
            Gizmos.DrawSphere(offsetPos_1, 0.05f);

            Gizmos.color = Color.red;
            Vector3 offsetPos_2 = new Vector3(worldPosition_2.x, worldPosition_2.y + 0.15f, worldPosition_2.z);
            Gizmos.DrawSphere(offsetPos_2, 0.05f);

            // Draw a line between the two points
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(offsetPos_1, offsetPos_2);
        }
    }



    private bool isOnCooldown = false; // Flag to prevent simultaneous detaching

    void ProcessCorner(float cornerT)
    {
        if (isOnCooldown) return; // Prevent further processing if detaching has already occurred.

        Vector3 cornerTangent = playerRailGrind.splineContainer.Spline.EvaluateTangent(cornerT);
        cornerTangent = playerRailGrind.splineContainer.transform.TransformDirection(cornerTangent); // Transform to world space

        cornerTangent.Normalize();
        Vector3 cameraDirection = cameraSteer.GetHorizontalCameraDirection();

        if (!playerRailGrind.isMovingForward)
        {
            cameraDirection = -cameraDirection;
        }

        // Calculate the angle between the player's direction and the corner tangent
        float angleToCorner = Vector3.Angle(cameraDirection, cornerTangent);
        //Debug.Log("Angle:" + angleToCorner);

        // If the angle to the corner exceeds the threshold, we detach the player.
        if (angleToCorner >= detachAngleThreshold)
        {
            // Detach the player from the rail
            isOnCooldown = true;
            StartCoroutine(ResetDetachFlag());
            playerRailGrind.ChangePlayerAttach(false);
            //Debug.Log("Detach");
        }
        else if (!isFreezeFramed)
        {
            // Boost the player if not freeze-framed
            isOnCooldown = true;
            StartCoroutine(ResetDetachFlag());
            processedCorners.Add(cornerT);
            StartCoroutine(CornerBoostSequence());
            //Debug.Log("Boost");
        }
    }

    private IEnumerator ResetDetachFlag()
    {
        // Optionally add some delay before allowing detaching again
        yield return new WaitForSeconds(0.1f + freezeFrames); // Add a small delay to prevent rapid detach
        isOnCooldown = false; // Allow detaching again after a short delay
    }



    public IEnumerator CornerBoostSequence()
    {
        StartRailGrindFreezeFrame(freezeFrames);
        yield return new WaitForSeconds(freezeFrames);
        actionChainData.AddToChain();
        RailGrindBoost();
    }

    public void StartRailGrindFreezeFrame(float freezeFrameDuration)
    {
        StartCoroutine(_StartRailGrindFreezeFrame(freezeFrameDuration));
    }

    public IEnumerator _StartRailGrindFreezeFrame(float freezeFrameDuration)
    {
        isFreezeFramed = true;
        float savedSpeed = playerRailGrind.currentMoveSpeed;
        playerRailGrind.currentMoveSpeed = 0;

        yield return new WaitForSeconds(freezeFrameDuration);

        isFreezeFramed = false;
        playerRailGrind.currentMoveSpeed = savedSpeed;
    }

    public void RailGrindBoost()
    {
        playerRailGrind.currentMoveSpeed += boostAmount;
        playerParticleController.PlayBoostRing(); 
    }

    public void ClearProcessedCorners()
    {
        //if (processedCorners.Count != 0) processedCorners.Clear();
        //Debug.Log("Clear");
    }

    public override void ResetAbility()
    {
        base.ResetAbility();

        ClearProcessedCorners();
        isFreezeFramed = false;
    }
}
