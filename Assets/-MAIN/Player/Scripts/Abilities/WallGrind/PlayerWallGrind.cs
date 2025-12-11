using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class PlayerWallGrind : PlayerAbility
{
    [Header("Data")]
    [ReadOnly] public bool isWallGrinding;
    [ReadOnly] public bool isWallRight;
    [ReadOnly] public bool isWallLeft;
    private Vector3 wallNormal;

    [Header("Speed")]
    public float wallSpeedBoost = 1.7f;
    public float maxWallGrindSpeed = 10;
    public float minWallGrindSpeed = 6;
    public float dragWhileWallGrinding = 0.6f; 

    [Header("Attach")]
    public LayerMask wallLayer;
    public float wallDistanceCheckRadius = 1;
    public float sphereCheckRadius = 0.5f;

    [Header("WallJump")]
    public float requiredControllerSpeed = 2;
    public float forwardForce;
    public float upwardForce;
    public float wallNormalInfluence = 0.2f;

    [Header("Audio Hit")]
    public AudioSource sfx_SourceHit;
    public AudioClip sfx_ClipHit;
    [Range(0, 1)] public float sfx_VolumeHit = 0.4f;

    [Header("Audio Loop")]
    public AudioSource sfx_SourceLoop;
    public AudioClip sfx_ClipLoop;
    [Range(0, 1)] public float sfx_VolumeLoop = 0.4f;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerAbilityManager pA;
    private PlayerPassivesManager pP;
    private PlayerInputManager pI; 
    [Header("Refs")]
    private PlayerActionChainData actionChainData;
    private ModularCustomGravity customGravity;
    private PlayerCameraSteer cameraSteer;
    private PlayerQuickDash quickDash;
    private PlayerLockOnDash lockOnDash;
    private PlayerLockOnSystem lockOnSystem;
    private PlayerJump playerJump;
    private ModularCustomDrag drag; 
    private Vector3 currentWallNormal;


    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pA = PlayerAbilityManager.Instance;
        pP = PlayerPassivesManager.Instance;
        pI = PlayerInputManager.Instance; 
    }

    private void Start()
    {
        actionChainData = pA.GetAbility<PlayerActionChainData>();
        quickDash = pA.GetAbility<PlayerQuickDash>();
        lockOnDash = pA.GetAbility<PlayerLockOnDash>();
        playerJump = pA.GetAbility<PlayerJump>(); 
        lockOnSystem = pP.GetPassive<PlayerLockOnSystem>(); 
        customGravity = pP.GetPassive<ModularCustomGravity>();
        cameraSteer = pP.GetPassive<PlayerCameraSteer>();
        drag = pP.GetPassive<ModularCustomDrag>(); 
    }

    void Update()
    {
        if (isWallGrinding)
        {
            customGravity.DisableGravity();
            drag.ChangeDrag(dragWhileWallGrinding); 

            CheckForWallDetachment();
            actionChainData.RestActionChainTimeStamp(); 

            if (isWallLeft && pI.gripValue_L > 0.7f)
            {
                if (pI.controllerVelocity_L.magnitude > requiredControllerSpeed)
                {
                    WallJump();
                }
            }

            if (isWallRight && pI.gripValue_R > 0.7f)
            {
                if (pI.controllerVelocity_R.magnitude > requiredControllerSpeed)
                {
                    WallJump();
                }
            }
        }
    }

    public void StartWallGrind(Collision collision)
    {
        cameraSteer.DisablePassive();
        lockOnDash.ResetAbility();
        lockOnDash.DisableAbility();
        lockOnSystem.DisablePassive(); 
        quickDash.ResetAbility(); 
        quickDash.DisableAbility();
        playerJump.ResetAbility();
        customGravity.DisableGravity();
        isWallGrinding = true;

        currentWallNormal = collision.contacts[0].normal;

        Vector3 projectedVelocity = Vector3.ProjectOnPlane(cM.playerRB.linearVelocity, currentWallNormal);

        float currentSpeed = cM.playerRB.linearVelocity.magnitude * wallSpeedBoost;
        float clampedSpeed = Mathf.Clamp(currentSpeed, minWallGrindSpeed, maxWallGrindSpeed);

        projectedVelocity = projectedVelocity.normalized * clampedSpeed;
        projectedVelocity.y = 0;

        // Apply to rigidbody
        cM.playerRB.linearVelocity = projectedVelocity;


        actionChainData.AddToChain();
        quickDash.ResetCharges();
        PlaySFXHit();
        PlaySFXLoop(); 
    }

    void CheckForWallDetachment()
    {
        //Cast the ray in the opposite direction of the wall normal to check if we're still near the wall
        if (Physics.Raycast(cM.transform_PlayerFeet.position, -currentWallNormal, out RaycastHit hit, wallDistanceCheckRadius, wallLayer))
        {
            CheckWallSide(hit);
        }
        else
        {
            DetachFromWall();
        }
    }

    public void CheckWallSide(RaycastHit hit)
    {
        wallNormal = hit.normal;
        Vector3 movementRight = Vector3.Cross(Vector3.up, cM.playerRB.linearVelocity.normalized).normalized;
        float dotProduct = Vector3.Dot(movementRight, wallNormal);

        isWallLeft = dotProduct > 0;
        isWallRight = !isWallLeft;
    }

    public void WallJump()
    {
        DetachFromWall();
        UpdatePlayerDirection();

        Vector3 camDirection = cameraSteer.GetHorizontalCameraDirection(); // Camera direction, flattened

        Vector3 sidewaysDirection = Vector3.zero; 
        if (isWallRight)
        {
            sidewaysDirection = Vector3.Cross(Vector3.up, wallNormal).normalized;
        }
        else if (isWallLeft)
        {
            sidewaysDirection = Vector3.Cross(wallNormal, Vector3.up).normalized;
        }

        float lookAngle = Vector3.Angle(camDirection, sidewaysDirection);
        float blendFactor = Mathf.InverseLerp(0, 90, lookAngle);

        //Debug.Log("Angle" + lookAngle);
        //Debug.Log("Factor" + blendFactor);

        Vector3 finalJumpDirection = Vector3.Lerp(camDirection, wallNormal, blendFactor).normalized;
        finalJumpDirection = (finalJumpDirection + wallNormal * wallNormalInfluence).normalized;

        //Debug.DrawRay(cM.transform_PlayerFeet.position, finalJumpDirection * 2, Color.blue);
        //Debug.Log("Jumpofff"); 
        //Debug.Break();
        cM.playerRB.linearVelocity = finalJumpDirection * cM.playerRB.linearVelocity.magnitude; 
        cM.playerRB.AddForce(finalJumpDirection * forwardForce, ForceMode.VelocityChange);
        cM.playerRB.AddForce(Vector3.up * upwardForce, ForceMode.VelocityChange);
        PlaySFXHit();
    }

    void UpdatePlayerDirection()
    {
        cM.playerRB.linearVelocity = cameraSteer.GetHorizontalCameraDirection() * cM.playerRB.linearVelocity.magnitude;
    }

    public void DetachFromWall()
    {
        //Debug.Log("Detach"); 
        isWallGrinding = false;
        customGravity.EnableGravity(); 
        drag.ResetDragToDefault(); 
        cameraSteer.EnablePassive();
        lockOnDash.EnableAbility();
        lockOnSystem.EnablePassive();
        quickDash.EnableAbility();
        StopSFXLoop();
    }

    void PlaySFXHit()
    {
        sfx_SourceHit.clip = sfx_ClipHit;
        sfx_SourceHit.volume = sfx_VolumeHit;
        sfx_SourceHit.Play();
    }

    void PlaySFXLoop()
    {
        sfx_SourceLoop.clip = sfx_ClipLoop;
        sfx_SourceLoop.loop = true;
        sfx_SourceLoop.volume = sfx_VolumeLoop;
        sfx_SourceLoop.Play();
    }

    void StopSFXLoop()
    {
        sfx_SourceLoop.Stop();
    }

    public override void DisableAbility()
    {
        base.DisableAbility();
    }

    public override void EnableAbility()
    {
        base.EnableAbility();
    }

    public override void ResetAbility()
    {
        base.ResetAbility();

        DetachFromWall();
    }
}

