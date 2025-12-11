using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerQuickStep : PlayerAbility
{
    [Header("Data")]
    [ReadOnly] public bool quickStepStarted;
    [ReadOnly] public bool quickStepCompleted;
    [ReadOnly] private bool isQuickStepping = false;
    private float currentQuickStepSpeed = 0;
    private float quickStepTimeStamp;
    private Vector3 quickStepDirection;
    private Vector3 quickStepStartPos;
    private float savedPlayerSpeed;

    [Header("Settings-Force")]
    public float quickStepForwardForce = 1;
    public float quickStepAngle = 40;
    public float quickStepDuration = 0.15f;
    public float quickStepSpeed = 3.4f;
    [Range(0,1)] public float quickStepRampTime; 
    public Ease quickStepSpeedRampEaseType;
    public float quickStepFreezeFrames = 0.06f;
    [Range(0, 1)] public float quickStepStiffness = 0.7f; 

    [Header("Settings-RiskBoost")]
    public float riskBoostForce = 2;
    public float riskBoostDelay = 0.3f; 
    public float riskBoostRayLength = 0.5f;
    public bool canRiskBoost;
    public LayerMask riskBoostLayerMask;

    [Header("Cooldown")]
    public float cooldownTime = 0.5f;
    private float cooldownTimeStamp;
    [ReadOnly] public bool isOnCooldown;

    [Header("Controller")]
    public float TriggerButtonThreshold = 0.6f;
    public bool holding_Trigger_L;
    public bool holding_Trigger_R;

    [Header("Audio")]
    public AudioSource sfx_Source;
    public AudioClip sfx_Clip;
    [Range(0, 1)] public float sfx_Volume = 0.4f;

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
    [Header("Refs")]
    private Coroutine coroutine_QuickDashInit;
    private Tweener tween_DashSpeed;
    private PlayerOverclockBoost overclockBoost;
    private PlayerActionChainData actionChainData;
    private ModularGroundedDetector groundedDetector;
    private PlayerLockOnSystem lockOnSystem;
    private PlayerCameraSteer cameraSteer;
    private PlayerFreezeFrame freezeFrame; 

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pA = PlayerAbilityManager.Instance;
        pP = PlayerPassivesManager.Instance;
        pI = PlayerInputManager.Instance; 
    }

    private void Start()
    {
        overclockBoost = pA.GetAbility<PlayerOverclockBoost>();
        actionChainData = pA.GetAbility<PlayerActionChainData>();
        groundedDetector = pP.GetPassive<ModularGroundedDetector>();
        lockOnSystem = pP.GetPassive<PlayerLockOnSystem>();
        cameraSteer = pP.GetPassive<PlayerCameraSteer>();
        freezeFrame = pP.GetPassive<PlayerFreezeFrame>();         
    }

    void Update()
    {
        float triggerValue_L = pI.triggerValue_L;
        float triggerValue_R = pI.triggerValue_R;

        //if (!holding_Trigger_L && triggerValue_L > TriggerButtonThreshold)
        //{
        //    holding_Trigger_L = true;
        //}
        //else
        //{
        //    holding_Trigger_L = false;
        //}

        //if (!holding_Trigger_R && triggerValue_R > TriggerButtonThreshold)
        //{
        //    holding_Trigger_R = true;
        //}
        //else
        //{
        //    holding_Trigger_R = false;
        //}

        //Overwrite to false if both triggers are held.
        if (triggerValue_L > TriggerButtonThreshold && triggerValue_R > TriggerButtonThreshold)
        {
            holding_Trigger_L = false;
            holding_Trigger_R = false;
        }

        if (!quickStepStarted && !lockOnSystem.isLockedOn && groundedDetector.isGrounded && !isOnCooldown)
        {
            if (holding_Trigger_L || holding_Trigger_R)
            {
                coroutine_QuickDashInit = StartCoroutine(QuickStepInit());
            }
        }

        if (Time.time > cooldownTimeStamp + cooldownTime + quickStepDuration + quickStepFreezeFrames)
        {
            isOnCooldown = false;
        }
    }

    IEnumerator QuickStepInit()
    {
        pA.ResetAbilityByType<PlayerJump>();
        pP.DisablePassiveByType<PlayerCameraSteer>(); 

        float totalQuickStepDuration = quickStepDuration + quickStepFreezeFrames;
        quickStepStarted = true;
        cooldownTimeStamp = Time.time;
        isOnCooldown = true;

        savedPlayerSpeed = cM.playerRB.linearVelocity.magnitude;

        pI.playerHaptic_L.SendHapticImpulse(hapticAmplitude, hapticDuration);
        pI.playerHaptic_R.SendHapticImpulse(hapticAmplitude, hapticDuration);

        //Risk Boost check
        Vector3 projectedVelocityDirection = Vector3.ProjectOnPlane(cM.playerRB.linearVelocity.normalized, Vector3.up);
        Vector3 rayOrigin = cM.playerCollider.bodyCollider.transform.TransformPoint(cM.playerCollider.bodyCollider.center);
        float bottom = rayOrigin.y - (cM.playerCollider.bodyCollider.height / 4);
        rayOrigin = new Vector3(rayOrigin.x, bottom, rayOrigin.z);
        float rayLength = cM.playerCollider.bodyCollider.radius + riskBoostRayLength;
        if (Physics.Raycast(rayOrigin, projectedVelocityDirection, out RaycastHit hit, rayLength, riskBoostLayerMask))
        {
            if (hit.transform.GetComponent<PlayerRiskBoostObject>() != null)
            {
                canRiskBoost = true;
                actionChainData.AddToChain();
            }
            else
            {
                canRiskBoost = false; 
            }
        }

        if (holding_Trigger_L)
        {
            Vector3 leftBoostDirection = Quaternion.Euler(0, -quickStepAngle, 0) * cameraSteer.GetHorizontalCameraDirection();
            quickStepDirection = leftBoostDirection;
        }
        if (holding_Trigger_R)
        {
            Vector3 rightBoostDirection = Quaternion.Euler(0, quickStepAngle, 0) * cameraSteer.GetHorizontalCameraDirection();
            quickStepDirection = rightBoostDirection;
        }

        freezeFrame.StartFreezeFrame(quickStepFreezeFrames * (canRiskBoost ? 2 : 1));
        yield return new WaitForSeconds(quickStepFreezeFrames * (canRiskBoost ? 2 : 1));

        StartDash();

        yield return new WaitForSeconds(quickStepDuration);
    }

    void UpdatePlayerVelocityDirection()
    {
        cM.playerRB.linearVelocity = cameraSteer.GetHorizontalCameraDirection() * cM.playerRB.linearVelocity.magnitude;
    }

    void FixedUpdate()
    {
        if (isQuickStepping)
        {
            Vector3 dashForce = quickStepDirection * (currentQuickStepSpeed);
            cM.playerRB.AddForce(dashForce, ForceMode.VelocityChange);
            if (Time.time > quickStepTimeStamp + quickStepDuration)
            {
                StartCoroutine(StopDash());
            }
        }
    }

    void StartDash()
    {
        currentQuickStepSpeed = 0f;
        tween_DashSpeed = DOTween.To(() => currentQuickStepSpeed, x => currentQuickStepSpeed = x, quickStepSpeed, quickStepDuration * quickStepRampTime).SetEase(quickStepSpeedRampEaseType);
        quickStepTimeStamp = Time.time;
        isQuickStepping = true;
        PlaySFX(); 
    }

    IEnumerator StopDash()
    {
        pP.EnablePassiveByType<PlayerCameraSteer>();
        ResetValues();
        if (coroutine_QuickDashInit != null) { StopCoroutine(coroutine_QuickDashInit); }
        tween_DashSpeed.Kill();

        freezeFrame.StartFreezeFrame(0.01f); 
        yield return new WaitForSeconds(0.01f);
        cM.playerRB.linearVelocity = Vector3.zero;
        //cM.playerRB.linearVelocity = cM.playerCameraSteer.GetHorizontalCameraDirection() * 0;
        //Debug.Log(cM.playerRB.linearVelocity); 

        Vector3 directionToSet = Vector3.Lerp(quickStepDirection, cameraSteer.GetHorizontalCameraDirection(), quickStepStiffness); 
        cM.playerRB.linearVelocity = directionToSet * savedPlayerSpeed;
        //Debug.Log(cM.playerRB.linearVelocity);
        if (canRiskBoost)
        {
            yield return new WaitForSeconds(riskBoostDelay);
            cM.playerRB.AddForce(cM.playerRB.linearVelocity.normalized * riskBoostForce, ForceMode.VelocityChange);
            canRiskBoost = false;
        }
        else
        {
            cM.playerRB.AddForce(cM.playerRB.linearVelocity.normalized * quickStepForwardForce, ForceMode.VelocityChange);
        }
    }

    void PlaySFX()
    {
        sfx_Source.clip = sfx_Clip;
        sfx_Source.volume = sfx_Volume;
        sfx_Source.Play();
    }

    public void ResetValues()
    {
        isQuickStepping = false;
        quickStepStarted = false;
    }

    public override void ResetAbility()
    {
        base.ResetAbility();

        ResetValues();
        tween_DashSpeed.Kill(); 
    }
}