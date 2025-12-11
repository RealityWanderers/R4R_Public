using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallBounce : PlayerPassive
{
    [Header("Force")]
    public float backwardForce;
    public float upwardForce;
    [PropertyRange(0, 1)]
    public float afterBounceMomentumKeepMulti = 0.5f;
    [PropertyRange(0, 1)] public float minSpeedPercentageForBounce = 0.3f;

    [Header("Lockout")]
    public float bounceBackLockout = 0.2f;
    [ReadOnly] public bool onCoolDown;

    [Header("Steer Disable")]
    public float steerDisableDuration = 1.2f;

    [Header("Collision Check")]
    [Range(0,1)] public float skewTowardsCamera = 0.7f; 
    public float collisionAngleThreshold = 45;
    public LayerMask collisionLayerMask;
    [ReadOnly] public bool isColliding;
    [ReadOnly] public float collisionAngle;

    [Header("SFX")]
    public AudioClip sfx_Clip;
    [Range(0, 1)] public float sfx_Volume = 0.4f;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerPassivesManager pP; 
    private PlayerAbilityManager pA;
    private PlayerSFX sfx; 
    [Header("Refs")]
    private PlayerCameraSteer cameraSteer;
    private PlayerSoftSpeedCap softSpeedCap;
    private PlayerQuickDash playerQuickDash; 

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pA = PlayerAbilityManager.Instance;
        pP = PlayerPassivesManager.Instance;
        sfx = PlayerSFX.Instance;
    }

    private void Start()
    {        
        cameraSteer = pP.GetPassive<PlayerCameraSteer>();
        softSpeedCap = pP.GetPassive<PlayerSoftSpeedCap>();
        playerQuickDash = pA.GetAbility<PlayerQuickDash>(); 
    }

    void Update()
    {
        if (!onCoolDown)
        {
            CheckForWallBounce();
        }
    }

    public void CheckForWallBounce()
    {
        if (isColliding == false)
        {
            collisionAngle = 0;
        }

        isColliding = false;

        float speedPercentage = softSpeedCap.GetSpeedPercentage();

        Vector3 projectedVelocityDirection = Vector3.ProjectOnPlane(cM.playerRB.linearVelocity.normalized, Vector3.up);
        projectedVelocityDirection = Vector3.Lerp(projectedVelocityDirection, cameraSteer.GetHorizontalCameraDirection(), skewTowardsCamera); 
        if (projectedVelocityDirection.sqrMagnitude < 0.01f) return;
        //Debug.Log("CorrectVelocity");

        Vector3 rayOrigin = cM.playerCollider.bodyCollider.transform.TransformPoint(cM.playerCollider.bodyCollider.center);
        float bottom = rayOrigin.y - (cM.playerCollider.bodyCollider.height / 4); 
        rayOrigin = new Vector3(rayOrigin.x, bottom, rayOrigin.z);
        float rayLength = cM.playerCollider.bodyCollider.radius + 0.1f + cM.playerRB.linearVelocity.magnitude * 0.03f; //Longer ray at higher speed to not miss collisions.

        //Debug.DrawRay(rayOrigin, projectedVelocityDirection * rayLength, Color.magenta);

        if (Physics.Raycast(rayOrigin, projectedVelocityDirection, out RaycastHit hit, rayLength, collisionLayerMask))
        {
            //Debug.Log($"Hit: {hit.collider.gameObject.name}");
            collisionAngle = Vector3.Angle(projectedVelocityDirection, -hit.normal);
            //Debug.Log(collisionAngle);

            if (speedPercentage > minSpeedPercentageForBounce && collisionAngle < collisionAngleThreshold)
            {
                //Debug.Log("BounceInnit");
                BounceInit();
            }
            isColliding = true;
        }
    }

    public void BounceInit()
    {
        //Debug.Log("Bounce");
        StartCoroutine(BounceLockout(bounceBackLockout));
        StartCoroutine(DoBounce());



        //sfx_Bonk.pitch = Random.Range(0.8f, 1.2f);
        //sfx_Bonk.Play();
    }

    public IEnumerator DoBounce()
    {
        sfx.PlaySFX(sfx_Clip, sfx_Volume); 
        pA.ResetAbilityByType<PlayerJump>();
        pA.ResetAbilityByType<PlayerQuickDash>();
        pA.ResetAbilityByType<PlayerLockOnDash>();
        pA.ResetAbilityByType<PlayerOverclockBoost>(); 

        pA.DisableAbilityByType<PlayerQuickDash>();
        pP.DisablePassiveByType<PlayerCameraSteer>();

        float knockBackPower = backwardForce * (softSpeedCap.GetSpeedPercentage() + 0.45f);
        cM.playerRB.linearVelocity = Vector3.zero;
        Vector3 knockbackDirection = -cameraSteer.GetHorizontalCameraDirection() * knockBackPower + Vector3.up * upwardForce;
        cM.playerRB.AddForce(knockbackDirection, ForceMode.VelocityChange);
        //Debug.Log(cM.playerRB.linearVelocity.magnitude);
        yield return new WaitForSeconds(0.2f);
        cM.playerRB.linearVelocity *= afterBounceMomentumKeepMulti;

        yield return new WaitForSeconds(steerDisableDuration);
        pA.EnableAbilityByType<PlayerQuickDash>();
        pP.EnablePassiveByType<PlayerCameraSteer>();
    }

    public IEnumerator BounceLockout(float lockoutTime)
    {
        onCoolDown = true;
        yield return new WaitForSeconds(lockoutTime);
        onCoolDown = false;
    }

    public override void ResetPassive()
    {
        base.ResetPassive();

        onCoolDown = false;
    }
}
