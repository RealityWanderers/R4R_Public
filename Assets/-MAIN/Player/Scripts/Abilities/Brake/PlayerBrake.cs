using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;

public class PlayerBrake : PlayerAbility, IAbility
{
    [Header("State")]
    [ReadOnly] public bool isBraking;

    [Header("Settings")]
    public float brakeDelay; 
    public float brakeScaleInTime = 1.5f;
    public Ease brakeEaseType = Ease.InExpo; 
    public float brakeStrengthMulti = 10; 
    private float currentBrakeStrength;

    [Header("Effects")]
    public AudioSource sfx_Brake;

    [Header("Haptics")]
    [PropertyRange(0, 1)]
    public float hapticAmplitude;
    [PropertyRange(0, 1)]
    public float hapticDuration;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerPassivesManager pP;
    private PlayerInputManager pI; 
    [Header("Refs")]
    private ModularGroundedDetector groundedDetector;
    private Rigidbody rb;


    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance;
        pI = PlayerInputManager.Instance; 
    }

    private void Start()
    {
        groundedDetector = pP.GetPassive<ModularGroundedDetector>();
        rb = cM.playerRB;
    }

    void Update()
    {
        bool brakingAction = false;
        //if (pI.stickAxis_Y_L <= -0.5f)
        //{
        //    brakingAction = true; 
        //}
        //if (pI.stickAxis_Y_L >= -0.5f)
        //{
        //    brakingAction = false; 
        //}

        if (pI.gripValue_L > 0.7f && pI.gripValue_R > 0.7f)
        {
            brakingAction = true;
        }
        else
        {
            brakingAction = false;
        }


        if (!isBraking && brakingAction && groundedDetector.isGrounded)
        {
            BrakeStart();
        }

        if (isBraking && !brakingAction || !groundedDetector.isGrounded)
        {
            BrakeCancel();
        }

        if (isBraking && brakingAction)
        {
            pI.playerHaptic_L.SendHapticImpulse(hapticAmplitude, hapticDuration);
            pI.playerHaptic_R.SendHapticImpulse(hapticAmplitude, hapticDuration);
        }
    }

    private Sequence brakeSequence; 
    void BrakeStart()
    {
        isBraking = true;
        sfx_Brake.Play();
        sfx_Brake.volume = 0;
        DOTween.To(() => sfx_Brake.volume, x => sfx_Brake.volume = x, 0.45f, 0.2f);
        currentBrakeStrength = 0;

        brakeSequence = DOTween.Sequence();
        brakeSequence.AppendInterval(brakeDelay);
        brakeSequence.Append(DOTween.To(() => currentBrakeStrength, x => currentBrakeStrength = x, 1, brakeScaleInTime).SetEase(brakeEaseType));
    }

    void BrakeCancel()
    {
        isBraking = false;
        DOTween.To(() => sfx_Brake.volume, x => sfx_Brake.volume = x, 0, 0.2f);
        brakeSequence?.Kill();
    }

    void FixedUpdate()
    {
        if (isBraking)
        {
            Brake();
        }
    }

    public void Brake()
    {
        rb.linearVelocity -= rb.linearVelocity.normalized * (currentBrakeStrength * brakeStrengthMulti * Time.fixedDeltaTime);

        // Ensure velocity doesn’t reverse or oscillate
        if (rb.linearVelocity.magnitude < 0.01f)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    public override void ResetAbility()
    {
        base.ResetAbility();

        BrakeCancel();
    }
}
