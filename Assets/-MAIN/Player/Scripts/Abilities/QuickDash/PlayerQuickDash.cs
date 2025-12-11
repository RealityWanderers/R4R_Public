using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class PlayerQuickDash : PlayerAbility
{
    [Header("Data")]
    [ReadOnly] public bool canTriggerDash; 
    [ReadOnly] public bool quickDashStarted;
    [ReadOnly] public bool quickDashCompleted;
    [ReadOnly] private bool isDashing = false;
    private Vector3 quickDashDirection;

    [Header("Settings-Force")]
    public float downwardQuickDashAngleThreshold = -5; 
    public float quickDashDuration = 0.17f;
    public float quickDashSpeed = 120;
    public float quickDashUpwardsForce = 200f;
    [Range(0, 1)] public float quickDashEndSlowDown = 0.55f;
    public float quickDashFreezeFrames = 0.12f;

    [Header("Cooldown")]
    [ReadOnly] public int currentCharges = 1;
    public int maxCharges = 1;

    [Header("Controller")]
    public float InputThreshold = 0.6f;
    public float requiredControllerSpeed = 2f;
    [ReadOnly] public bool holdingBothInputs;
    private bool releaseFrame;

    [Header("Audio")]
    public AudioSource sfx_Source;
    public AudioClip sfx_Clip;
    [Range(0, 1)] public float sfx_Volume; 

    [Header("Haptics")]
    [PropertyRange(0, 1)]
    public float hapticAmplitude = 0.45f;
    [PropertyRange(0, 1)]
    public float hapticDuration = 0.2f;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerAbilityManager pA;
    private PlayerPassivesManager pP;
    private PlayerInputManager pI;
    private PlayerSFX pSFX;
    [Header("Refs")]
    private PlayerJump playerJump;
    private PlayerLockOnSystem playerLockOnSystem;
    private Coroutine coroutine_QuickDashInit;
    private float quickDashElapsedTime;
    private ModularGroundedDetector groundedDetector;
    private ModularCustomGravity customGravity;
    private PlayerCameraSteer cameraSteer;
    private PlayerFreezeFrame freezeFrame;
    private PlayerLockOnDash lockOnDash;
    private PlayerParticleController playerParticleController;

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pA = PlayerAbilityManager.Instance;
        pP = PlayerPassivesManager.Instance;
        pI = PlayerInputManager.Instance;
        pSFX = PlayerSFX.Instance;
    }

    private void Start()
    {
        playerJump = pA.GetAbility<PlayerJump>();
        playerLockOnSystem = pP.GetPassive<PlayerLockOnSystem>();
        groundedDetector = pP.GetPassive<ModularGroundedDetector>();
        customGravity = pP.GetPassive<ModularCustomGravity>();
        cameraSteer = pP.GetPassive<PlayerCameraSteer>();
        freezeFrame = pP.GetPassive<PlayerFreezeFrame>();
        playerParticleController = pP.GetPassive<PlayerParticleController>();
        lockOnDash = pA.GetAbility<PlayerLockOnDash>();
    }

    void Update()
    {
        float gripValue_L = pI.gripValue_L;
        float gripValue_R = pI.gripValue_R;

        float speed_L = pI.controllerVelocity_L.magnitude;
        float speed_R = pI.controllerVelocity_R.magnitude;

        bool isAirborne = !groundedDetector.isGrounded;
        bool lockedOn = playerLockOnSystem.isLockedOn;
        bool hasCharges = currentCharges >= 1;

        // If just became airborne, require a new grip press
        if (isAirborne && !holdingBothInputs && gripValue_L < InputThreshold && gripValue_R < InputThreshold)
        {
            if (!playerJump.cyoteTimeActive)
            {
                if (playerJump.jumpActionBeingCanceled || !playerJump.isJumping)
                {
                    holdingBothInputs = false; // Reset state to force a fresh press
                    canTriggerDash = true; // Allow dash after a fresh grip press
                }
            }
        }

        if (!isAirborne || playerJump.cyoteTimeActive)
        {
            canTriggerDash = false;
            holdingBothInputs = false;
        }

        // Register a fresh grip press when airborne
        if (isAirborne && canTriggerDash && gripValue_L > InputThreshold && gripValue_R > InputThreshold)
        {
            holdingBothInputs = true;
            canTriggerDash = false; // Prevent immediate re-trigger
        }

        // Trigger Quick Dash if conditions are met
        if (holdingBothInputs && isAirborne && hasCharges && !quickDashStarted && gripValue_L > InputThreshold && gripValue_R > InputThreshold) //Check again for grips here to ensure we are actually holding the input currently.
        {
            if (speed_L > requiredControllerSpeed && speed_R > requiredControllerSpeed)
            {
                if (lockedOn)
                {
                    lockOnDash.StartLockOnDash();
                }
                else
                {
                    coroutine_QuickDashInit = StartCoroutine(QuickDashInit());
                }
                holdingBothInputs = false; // Prevent re-trigger
            }
        }

        // Reset charges upon landing
        if (groundedDetector.isGrounded && currentCharges != maxCharges)
        {
            ResetCharges();
        }
    }

    public void OnDisable() //We disable the ability and thus update upon railgrinding, so reset this value manually as otherwise it stays true.
    {
        canTriggerDash = false; 
    }

    IEnumerator QuickDashInit()
    {
        pA.ResetAbilityByType<PlayerJump>();

        customGravity.EnableGravity();
        quickDashStarted = true;

        currentCharges -= 1;
        currentCharges = Mathf.Clamp(currentCharges, 0, maxCharges);

        freezeFrame.StartFreezeFrame(quickDashFreezeFrames);
        yield return new WaitForSeconds(quickDashFreezeFrames);

        pI.playerHaptic_L.SendHapticImpulse(hapticAmplitude, hapticDuration);
        pI.playerHaptic_R.SendHapticImpulse(hapticAmplitude, hapticDuration);
        StartDash();
        PlaySFX();
        playerParticleController.PlayBoostRing(); 
    }

    void UpdatePlayerVelocityDirection()
    {
        cM.playerRB.linearVelocity = cameraSteer.GetHorizontalCameraDirection() * cM.playerRB.linearVelocity.magnitude;
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            customGravity.DisableGravity();

            Vector3 dashForce = quickDashDirection * quickDashSpeed * Time.fixedDeltaTime;
            cM.playerRB.AddForce(dashForce, ForceMode.VelocityChange);

            quickDashElapsedTime += Time.fixedDeltaTime;

            if (quickDashElapsedTime > quickDashDuration || groundedDetector.isGrounded)
            {
                Vector3 stopDashForce = -cM.playerRB.linearVelocity * quickDashEndSlowDown;
                cM.playerRB.AddForce(stopDashForce, ForceMode.VelocityChange);
                StopDash();
            }
        }
    }

    void StartDash()
    {
        quickDashElapsedTime = 0f;
        UpdatePlayerVelocityDirection();
        isDashing = true;

        //float cameraXAngle = cM.transform_MainCamera.eulerAngles.x;
        //if (cameraXAngle > 180f)
        //{
        //    cameraXAngle -= 360f;
        //}
        //if (cameraXAngle >= downwardQuickDashAngleThreshold)
        //{
        //    quickDashDirection = cM.transform_MainCamera.forward;     
        //}
        //else if (cameraXAngle < downwardQuickDashAngleThreshold)
        //{
        //    quickDashDirection = cameraSteer.GetHorizontalCameraDirection();
        //    cM.playerRB.AddForce(Vector3.up * quickDashUpwardsForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
        //}

        quickDashDirection = cameraSteer.GetHorizontalCameraDirection();
        cM.playerRB.AddForce(Vector3.up * quickDashUpwardsForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }

    void StopDash()
    {
        if (coroutine_QuickDashInit != null) { StopCoroutine(coroutine_QuickDashInit); }
        customGravity.EnableGravity();

        isDashing = false;
        quickDashStarted = false;
    }

    void PlaySFX()
    {
        sfx_Source.clip = sfx_Clip;
        sfx_Source.volume = sfx_Volume; 
        sfx_Source.Play(); 
    }

    public void ResetCharges()
    {
        currentCharges = maxCharges;
    }

    public override void ResetAbility()
    {
        base.ResetAbility();

        StopDash();
    }
}