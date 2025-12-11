using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerJump : PlayerAbility
{
    [Header("Data")]
    [ReadOnly] public bool isJumping;
    [ReadOnly] public bool canJump;
    [ReadOnly] public bool jumpStarted;
    [ReadOnly] public bool jumpActionBeingCanceled;

    [Header("Settings")]
    public float jumpForwardPower = 1;
    public float jumpMaxVerticalPower = 12;
    public float jumpDamping = 21f;
    public float jumpFreezeFrames = 0.03f;
    [Range(0, 1)] public float minJumpHeightPercentage = 0.8f;
    [ReadOnly] public float currentJumpVelocity;
    public float jumpSmoothCancelTime = 0.2f;
    public Ease jumpSmoothCancelEase = Ease.Linear;

    [Header("CyoteTime")]
    public float cyoteTimeDuration = 0.15f;
    [ReadOnly] public bool cyoteTimeActive;

    [Header("Input")]
    public float gripThreshold = 0.7f;
    public float requiredDistanceForJump = 0.4f;
    public float requiredControllerSpeed = 0.7f;
    public bool holdingBothGrip;
    [ReadOnly] public bool isListening;
    [ReadOnly] public float distanceTraveled_L;
    [ReadOnly] public float distanceTraveled_R;
    //[ReadOnly] public float totalDistance;
    private Vector3 startLocation_L;
    private Vector3 startLocation_R;
    private Vector3 playerStartPosition;

    [Header("Audio")]
    public AudioSource sfx_Source;
    public AudioClip sfx_Clip;
    [Range(0, 1)] public float sfx_Volume;
    public float sfx_maxPitch;
    public float sfx_pitchScaleTime;
    private Tween sfx_Tween;
    private float sfx_currentPitch;

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
    private PlayerAirTime airTime;
    private PlayerFreezeFrame freezeFrame;
    private ModularCustomGravity customGravity;
    private PlayerCameraSteer cameraSteer;
    private ModularGroundedDetector groundedDetector;
    private PlayerRailGrind railGrind;
    private Coroutine coroutine_JumpInit;
    private Tween tween_SmoothCancelJumpMomentum;

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pA = PlayerAbilityManager.Instance;
        pP = PlayerPassivesManager.Instance;
        pI = PlayerInputManager.Instance;
    }

    private void Start()
    {
        airTime = pP.GetPassive<PlayerAirTime>();
        groundedDetector = pP.GetPassive<ModularGroundedDetector>();
        freezeFrame = pP.GetPassive<PlayerFreezeFrame>();
        customGravity = pP.GetPassive<ModularCustomGravity>();
        cameraSteer = pP.GetPassive<PlayerCameraSteer>();
        railGrind = pA.GetAbility<PlayerRailGrind>();
    }

    void Update()
    {
        float gripValue_L = pI.gripValue_L;
        float gripValue_R = pI.gripValue_R;

        //Hold
        if (!jumpStarted)
        {
            if (!holdingBothGrip && gripValue_L > gripThreshold && gripValue_R > gripThreshold)
            {
                holdingBothGrip = true;

                if (!isListening)
                {
                    startLocation_L = cM.transform_Controller_L.transform.position;
                    startLocation_R = cM.transform_Controller_R.transform.position;
                    playerStartPosition = cM.playerRB.transform.position;
                    isListening = true;
                    //Debug.Log("IsListening");
                }
            }
        }

        if (holdingBothGrip && gripValue_L < gripThreshold || holdingBothGrip && gripValue_R < gripThreshold)
        {
            holdingBothGrip = false;
            if (isListening)
            {
                ResetListeningValues();
                //Debug.Log("IsNOTListening");
            }
        }

        if (isListening)
        {
            Vector3 playerMovement = cM.playerRB.transform.position - playerStartPosition;
            Vector3 controllerPosition_L = cM.transform_Controller_L.transform.position - playerMovement;
            Vector3 controllerPosition_R = cM.transform_Controller_R.transform.position - playerMovement;
            distanceTraveled_L = Vector3.Distance(startLocation_L, controllerPosition_L);
            distanceTraveled_R = Vector3.Distance(startLocation_R, controllerPosition_R);
            //totalDistance = distanceTraveled_L + distanceTraveled_R;
        }

        if (holdingBothGrip && canJump)
        {
            if (groundedDetector.isGrounded || cyoteTimeActive || railGrind.isRailGrinding)
            {
                if (distanceTraveled_L >= requiredDistanceForJump && distanceTraveled_R >= requiredDistanceForJump)
                {
                    if (pI.controllerVelocity_L.magnitude > requiredControllerSpeed && pI.controllerVelocity_R.magnitude > requiredControllerSpeed)
                    {
                        coroutine_JumpInit = StartCoroutine(JumpInit());
                        //Debug.Log("JUMP");
                    }
                }
            }
        }

        if (isJumping)
        {
            customGravity.DisableGravity(); //Ensure gravity is disabled as long as we are jumping.
            Vector3 currentPosition = cM.playerRB.position;
            currentPosition.y += currentJumpVelocity * Time.deltaTime;
            cM.playerRB.MovePosition(currentPosition);
            currentJumpVelocity -= jumpDamping * Time.deltaTime;
        }

        if (isJumping)
        {
            float jumpProgress = currentJumpVelocity / jumpMaxVerticalPower;
            if (currentJumpVelocity < 0) //Cancel from max jump reached
            {
                JumpComplete();
            }
            else if (!holdingBothGrip && jumpProgress < minJumpHeightPercentage) //Cancel from releasing grip
            {
                jumpActionBeingCanceled = true;
                if (tween_SmoothCancelJumpMomentum != null) { tween_SmoothCancelJumpMomentum.Kill(); }
                tween_SmoothCancelJumpMomentum = DOTween.To(() => currentJumpVelocity, x => currentJumpVelocity = x, 0, jumpSmoothCancelTime).SetEase(jumpSmoothCancelEase)
                .OnComplete(() => JumpComplete());
                if (sfx_Tween != null) { sfx_Tween.Kill(); }
            }
        }

        if (cyoteTimeActive && airTime.totalAirTime == 0)
        {
            cyoteTimeActive = false;
        }
        if (canJump && !cyoteTimeActive && !groundedDetector.isGrounded && airTime.totalAirTime < cyoteTimeDuration)
        {
            cyoteTimeActive = true;
        }

        if (!groundedDetector.isGrounded && !railGrind.isRailGrinding && airTime.totalAirTime > cyoteTimeDuration)
        {
            cyoteTimeActive = false;
        }

        if (!jumpStarted && !canJump && !cyoteTimeActive)
        {
            if (groundedDetector.isGrounded || railGrind.isRailGrinding)
            {
                canJump = true;
            }
        }

        UpdatePitch();
    }

    IEnumerator JumpInit()
    {
        canJump = false;
        jumpStarted = true;
        cyoteTimeActive = false;
        bool isRailGrinding = railGrind.isRailGrinding;
        if (isRailGrinding) { railGrind.ChangePlayerAttach(false); }

        ResetListeningValues();

        if (!isRailGrinding) //Skip freeze frame while railgrinding to prevent re attach during the freezeframe. PROPER FIX LATER. LIKE NOT ATTACHING WHILE FREEZEFRAMED.
        {
            freezeFrame.StartFreezeFrame(jumpFreezeFrames);
            yield return new WaitForSeconds(jumpFreezeFrames);
        }

        currentJumpVelocity = jumpMaxVerticalPower;

        UpdatePlayerDirection();
        pI.playerHaptic_L.SendHapticImpulse(hapticAmplitude, hapticDuration);
        pI.playerHaptic_R.SendHapticImpulse(hapticAmplitude, hapticDuration);

        Vector3 direction = cameraSteer.GetHorizontalCameraDirection();
        isJumping = true;

        cM.playerRB.AddForce(direction * (jumpForwardPower), ForceMode.VelocityChange);

        PlaySFX();
    }

    void PlaySFX()
    {
        sfx_Source.pitch = 1;
        if (sfx_Tween != null) { sfx_Tween.Kill(); sfx_currentPitch = 1; }
        sfx_Tween = DOTween.To(() => sfx_currentPitch, x => sfx_currentPitch = x, sfx_maxPitch, sfx_pitchScaleTime);
        sfx_Source.volume = sfx_Volume;
        sfx_Source.pitch = sfx_currentPitch;
        sfx_Source.clip = sfx_Clip;
        sfx_Source.Play();
    }

    void UpdatePitch()
    {
        sfx_Source.pitch = sfx_currentPitch;
    }

    void UpdatePlayerDirection()
    {
        cM.playerRB.linearVelocity = cameraSteer.GetHorizontalCameraDirection() * cM.playerRB.linearVelocity.magnitude;
    }

    void JumpComplete()
    {
        if (coroutine_JumpInit != null) { StopCoroutine(JumpInit()); }
        ResetListeningValues();
        isJumping = false;
        jumpStarted = false;
        jumpActionBeingCanceled = false;
        currentJumpVelocity = 0;
        customGravity.EnableGravity();
    }

    public void CancelJump()
    {
        JumpComplete();
    }

    void ResetListeningValues()
    {
        isListening = false;
        distanceTraveled_L = 0;
        distanceTraveled_R = 0;
        //totalDistance = 0;
    }

    public override void ResetAbility()
    {
        base.ResetAbility();

        JumpComplete();
        ResetListeningValues();
    }
}

