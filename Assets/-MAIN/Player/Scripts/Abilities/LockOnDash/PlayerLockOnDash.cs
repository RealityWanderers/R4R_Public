using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class PlayerLockOnDash : PlayerAbility
{
    [Header("Settings")]
    public float dashTime = 0.3f;
    public Ease easeType;
    public float endLaunchSpeed;

    [Header("Data")]
    private Vector3 endLocation;
    [ReadOnly] public bool isDashing = false;
    private Vector3 startLocation;

    [Header("Input")]
    public float TriggerButtonThreshold = 0.6f;
    public bool holdingBothTriggers;
    public float requiredControllerSpeed = 2f;

    [Header("FreezeFrames")]
    public float freezeFrames = 0.1f; 

    //[Header("Cooldown")]
    //public float cooldownTime = 0.2f;
    //[ReadOnly] public bool isOnCooldown;

    [Header("Audio")]
    public AudioSource sfx_Source;
    public AudioClip sfx_Clip;
    [Range(0, 1)] public float sfx_Volume = 0.4f;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerAbilityManager pA; 
    private PlayerPassivesManager pP;
    private PlayerInputManager pI; 
    [Header("Refs")]
    private Tweener tween_Dash;
    private PlayerLockOnSystem lockOnSystem;
    private ModularCustomGravity customGravity;
    private PlayerReticleController reticle;
    private PlayerParticleController playerParticleController; 

    [Header("Haptics")]
    [PropertyRange(0, 1)]
    public float hapticAmplitude = 0.45f;
    [PropertyRange(0, 1)]
    public float hapticDuration = 0.2f;

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pA = PlayerAbilityManager.Instance;
        pP = PlayerPassivesManager.Instance;
        pI = PlayerInputManager.Instance; 
    }

    private void Start()
    {
        lockOnSystem = pP.GetPassive<PlayerLockOnSystem>();
        customGravity = pP.GetPassive<ModularCustomGravity>();
        reticle = pP.GetPassive<PlayerReticleController>();
        playerParticleController = pP.GetPassive<PlayerParticleController>(); 
    }

    public void Update()
    {
        //float triggerValue_L = pI.triggerValue_L;
        //float triggerValue_R = pI.triggerValue_R;

        //float speed_L = pI.controllerVelocity_L.magnitude;
        //float speed_R = pI.controllerVelocity_R.magnitude;

        //if (triggerValue_L > TriggerButtonThreshold && triggerValue_R > TriggerButtonThreshold)
        //{
        //    holdingBothTriggers = true;
        //}
        //if (triggerValue_L < TriggerButtonThreshold && triggerValue_R < TriggerButtonThreshold)
        //{
        //    holdingBothTriggers = false;
        //}

        //if (!isOnCooldown && holdingBothTriggers && speed_L > requiredControllerSpeed && speed_R > requiredControllerSpeed)
        //{
        //    if (lockOnSystem.isLockedOn)
        //    {
        //        StartLockOnDash();
        //    }
        //}
    }

    public void StartLockOnDash()
    {
        if (isDashing == true) { return; }
        reticle.LockOnActivate(); 
        pA.ResetAbilityByType<PlayerJump>();
        //isOnCooldown = true;
        //Debug.Log("BeforeFreeze");
        //freezeFrame.StartFreezeFrame(freezeFrames);
        //yield return new WaitForSeconds(freezeFrames);
        //Debug.Log("AfterFreeze");
        cM.playerRB.linearVelocity = Vector3.zero;
        isDashing = true;
        startLocation = cM.playerRB.transform.position;

        pI.playerHaptic_L.SendHapticImpulse(hapticAmplitude, hapticDuration);
        pI.playerHaptic_R.SendHapticImpulse(hapticAmplitude, hapticDuration);

        Vector3 playerRigLocation = cM.transform_XRRig.position;
        Vector3 playerCameraLocation = cM.transform_MainCamera.position;
        Vector3 difference = playerCameraLocation - playerRigLocation;
        difference.y = 0; 

        endLocation = lockOnSystem.object_CurrentLockOnTarget.position - difference; 
        tween_Dash = cM.playerRB.DOMove(endLocation, dashTime)
    .SetEase(easeType)
    .OnComplete(() =>
    {
        ApplyVelocity();
        StopDash();
        //Debug.Log("LockOnDashComplete"); 
    });

        PlaySFX();
        playerParticleController.PlayBoostRing();
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            customGravity.DisableGravity(); 
        }
    }

    void ApplyVelocity()
    {
        Vector3 direction = (endLocation - startLocation).normalized;
        cM.playerRB.linearVelocity = direction.normalized * endLaunchSpeed;
    }

    void StopDash()
    {
        isDashing = false;
        customGravity.EnableGravity();
        if (tween_Dash != null) { tween_Dash.Kill(); }
        //StartCoroutine(CoolDownTimer());
    }

    //IEnumerator CoolDownTimer()
    //{
    //    isOnCooldown = true;
    //    yield return new WaitForSeconds(cooldownTime);
    //    isOnCooldown = false;
    //}

    void PlaySFX()
    {
        sfx_Source.clip = sfx_Clip;
        sfx_Source.volume = sfx_Volume;
        sfx_Source.Play();
    }

    public override void ResetAbility()
    {
        base.ResetAbility();
        //Debug.Log("Reset");
        StopDash(); 
    }
}
