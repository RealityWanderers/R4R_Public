using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using System.Collections;

public class PlayerRailGrind : PlayerAbility
{
    [Header("Snap")]
    public float snappingSpeed = 5f;
    public float snapDistanceThreshold = 0.2f;
    public int resolution = 8;
    public int iterations = 4;
    [ReadOnly] public bool isRailGrinding = false;

    [Header("Detach")]
    public float detachMovementMulti = 1;

    [Header("SoftSpeedCap")]
    public float currentSoftSpeedCap = 5;
    private float defaultSpeedCap;
    public float speedDrainAmountWhenAbove = 5f;
    public float amountAboveSpeedCapFactor = 9f;
    [Header("Data")]
    [ReadOnly] public float defaultSoftSpeedCap;
    [ReadOnly] public float currentDrain;
    [ReadOnly] public float amountAboveSpeedCap;
    [ReadOnly] public float speedPercentage;

    [Header("Speed")]
    [ReadOnly] public float currentMoveSpeed;
    //public float maxSpeed = 8f;
    public float minSpeed = 5f;
    public float idleSpeedLoss = 1;
    public float downHillSpeedGain = 2.5f;
    public float upHillSpeedLoss = 1.5f;
    public float slopeAngleThreshold = 0.5f;
    [ReadOnly] public float currentSlopeAngle;
    [ReadOnly] public float t = 0f;
    [ReadOnly] public bool directionIsSet;
    [ReadOnly] public bool isMovingForward;
    private Vector3 offset;  //This is dynamically set by using the rail radius.
    [HideInInspector] public bool isFirstFrame;

