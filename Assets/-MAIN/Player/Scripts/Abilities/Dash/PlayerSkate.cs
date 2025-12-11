using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkate : PlayerAbility
{
    [Header("DashSpeedCap")]
    [ReadOnly] public float currentDashSoftSpeedCapThreshold = 0;
    public float defaultDashSoftSpeedCapThreshold = 2;
    [ReadOnly] public float currentAboveSpeedCapMulti = 0;

    [Header("DuringDash")]
    public float duringDashMotionPower = 12f;

    [Header("DragChange")]
    public float dragDuringRunning = 0.3f;
    public float dragDelay = 0.5f;
    private float lastDashTimeStamp;

    [Header("SprintMode")]
    public float speedCapDuringSprint = 4.5f;
    public float swingMultiDuringSprint = 0.65f;
    public bool sprintActive;
    public float drainAmount = 0.05f; 

    [Header("Speed Tiers")]
    [ReadOnly] public float currentTier1Multi = 0;
    public float defaultTier1Multi = 0.5f; 
    public float tier1RequiredPercentage = 0;
    //public float tier2Multi = 0.5f;
    //public float tier2RequiredPercentage = 0.8f;
    [ReadOnly] public int speedTier;
    [ReadOnly] public float currentSpeedTierMulti;

    [Header("Power")]
    public float maxReleasePower = 16;
    [Space]
    [ReadOnly] public float currentPower_L;
    [ReadOnly] public float powerPercentage_L;
    [ReadOnly] public float currentPower_R;
    [ReadOnly] public float powerPercentage_R;
    [Space]
    [ReadOnly] public float currentTotalPower;

    [Header("Input")]
    public float minRequiredControllerSpeed = 1;
    [ReadOnly] public bool canDash_L; //Used to ensure player has to release the grip before starting a new dash.
    [ReadOnly] public bool canDash_R; //Used to ensure player has to release the grip before starting a new dash.
    [ReadOnly] public bool gripHeld_L;
    [ReadOnly] public bool gripHeld_R;
    [ReadOnly] private float controllerSpeed_L;
    [ReadOnly] private float controllerSpeed_R;

    [Header("SFX")]
    public AudioSource sfx_Dash;
    public float dashSFX_Volume = 0.1f;

    [Header("Haptics")]
    [PropertyRange(0, 1)]
    public float extraBaseHaptic;
    [PropertyRange(0, 1)]
    public float hapticDuration;

    [Header("HapticsRelease")]
    [PropertyRange(0, 1)]
    public float releaseHapticAmplitude;
    [PropertyRange(0, 1)]
    public float releaseHapticDuration;
    [PropertyRange(0, 5)]
    public int releaseHapticRepeatAmount;
    [PropertyRange(0, 1)]
    public float releaseHapticRepeatDelay;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerAbilityManager pM;
    private PlayerPassivesManager pP;
    private PlayerInputManager pI;
    [Header("Refs")]
    private PlayerOverclockBoost overclockBoost;
    private PlayerOverclockData overclockData; 
    private ModularGroundedDetector groundedDetector;
    private PlayerSoftSpeedCap softSpeedCap;
    private PlayerCameraSteer cameraSteer;
    private PlayerSkateSway skateSway;
    private PlayerParticleController speedLineController;
    private ModularCustomDrag drag; 

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pM = PlayerAbilityManager.Instance;
        pP = PlayerPassivesManager.Instance;
        pI = PlayerInputManager.Instance;
    }

    private void Start()
    {
        groundedDetector = pP.GetPassive<ModularGroundedDetector>();
        softSpeedCap = pP.GetPassive<PlayerSoftSpeedCap>();
        cameraSteer = pP.GetPassive<PlayerCameraSteer>();
        skateSway = pP.GetPassive<PlayerSkateSway>();
        speedLineController = pP.GetPassive<PlayerParticleController>();
        overclockBoost = pM.GetAbility<PlayerOverclockBoost>();
        drag = pP.GetPassive<ModularCustomDrag>();
        overclockData = pP.GetPassive<PlayerOverclockData>();

        currentDashSoftSpeedCapThreshold = defaultDashSoftSpeedCapThreshold;
        currentTier1Multi = defaultTier1Multi; 
    }

    void Update()
    {
        bool dashInput_L = pI.playerInput.Left.GripButton.IsPressed();
        bool dashInput_R = pI.playerInput.Right.GripButton.IsPressed();

        if (dashInput_L && gripHeld_L == false)
        {
            gripHeld_L = true;
            canDash_L = true;
            DashStartSFX();
        }
        else if (!dashInput_L && gripHeld_L == true)
        {
            gripHeld_L = false;
            DashStopSFX();
        }

        if (dashInput_R && gripHeld_R == false)
        {
            gripHeld_R = true;
            canDash_R = true;
            DashStartSFX();
        }
        else if (!dashInput_R && gripHeld_R == true)
        {
            gripHeld_R = false;
            DashStopSFX();
        }

        if (gripHeld_L && gripHeld_R)
        {
            ResetDashValues();
        }

        if (groundedDetector.isGrounded)
        {
            //Dash on grip release or max power reached.
            if (gripHeld_L == false && currentPower_L > 0 || gripHeld_R == false && currentPower_R > 0
                || currentPower_L >= maxReleasePower || currentPower_R >= maxReleasePower)
            {
                if (currentPower_L > maxReleasePower)
                {
                    currentPower_L = maxReleasePower;
                }
                if (currentPower_R > maxReleasePower)
                {
                    currentPower_R = maxReleasePower;
                }
                Dash(currentPower_L, currentPower_R);
            }
        }
        else
        {
            ResetDashValues();
        }

        float speedPerc = softSpeedCap.GetSpeedPercentage();
        if (speedPerc >= tier1RequiredPercentage)
        {
            speedTier = 1;
            currentSpeedTierMulti = currentTier1Multi;
        }
        //if (speedPerc > tier2RequiredPercentage)
        //{
        //    speedTier = 2;
        //    currentSpeedTierMulti = tier2Multi;
        //}

        if (gripHeld_L && !gripHeld_R)
        {
            SendHaptics(0, powerPercentage_L + 0.1f);
        }
        if (gripHeld_R && !gripHeld_L)
        {
            SendHaptics(1, powerPercentage_R + 0.1f);
        }

        controllerSpeed_L = pI.controllerVelocity_L.magnitude;
        controllerSpeed_R = pI.controllerVelocity_R.magnitude;

        //The closer our speed to our threshold the less the dash becomes worth.
        currentAboveSpeedCapMulti = Mathf.Lerp(1, 0.35f, softSpeedCap.GetSpeedPercentage());

        if (canDash_L && gripHeld_L && currentPower_L < maxReleasePower && controllerSpeed_L > minRequiredControllerSpeed)
        {
            currentPower_L += controllerSpeed_L / 7;
            powerPercentage_L = currentPower_L / maxReleasePower;

        }
        if (canDash_R && gripHeld_R && currentPower_R < maxReleasePower && controllerSpeed_R > minRequiredControllerSpeed)
        {
            currentPower_R += controllerSpeed_R / 7;
            powerPercentage_R = currentPower_R / maxReleasePower;
        }

        if (Time.time > lastDashTimeStamp + dragDelay)
        {
            drag.ResetDragToDefault(); 
        }

        bool triggerHeld_L = pI.playerInput.Left.TriggerButton.IsPressed();
        bool triggerHeld_R = pI.playerInput.Right.TriggerButton.IsPressed();

        // Sprint Mode is now based on holding BOTH grip + trigger on either hand
        bool sprintCondition_L = gripHeld_L && triggerHeld_L;
        bool sprintCondition_R = gripHeld_R && triggerHeld_R;

        bool sprintInputActive = sprintCondition_L || sprintCondition_R;

        if (!sprintActive && sprintInputActive && groundedDetector.isGrounded)
        {
            sprintActive = true;
            currentDashSoftSpeedCapThreshold = speedCapDuringSprint;
            currentTier1Multi = swingMultiDuringSprint;
            speedLineController.PlayBoostRing();
            speedLineController.PlayBigBurstLooping();
        }
        else if (sprintActive && !sprintInputActive)
        {
            sprintActive = false;
            currentDashSoftSpeedCapThreshold = defaultDashSoftSpeedCapThreshold;
            currentTier1Multi = defaultTier1Multi;
            speedLineController.StopBigBurstLooping();
        }
    }

    void FixedUpdate()
    {
        if (canDash_L && gripHeld_L && currentPower_L < maxReleasePower && controllerSpeed_L > minRequiredControllerSpeed)
        {
            cM.playerRB.AddForce(
                cameraSteer.GetHorizontalCameraDirection() *
                ((currentPower_L * duringDashMotionPower * currentSpeedTierMulti) * currentAboveSpeedCapMulti) * Time.fixedDeltaTime,
                ForceMode.VelocityChange
            );
        }

        if (canDash_R && gripHeld_R && currentPower_R < maxReleasePower && controllerSpeed_R > minRequiredControllerSpeed)
        {
            cM.playerRB.AddForce(
                cameraSteer.GetHorizontalCameraDirection() *
                ((currentPower_R * duringDashMotionPower * currentSpeedTierMulti) * currentAboveSpeedCapMulti) * Time.fixedDeltaTime,
                ForceMode.VelocityChange
            );
        }
    }

    void Dash(float currentPowerL, float currentPowerR)
    {
        drag.ChangeDrag(dragDuringRunning); 
        lastDashTimeStamp = Time.time;

        if (currentPowerL > currentPowerR)
        {
            currentTotalPower = currentPowerL;
            if (canDash_L)
            {
                float powerPercent = currentTotalPower / maxReleasePower;
                skateSway.SwayLeft(powerPercent);
                speedLineController.PlaySmallBurst_L();
                pI.playerHaptic_L.SendHapticImpulse(releaseHapticAmplitude, hapticDuration);
                canDash_L = false;
            }
        }
        if (currentPowerR > currentPowerL)
        {
            currentTotalPower = currentPowerR;
            if (canDash_R)
            {
                float powerPercent = currentTotalPower / maxReleasePower;
                skateSway.SwayRight(powerPercent);
                speedLineController.PlaySmallBurst_R();
                pI.playerHaptic_R.SendHapticImpulse(releaseHapticAmplitude, hapticDuration);
                canDash_R = false;
            }
        }

        currentTotalPower = Mathf.Clamp(currentTotalPower, 0, maxReleasePower);
        if (softSpeedCap.speedPercentage < currentDashSoftSpeedCapThreshold)
        {
            cM.playerRB.AddForce(cameraSteer.GetHorizontalCameraDirection() * (currentTotalPower * currentSpeedTierMulti), ForceMode.VelocityChange);
        }
        else
        {
            cM.playerRB.AddForce(cameraSteer.GetHorizontalCameraDirection() * ((currentTotalPower * currentSpeedTierMulti) * currentAboveSpeedCapMulti), ForceMode.VelocityChange);
        }

        ResetDashValues();

        if (sprintActive)
        {
            overclockData.DrainOverclockBar(drainAmount); 
        }
    }

    public void ResetDashValues()
    {
        currentTotalPower = 0;
        currentPower_L = 0;
        currentPower_R = 0;
        powerPercentage_L = 0;
        powerPercentage_R = 0;
    }

    void DashStartSFX()
    {
        sfx_Dash.pitch = Random.Range(0.95f, 1.05f);
        sfx_Dash.Play();
        DOTween.To(() => sfx_Dash.volume, x => sfx_Dash.volume = x, dashSFX_Volume, 0.15f);
    }

    void DashStopSFX()
    {
        DOTween.To(() => sfx_Dash.volume, x => sfx_Dash.volume = x, 0, 0.15f);
    }

    void SendHaptics(int hand, float power)
    {
        if (hand == 0)
        {
            pI.playerHaptic_L.SendHapticImpulse(power, hapticDuration);
        }
        if (hand == 1)
        {
            pI.playerHaptic_R.SendHapticImpulse(power, hapticDuration);
        }
    }

    public override void ResetAbility()
    {
        base.ResetAbility();

        ResetDashValues();
        DashStopSFX();
    }
}
