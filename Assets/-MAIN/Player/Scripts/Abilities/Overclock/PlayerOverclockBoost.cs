using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class PlayerOverclockBoost : PlayerAbility
{
    [Header("Data")]
    [ReadOnly] public bool isOverclocked;

    [Header("Settings-Force")]
    [ReadOnly] public float currentOverclockForce;
    public float overclockForce;
    public float defaultSpeedCapMulti = 1.5f;
    public float afterBoostSpeedCapMulti = 1.5f; 

    [Header("Settings-Duration")]
    public float overClockDuration = 0.5f;
    private float overclockTimeStamp; 
    public float speedRampTime = 0.2f;
    public Ease easeType;

    [Header("Settings-FreezeFrames")]
    public float freezeFrames = 0.1f;

    [Header("Audio")]
    public AudioSource sfx_Source;
    public AudioClip sfx_Clip;
    [Range(0, 1)] public float sfx_Volume = 0.4f;

    [Header("Haptics")]
    [PropertyRange(0, 1)]
    public float hapticAmplitude;
    [PropertyRange(0, 1)]
    public float hapticDuration;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerAbilityManager pA;
    private PlayerPassivesManager pP;
    private PlayerInputManager pI; 
    [Header("Refs")]
    private PlayerOverclockData overclockData;
    private PlayerRailGrind playerRailGrind;
    private PlayerRailGrindCornerBehaviour playerRailGrindCornerBehaviour;
    private PlayerFreezeFrame freezeFrame;
    private PlayerSoftSpeedCap softSpeedCap;
    private PlayerCameraSteer cameraSteer;
    private PlayerParticleController speedLineController;
    private PlayerParticleController playerParticleController; 

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pA = PlayerAbilityManager.Instance;
        pP = PlayerPassivesManager.Instance;
        pI = PlayerInputManager.Instance; 
    }

    private void Start()
    {        
        overclockData = pP.GetPassive<PlayerOverclockData>();
        playerRailGrind = pA.GetAbility<PlayerRailGrind>();
        playerRailGrindCornerBehaviour = pA.GetAbility<PlayerRailGrindCornerBehaviour>();
        freezeFrame = pP.GetPassive<PlayerFreezeFrame>();
        softSpeedCap = pP.GetPassive<PlayerSoftSpeedCap>();
        cameraSteer = pP.GetPassive<PlayerCameraSteer>();
        speedLineController = pP.GetPassive<PlayerParticleController>();
        playerParticleController = pP.GetPassive<PlayerParticleController>(); 
    }

    private bool hasReleased = false; 
    void Update()
    {
        //if (pI.stickAxis_Y_L <= 0.5f)
        //{
        //    hasReleased = true;
        //    //Debug.Log("Released");
        //}

        //if (hasReleased && pI.stickAxis_Y_L >= 0.5f)
        //{
        //    //Debug.Log("Held");
        //    if (!isOverclocked && overclockData.currentReadySegments > 0f)
        //    {
        //        StartCoroutine(StartOverclock());
        //    }
        //    hasReleased = false; // Reset so it doesn't trigger repeatedly
        //}

        if (isOverclocked && Time.time > overclockTimeStamp + overClockDuration)
        {
            StopOverclock(); 
        }

//#if UNITY_EDITOR
//        if (pI.playerInput.Left.Primary.WasPressedThisFrame())
//        {
//            DebugFillBar();
//        }
//#endif
    }

    void DebugFillBar()
    {
        overclockData.UpdateOverclockPercentage(1);
    }

    void FixedUpdate()
    {
        if (isOverclocked)
        {
            pI.playerHaptic_L.SendHapticImpulse(hapticAmplitude, hapticDuration);
            pI.playerHaptic_R.SendHapticImpulse(hapticAmplitude, hapticDuration);

            bool isRailGrinding = playerRailGrind.isRailGrinding;
            if (isRailGrinding)
            {
                playerRailGrind.currentMoveSpeed += (currentOverclockForce * 0.65f) * Time.fixedDeltaTime; 
            }
            else
            {
                Vector3 direction = cM.playerRB.linearVelocity.normalized;
                cM.playerRB.AddForce(direction * currentOverclockForce * Time.fixedDeltaTime, ForceMode.VelocityChange); 
            }
        }
    }

    private Tween tween_CurrentOverclockSpeed;
    IEnumerator StartOverclock()
    {
        overclockData.DrainOverclockBar(0.3f); 
        isOverclocked = true;
        overclockTimeStamp = Time.time; 
        bool isRailGrinding = playerRailGrind.isRailGrinding;
        if (isRailGrinding)
        {
            playerRailGrindCornerBehaviour.StartRailGrindFreezeFrame(freezeFrames);
            yield return new WaitForSeconds(freezeFrames);
            playerRailGrind.currentMoveSpeed = playerRailGrind.defaultSoftSpeedCap * defaultSpeedCapMulti;
        }
        else
        {
            freezeFrame.StartFreezeFrame(freezeFrames);
            yield return new WaitForSeconds(freezeFrames);
            cM.playerRB.linearVelocity = cameraSteer.GetHorizontalCameraDirection() * (softSpeedCap.defaultSoftSpeedCap * defaultSpeedCapMulti);
        }

        playerParticleController.PlayBoostRing();
        StartFX();
        PlaySFX(); 
        currentOverclockForce = 0;
        if (tween_CurrentOverclockSpeed != null) { tween_CurrentOverclockSpeed.Kill(); }
        tween_CurrentOverclockSpeed = DOTween.To(() => currentOverclockForce, x => currentOverclockForce = x, overclockForce, speedRampTime * 0.2f);
    }

    void StartFX()
    {
        //Effects
        speedLineController.PlayBigBurstLooping();
        //cm.playerSpeedLineController.PlayLoop(); 
        //Source.Play(SFX); 
        //cm.PlayerMusic.DistortMusic(overclockDuration, distortionAmount) >>> this should be in a global system with it's own reset.
    }

    void StopFX()
    {
        speedLineController.StopBigBurstLooping();
    }

    void StopOverclock()
    {
        isOverclocked = false;
        //currentOverclockForce = 0;
        //if (tween_CurrentOverclockSpeed != null) { tween_CurrentOverclockSpeed.Kill(); }

        bool isRailGrinding = playerRailGrind.isRailGrinding;
        if (isRailGrinding)
        {
            if (playerRailGrind.currentMoveSpeed > playerRailGrind.defaultSoftSpeedCap)
            {
                playerRailGrind.currentMoveSpeed = playerRailGrind.defaultSoftSpeedCap * afterBoostSpeedCapMulti * 0.65f;
            }
        }
        else
        {
            if (cM.playerRB.linearVelocity.magnitude > softSpeedCap.defaultSoftSpeedCap)
            {
                cM.playerRB.linearVelocity = cM.playerRB.linearVelocity.normalized * (softSpeedCap.defaultSoftSpeedCap * afterBoostSpeedCapMulti);
            }
        }

        StopFX(); 
    }

    void PlaySFX()
    {
        sfx_Source.clip = sfx_Clip;
        sfx_Source.volume = sfx_Volume;
        sfx_Source.Play();
    }

    public override void ResetAbility()
    {
        base.ResetAbility();

        StopOverclock();
    }
}
