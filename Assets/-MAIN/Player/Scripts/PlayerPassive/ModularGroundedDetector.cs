using Sirenix.OdinInspector;
using UnityEngine;

public class ModularGroundedDetector : PlayerPassive
{
    [Header("Settings")]
    public LayerMask layerMask;
    public enum DetectionOrigin {Transform, Collider}
    public DetectionOrigin detectionOrigin;
    public bool nonModularMode; 

    [ShowIf(nameof(detectionOrigin), DetectionOrigin.Transform)] public Transform rayOriginObject;
    [ShowIf(nameof(detectionOrigin), DetectionOrigin.Transform)] public float rayLength;
    [ShowIf(nameof(detectionOrigin), DetectionOrigin.Transform)] public Vector3 offset;

    [ShowIf(nameof(detectionOrigin), DetectionOrigin.Collider)] public CapsuleCollider originCollider; 

    [Header("Data")]
    [ReadOnly] public bool isGrounded;
    private Vector3 rayOrigin;
    private float currentRayLength;

    [Header("References")]
    private PlayerRailGrind railGrind;
    private PlayerAbilityManager pA; 

    void Awake()
    {
        if (nonModularMode)
        {
            pA = PlayerAbilityManager.Instance; 
        }
    }

    private void Start()
    {
        if (nonModularMode)
        {
            railGrind = pA.GetAbility<PlayerRailGrind>(); 
        }
    }

    void Update()
    {
        if (detectionOrigin == DetectionOrigin.Transform)
        {
            rayOrigin = rayOriginObject.position + offset;
            currentRayLength = rayLength;
        }
        if (detectionOrigin == DetectionOrigin.Collider)
        {

            float colliderBottomY = originCollider.center.y - originCollider.height / 2;
            Vector3 colliderBottom = new Vector3(originCollider.center.x, colliderBottomY + 0.01f, originCollider.center.z);
            rayOrigin = originCollider.transform.TransformPoint(colliderBottom);
            currentRayLength = 0.01f;
            Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.cyan);
        }

        if (nonModularMode) //Used when the object is the player, as we want to ensure grounded state is always set to true when railgrinding even if ray misses due to object shape.
        {
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, currentRayLength, layerMask) || railGrind.isRailGrinding)
            {
                isGrounded = true;
                //Debug.Log(cM.playerRB.linearVelocity.magnitude); 
            }
            else
            {
                isGrounded = false;
            }
        }
        else
        {
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, currentRayLength, layerMask))
            {
                isGrounded = true;
                //Debug.Log(cM.playerRB.linearVelocity.magnitude); 
            }
            else
            {
                isGrounded = false;
            }
        }
    }

    public void OverwriteGroundedState(bool state)
    {
        isGrounded = state; 
    }

    public void EnableScript()
    {
        enabled = true; 
    }

    public void DisableScript()
    {
        enabled = false; 
    }

    public override void ResetPassive()
    {
        base.ResetPassive();     
    }
}