    [Header("Cooldown")]
    public float coolDownTime = 0.15f;
    public bool onCoolDown;

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
    [Header("Refs")]
    [HideInInspector] public SplineContainer splineContainer;
    private Rigidbody playerRB;
    private PlayerQuickDash playerQuickDash;
    private PlayerRailGrindCornerBehaviour playerRailGrindCornerBehaviour;
    private Coroutine coroutine_StartCoolDown;
    private PlayerRailGrindCornerDetector railGrindCornerDetector;
    private PlayerCameraSteer cameraSteer;
    private PlayerRailGrindCornerBehaviour railGrindCornerBehaviour;
    private PlayerParticleController speedLineController; 

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pA = PlayerAbilityManager.Instance;
        pP = PlayerPassivesManager.Instance;
    }

    private void Start()
    {
        playerQuickDash = pA.GetAbility<PlayerQuickDash>();
        railGrindCornerBehaviour = pA.GetAbility<PlayerRailGrindCornerBehaviour>();
        playerRailGrindCornerBehaviour = pA.GetAbility<PlayerRailGrindCornerBehaviour>();
        cameraSteer = pP.GetPassive<PlayerCameraSteer>();
        railGrindCornerDetector = pP.GetPassive<PlayerRailGrindCornerDetector>();
        speedLineController = pP.GetPassive<PlayerParticleController>(); 
        playerRB = cM.playerRB;
        defaultSoftSpeedCap = currentSoftSpeedCap;
    }

    void FixedUpdate()
    {
        if (!onCoolDown)
        {
            if (isRailGrinding)
            {
                MoveAlongSpline();
            }
        }

        if (isRailGrinding)
        {
            if (currentMoveSpeed > currentSoftSpeedCap)
            {
                currentMoveSpeed -= currentDrain * Time.fixedDeltaTime;
            }
        }
    }

    void Update()
    {
        if (isRailGrinding)
        {
            ApplySoftSpeedCap();
        }
    }

    public void ApplySoftSpeedCap()
    {
        if (currentMoveSpeed < 0.01f) //Prevents a bug where the playerspeed would be set back to max speed cap.
        {
            currentMoveSpeed = 0;
            speedPercentage = 0;
        }

        if (currentMoveSpeed > currentSoftSpeedCap)
        {
            amountAboveSpeedCap = currentMoveSpeed - currentSoftSpeedCap;
            currentDrain = speedDrainAmountWhenAbove * (amountAboveSpeedCap * amountAboveSpeedCapFactor);
        }

        if (currentMoveSpeed < currentSoftSpeedCap)
        {
            currentDrain = 0;
            amountAboveSpeedCap = 0;
        }
    }

    public void AttachToSpline()
    {
        if (onCoolDown)
        {
            return;
        }

        if (splineContainer == null || playerRB == null) return;
        Spline spline = splineContainer.Spline;

        playerQuickDash.ResetCharges();

        //Handle offset automatically by grabbing the radius from the extrude object.
        SplineExtrude splineExtrude = splineContainer.GetComponent<SplineExtrude>();
        if (splineExtrude != null)
        {
            if (splineExtrude.Radius != 0)
            {
                offset = new Vector3(0, splineExtrude.Radius / 0.5f, 0);
            }
        }

        Vector3 playerWorldPosition = playerRB.position;
        Vector3 playerLocalPosition = splineContainer.transform.InverseTransformPoint(playerWorldPosition);
        SplineUtility.GetNearestPoint(
            spline,
            (float3)playerLocalPosition,
            out float3 localNearestPoint,
            out float tPosition,
            resolution,  // resolution (number of segments for accuracy)
            iterations   // iterations (refinement for accuracy)
        );

        t = tPosition;
        ChangePlayerAttach(true);
    }

    void MoveAlongSpline()
    {
        if (splineContainer == null || playerRB == null || playerRailGrindCornerBehaviour.isFreezeFramed) return;
        Spline spline = splineContainer.Spline;
        Vector3 localNextSplinePoint = spline.EvaluatePosition(t);
        Vector3 nextSplinePoint = splineContainer.transform.TransformPoint(localNextSplinePoint);
        Vector3 tangent = spline.EvaluateTangent(t);
        Vector3 worldTangent = splineContainer.transform.TransformDirection(tangent);

        if (!directionIsSet)
        {
            isMovingForward = Vector3.Dot(cameraSteer.GetHorizontalCameraDirection(), worldTangent) > 0f;
            directionIsSet = true;
        }

        currentSlopeAngle = Vector3.Dot(worldTangent, Vector3.up);
        if (!isMovingForward)
        {
            currentSlopeAngle = -currentSlopeAngle;
        }

        if (Mathf.Abs(currentSlopeAngle) > slopeAngleThreshold)
        {
            if (currentSlopeAngle > 0f)
            {
                currentMoveSpeed = Mathf.Max(minSpeed, currentMoveSpeed - upHillSpeedLoss * Time.fixedDeltaTime);
                //currentMoveSpeed -= upHillSpeedLoss * Time.fixedDeltaTime;
            }
            else if (currentSlopeAngle < 0f)
            {
                //currentMoveSpeed = Mathf.Min(maxSpeed, currentMoveSpeed + downHillSpeedGain * Time.fixedDeltaTime);
                currentMoveSpeed += downHillSpeedGain * Time.fixedDeltaTime;
            }
        }
        else
        {
            currentMoveSpeed = Mathf.Max(minSpeed, currentMoveSpeed - idleSpeedLoss * Time.fixedDeltaTime);
            //currentMoveSpeed -= -idleSpeedLoss * Time.fixedDeltaTime;
        }

        if (isMovingForward)
        {
            t += currentMoveSpeed * Time.fixedDeltaTime / spline.GetLength();
        }
        else
        {
            t -= currentMoveSpeed * Time.fixedDeltaTime / spline.GetLength();
        }
        t = Mathf.Clamp01(t);

        if (isFirstFrame)
        {
            playerRB.position = nextSplinePoint; // Align player position to spline at the start
            railGrindCornerBehaviour.previousT = t;
            isFirstFrame = false; // No longer the first frame
            cM.playerRecenter.RecenterPlayer(false);
            railGrindCornerDetector.AnalyzeSpline(splineContainer.Spline, isMovingForward); //Analyze spline to get corner positions. 
        }
        else
        {
            // Smooth movement from the current position to the next spline point
            Vector3 smoothedMoveDirection = (nextSplinePoint + offset) - playerRB.position;
            playerRB.MovePosition(playerRB.position + smoothedMoveDirection);
        }
        //Vector3 smoothedMoveDirection = (nextSplinePoint + offset) - playerRB.position;
        //playerRB.MovePosition(playerRB.position + smoothedMoveDirection);

        bool isLoopingSpline = splineContainer.Spline.Closed;
        // Check for out-of-bounds t values
        if (t >= 1f || t <= 0f)
        {
            if (isLoopingSpline)
            {
                // Handle the looping spline behavior
                if (t >= 1f)
                {
                    t = 0f;  // Wrap to start of spline if t goes over 1
                    railGrindCornerBehaviour.ClearProcessedCorners();
                }
                else if (t <= 0f)
                {
                    t = 1f;  // Wrap to end of spline if t goes below 0
                    railGrindCornerBehaviour.ClearProcessedCorners();
                }
            }
            else
            {
                // If the spline is not looping, detach when t is out of bounds
                ChangePlayerAttach(false);
            }
        }

    }

    IEnumerator StartCoolDown()
    {
        onCoolDown = true;
        yield return new WaitForSeconds(coolDownTime);
        onCoolDown = false;
    }

    public void ChangePlayerAttach(bool attached)
    {
        if (attached)
        {
            AttachToRail();
        }
        else
        {
            DetachFromRail(); 
        }
    }

    private void AttachToRail()
    {
        DisableComponents(true);
        isFirstFrame = true;
        currentMoveSpeed = playerRB.linearVelocity.magnitude * 1.15f; //Slight multi so you keep your speed rather than slightly losing it.
        cM.playerRB.linearVelocity = Vector3.zero;
        playerRB.isKinematic = true;
        isRailGrinding = true;
        PlaySFXHit();
        PlaySFXLoop();
        speedLineController.StartRailGrindSpeedLines();
    }

    private void DetachFromRail()
    {
        if (isRailGrinding)
        {
            playerRB.isKinematic = false;
            DisableComponents(false);
            coroutine_StartCoolDown = StartCoroutine(StartCoolDown());
            directionIsSet = false;
            Vector3 direction = railGrindCornerBehaviour.GetDetachDirection();
            playerRB.linearVelocity = direction * (currentMoveSpeed * detachMovementMulti);
            isRailGrinding = false;
            StopSFXLoop();
            speedLineController.StopRailGrindSpeedLines();
        }
    }

    void DisableComponents(bool disable)
    {
        if (disable)
        {
            pP.DisablePassiveByType<PlayerLockOnSystem>();
            pP.DisablePassiveByType<PlayerCameraSteer>();
            pA.DisableAbilityByType<PlayerSkate>();
            pA.DisableAbilityByType<PlayerQuickDash>();
            pA.DisableAbilityByType<PlayerBrake>();
            pA.DisableAbilityByType<PlayerLockOnDash>();
        }
        else
        {
            pP.EnablePassiveByType<PlayerLockOnSystem>();
            pP.EnablePassiveByType<PlayerCameraSteer>();
            pA.EnableAbilityByType<PlayerSkate>();
            pA.EnableAbilityByType<PlayerQuickDash>();
            pA.EnableAbilityByType<PlayerBrake>();
            pA.EnableAbilityByType<PlayerLockOnDash>();
        }
    }

    //public void IncreaseSpeedCapWithDuration(float multi, float duration)
    //{
    //    StartCoroutine(CoroutineIncreaseSpeedCap(multi, duration));
    //}

    //public void IncreaseSpeedCap(float multi)
    //{
    //    currentSoftSpeedCap = defaultSoftSpeedCap * multi;
    //}

    //public IEnumerator CoroutineIncreaseSpeedCap(float multi, float duration)
    //{
    //    currentSoftSpeedCap = defaultSoftSpeedCap * multi;
    //    yield return new WaitForSeconds(duration);
    //    ResetSpeedCap();
    //}

    //public void ResetSpeedCap()
    //{
    //    currentSoftSpeedCap = defaultSoftSpeedCap;
    //}

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

    public override void ResetAbility()
    {
        base.ResetAbility();

        //ResetSpeedCap();
        onCoolDown = false; 
        ChangePlayerAttach(false);
    }
}