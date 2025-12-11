using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ModularSlopeDetector))]
public class PlayerSlopeSpeed : PlayerPassive
{
    [Header("Settings")]
    public float downHillBonusForce;
    public float upHillSlowMulti = 0.95f;
    public float uphillDampFactor = 0.1f;
    public float steepRampAngleThreshold = 60;
    public float gentlePushForce = 5;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerPassivesManager pP; 
    [Header("Refs")]
    private ModularSlopeDetector slopeDetector;
    private Rigidbody rb;

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance; 
    }

    private void Start()
    {
        rb = cM.playerRB;
        slopeDetector = pP.GetPassive<ModularSlopeDetector>();
    }

    void FixedUpdate()
    {
        //Vector3 forceDirection = transform.forward; //PLACEHOLDER FOR CAMERA BASED STEERING
        //Vector3 forceDirection = cM.playerCameraSteer.halfWayVector;
        //Vector3 forceDirection = Vector3.ProjectOnPlane(Vector3.down, slopeDetector.hitNormal).normalized;

        //if (slopeDetector.slopeType == SlopeDetector.SlopeType.Idle)
        //{

        //}
        //else if (slopeDetector.slopeType == SlopeDetector.SlopeType.UpHill)
        //{
        //    float currentUpHillMulti = Mathf.Lerp(1, upHillSlowMulti, slopeDetector.slopeAngle / 90);
        //    Debug.Log(currentUpHillMulti); 
        //    //rb.linearVelocity = rb.linearVelocity.normalized * (rb.linearVelocity.magnitude * currentUpHillMulti);
        //    rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, rb.linearVelocity.normalized * (rb.linearVelocity.magnitude * currentUpHillMulti), uphillDampFactor);
        //}
        //else if (slopeDetector.slopeType == SlopeDetector.SlopeType.DownHill)
        //{
        //    rb.AddForce(forceDirection * (downHillBonusForce * slopeDetector.slopeAngle));
        //}
    }

    public override void ResetPassive()
    {
        base.ResetPassive();
    }
}
