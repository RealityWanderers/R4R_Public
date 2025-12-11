using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrabbableLockOnSystem : PlayerPassive
{
    [Header("Aiming")]
    public float dashLockOnDistance = 9;
    public float lockOnAngle = 30f;
    public bool isLockedOn;
    public bool requireGrounded;

    [Header("Lock On")]
    public LayerMask lockOnMask;
    public LayerMask obstructionMask;
    [ReadOnly] public Transform object_CurrentLockOnTarget;
    [ReadOnly] public float object_CurrentRadius;
    [ReadOnly] public Vector3 endLocation;
    private Transform closestTarget;

    [Header("Audio")]
    public AudioSource sfx_Source;
    public AudioClip sfx_Clip;
    [Range(0, 1)] public float sfx_Volume = 0.4f;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerPassivesManager pP;
    [Header("Refs")]
    private ModularGroundedDetector groundedDetector;
    private PlayerGrabbableSystem playerGrabbableSystem;
    public PlayerReticleController reticleController;
    [ReadOnly] public PlayerGrabbable grabbable;

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance;
    }

    private void Start()
    {
        groundedDetector = pP.GetPassive<ModularGroundedDetector>();
        playerGrabbableSystem = pP.GetPassive<PlayerGrabbableSystem>();
    }

    void Update()
    {
        if (requireGrounded)
        {
            if (!groundedDetector.isGrounded)
            {
                TryFindTarget();
            }
            if (isLockedOn && groundedDetector.isGrounded)
            {
                ResetLockOn();
            }
        }
        else
        {
            if (!playerGrabbableSystem.CheckForActiveGrab())
            {
                TryFindTarget();
            }
        }

        CheckIfNull();

        if (isLockedOn && object_CurrentLockOnTarget != null)
        {
            endLocation = object_CurrentLockOnTarget.position; 
            reticleController.UpdatePosition(endLocation);
        }
    }

    // Predefine an array large enough to hold expected targets
    private Collider[] potentialTargets = new Collider[10];

    void TryFindTarget()
    {
        Vector3 cameraPos = cM.transform_MainCamera.position;
        Vector3 cameraForward = cM.transform_MainCamera.forward;

        //Stick to current target if it's still valid
        if (object_CurrentLockOnTarget != null)
        {
            Vector3 dirToTarget = (object_CurrentLockOnTarget.position - cameraPos).normalized;
            float angle = Vector3.Angle(cameraForward, dirToTarget);
            float dist = Vector3.Distance(cameraPos, object_CurrentLockOnTarget.position);

            if (angle < lockOnAngle && dist < dashLockOnDistance &&
                !Physics.Raycast(cameraPos, dirToTarget, dist, obstructionMask))
            {
                return; // Still a valid target, no need to search again
            }
        }

        // Now we perform the overlap check if no valid current target
        int numTargets = Physics.OverlapSphereNonAlloc(cameraPos, dashLockOnDistance, potentialTargets, lockOnMask, QueryTriggerInteraction.Collide);

        closestTarget = null;
        float closestAngle = Mathf.Infinity;
        float closestDistance = Mathf.Infinity;

        for (int i = 0; i < numTargets; i++)
        {
            Collider collider = potentialTargets[i];

            if (collider.TryGetComponent(out PlayerGrabbableLockOnObject lockOnScript))
            {
                //Debug.Log("LockOnFound");
                Vector3 directionToTarget = (collider.transform.position - cameraPos).normalized;
                float angleToTarget = Vector3.Angle(cameraForward, directionToTarget);

                if (angleToTarget < lockOnAngle)
                {
                    float distanceToTarget = Vector3.Distance(cameraPos, collider.transform.position);

                    if (!Physics.Raycast(cameraPos, directionToTarget, out RaycastHit hit, distanceToTarget, obstructionMask))
                    {
                        if (angleToTarget < closestAngle || (Mathf.Approximately(angleToTarget, closestAngle) && distanceToTarget < closestDistance))
                        {
                            closestAngle = angleToTarget;
                            closestDistance = distanceToTarget;
                            closestTarget = collider.transform;

                            if (closestTarget != object_CurrentLockOnTarget)
                            {
                                //Debug.Log("ClosestLockOnFound");
                                object_CurrentLockOnTarget = closestTarget;
                                object_CurrentRadius = lockOnScript.reticleRadiusOffset;
                                endLocation = closestTarget.position;
                                reticleController.Appear(endLocation, object_CurrentRadius);
                                grabbable = object_CurrentLockOnTarget.GetComponent<PlayerGrabbable>();
                                isLockedOn = true;
                                PlaySFX();
                            }
                        }
                    }
                }
            }
        }
    }

    void CheckIfNull()
    {
        if (closestTarget == null)
        {
            if (object_CurrentLockOnTarget != null)
            {
                ResetLockOn();
                //Debug.Log("LockOn Null");
            }
        }
    }

    void ResetLockOn()
    {
        if (object_CurrentLockOnTarget != null)
        {
            object_CurrentLockOnTarget = null;
        }
        grabbable = null;
        reticleController.Hide();
        isLockedOn = false;
    }

    void PlaySFX()
    {
        sfx_Source.clip = sfx_Clip;
        sfx_Source.volume = sfx_Volume;
        sfx_Source.Play();
    }

    public override void ResetPassive()
    {
        base.ResetPassive();

        ResetLockOn();
    }
}

