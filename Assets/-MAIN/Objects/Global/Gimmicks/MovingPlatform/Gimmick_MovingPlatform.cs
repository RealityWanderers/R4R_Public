using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class Gimmick_MovingPlatform : MonoBehaviour
{
    [Header("Settings")]
    public bool moveAtStart;
    public bool loop;
    public float moveTime;
    public float delay;
    public Ease easeType;

    [Header("Checks")]
    [ReadOnly] public bool playerOnPlatform;

    [Header("Platform Refs")]
    public Transform transform_Start;
    public Transform transform_End;
    public GameObject platform;

    [Header("Refs")]
    private PlayerComponentManager cM;
    private PlayerPassivesManager pP;
    private PlayerAbilityManager pA;
    private ModularGroundedDetector groundedDetector;
    private ModularCustomGravity gravity;
    private PlayerJump playerJump;
    private Rigidbody rb;
    private Vector3 lastPlatformPosition;
    private Transform playerTransform;

    public void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance;
        pA = PlayerAbilityManager.Instance;
    }

    public void Start()
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

    private void Update()
    {
        if (playerJump.jumpStarted && playerOnPlatform)
        {
            DetachFromPlatform();
            //Debug.Log("JumpDetach");
        }
    }

    private void LateUpdate()
    {
        if (playerOnPlatform && playerTransform != null && !playerJump.jumpStarted)
        {
            gravity.DisableGravity();
            Vector3 platformDelta = platform.transform.position - lastPlatformPosition;
            rb.MovePosition(rb.position + platformDelta);
            //Debug.Log("MovingPlayer"); 
        }

        lastPlatformPosition = platform.transform.position;
    }

    public void ResetPlatformPosition()
    {
        platform.transform.position = transform_Start.position;
    }

    [Button]
    public void MoveToEnd()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(delay);
        sequence.Append(platform.transform.DOMove(transform_End.position, moveTime).SetEase(easeType));
    }

    [Button]
    public void MoveToStart()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(delay);
        sequence.Append(platform.transform.DOMove(transform_Start.position, moveTime).SetEase(easeType));
    }

    [Button]
    public void MoveLooping()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(delay);
        sequence.Append(platform.transform.DOMove(transform_End.position, moveTime).SetEase(easeType));
        sequence.AppendInterval(delay);
        sequence.Append(platform.transform.DOMove(transform_Start.position, moveTime).SetEase(easeType));
        sequence.SetLoops(-1); // Infinite loop
    }

    public void OnTriggerEnter(Collider other)
    {
        PlayerObject player = other.GetComponentInParent<PlayerObject>();
        if (player != null && !playerJump.jumpStarted)
        {
            AttachToPlatform(player);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        PlayerObject player = other.GetComponentInParent<PlayerObject>();
        if (player != null)
        {
            DetachFromPlatform();
        }
    }

    public void AttachToPlatform(PlayerObject player)
    {
        lastPlatformPosition = platform.transform.position;
        playerTransform = player.transform;
        playerOnPlatform = true;
        //Debug.Log("PlayerOn");
    }

    public void DetachFromPlatform()
    {
        playerOnPlatform = false;
        playerTransform = null;
        gravity.EnableGravity();
        //Debug.Log("PlayerOff");
    }
}