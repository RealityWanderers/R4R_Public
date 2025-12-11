using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class BaseMovingPlatform : MonoBehaviour
{
    [Header("Settings")]
    [ShowIf(nameof(ShowMoveAtStart))] public bool moveAtStart;
    [ShowIf(nameof(ShowLoop))] public bool loop;
    [ShowIf(nameof(ShowTimingSettings))] public float moveTime = 1f;
    [ShowIf(nameof(ShowTimingSettings))] public float delay = 0.2f;
    [ShowIf(nameof(ShowTimingSettings))] public Ease easeType = Ease.InOutSine;

    protected virtual bool ShowMoveAtStart => true;
    protected virtual bool ShowLoop => true;
    protected virtual bool ShowTimingSettings => true;

    [Header("Platform Refs")]
    public Transform transform_Start;
    public Transform transform_End;
    public GameObject platform;

    [Header("Debug")]
    [ReadOnly] public bool playerOnPlatform;

    protected Rigidbody rb;
    protected Transform playerTransform;
    protected Vector3 lastPlatformPosition;

    protected PlayerComponentManager cM;
    protected PlayerPassivesManager pP;
    protected PlayerAbilityManager pA;

    protected ModularGroundedDetector groundedDetector;
    protected ModularCustomGravity gravity;
    protected PlayerJump playerJump;

    protected virtual void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pA = PlayerAbilityManager.Instance;
        pP = PlayerPassivesManager.Instance;
    }

    protected virtual void Start()
    {
        gravity = pP.GetPassive<ModularCustomGravity>();
        groundedDetector = pP.GetPassive<ModularGroundedDetector>();
        playerJump = pA.GetAbility<PlayerJump>();
        rb = cM.playerRB;

        DOTween.defaultUpdateType = UpdateType.Fixed;
        ResetPlatformPosition();
        lastPlatformPosition = platform.transform.position;

        if (moveAtStart)
        {
            if (loop)
            {
                MoveLooping();
            }
            else
            {
                MoveToEnd();
            }
        }
    }

    protected virtual void Update()
    {
        if (playerJump.jumpStarted && playerOnPlatform)
        {
            DetachFromPlatform();
        }
    }

    protected virtual void LateUpdate()
    {
        if (playerOnPlatform && playerTransform != null && !playerJump.jumpStarted)
        {
            if (groundedDetector.isGrounded)
            {
                gravity.DisableGravity();
            }

            Vector3 platformDelta = platform.transform.position - lastPlatformPosition;
            rb.MovePosition(rb.position + platformDelta);
        }

        lastPlatformPosition = platform.transform.position;
    }

    protected virtual void ResetPlatformPosition()
    {
        platform.transform.position = transform_Start.position;
    }

    [Button]
    protected virtual void MoveToEnd()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(delay);
        sequence.Append(platform.transform.DOMove(transform_End.position, moveTime).SetEase(easeType));
    }

    [Button]
    protected virtual void MoveToStart()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(delay);
        sequence.Append(platform.transform.DOMove(transform_Start.position, moveTime).SetEase(easeType));
    }

    [Button]
    protected virtual void MoveLooping()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(delay);
        sequence.Append(platform.transform.DOMove(transform_End.position, moveTime).SetEase(easeType));
        sequence.AppendInterval(delay);
        sequence.Append(platform.transform.DOMove(transform_Start.position, moveTime).SetEase(easeType));
        sequence.SetLoops(-1);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        PlayerObject player = other.GetComponentInParent<PlayerObject>();
        if (player != null && !playerJump.jumpStarted)
        {
            AttachToPlatform(player);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        PlayerObject player = other.GetComponentInParent<PlayerObject>();
        if (player != null)
        {
            DetachFromPlatform();
        }
    }

    protected virtual void AttachToPlatform(PlayerObject player)
    {
        lastPlatformPosition = platform.transform.position;
        playerTransform = player.transform;
        playerOnPlatform = true;
    }

    protected virtual void DetachFromPlatform()
    {
        playerOnPlatform = false;
        playerTransform = null;
        gravity.EnableGravity();
    }
}