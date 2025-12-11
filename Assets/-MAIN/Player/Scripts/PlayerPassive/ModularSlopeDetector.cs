using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ModularGroundedDetector))]
public class ModularSlopeDetector : PlayerPassive
{
    [Header("Settings")]
    public float rayLength;
    public Transform rayOrigin;
    public LayerMask layerMask;
    [Header("Slopes")]
    public SlopeType slopeType;
    [ReadOnly] public enum SlopeType { Idle, UpHill, DownHill }
    public float minimumSlopeAngle = 25;
    [Header("Data")]
    [ReadOnly] public float slopeAngle;
    [ReadOnly] public Vector3 groundNormal;
    [Header("Refs")]
    private Rigidbody rb;
    private ModularGroundedDetector groundedDetector;

    public void Start()
    {
        groundedDetector = GetComponent<ModularGroundedDetector>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Physics.Raycast(rayOrigin.position, Vector3.down, out RaycastHit hit, rayLength, layerMask))
        {
            slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            groundNormal = hit.normal;
        }

        Vector3 projectedDown = Vector3.ProjectOnPlane(Vector3.down, groundNormal);
        Vector3 forwardAlongPlane = Vector3.ProjectOnPlane(transform.forward, groundNormal);
        forwardAlongPlane.Normalize();

        if (slopeAngle > minimumSlopeAngle) //Only activate when we have a reasonable slope.
        {
            //Debug.Log("Big enough angle"); 

            if (groundedDetector.isGrounded)
            {
                if (Vector3.Angle(projectedDown.normalized, rb.linearVelocity.normalized) <= 90 /*&& Vector3.Angle(projectedDown.normalized, rb.linearVelocity.normalized) >= 5*/)
                {
                    slopeType = SlopeType.DownHill;
                    //Debug.Log("Down");
                }
                if (Vector3.Angle(projectedDown.normalized, rb.linearVelocity.normalized) >= 90)
                {
                    slopeType = SlopeType.UpHill;
                    //Debug.Log("Up");
                }
            }
        }
        else if (slopeAngle <= minimumSlopeAngle)
        {
            //Debug.Log("Idle");
            if (groundedDetector.isGrounded)
            {
                slopeType = SlopeType.Idle;
            }
        }
    }

    public override void ResetPassive()
    {
        base.ResetPassive();
    }
}
