using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class Gimmick_Pulley : MonoBehaviour
{
    [Header("Type")]
    public PulleyType pulleyType;
    public enum PulleyType { straight, swing }

    [Header("PulleyMoveTime")]
    public float pulleyMoveTime = 1.5f;
    public float pulleyEndYOffset = 0.5f; 
    public Ease easeType = Ease.InOutSine;

    [Header("State")]
    [ReadOnly] public bool isPulleyActive;
    [ReadOnly] public bool isPulleyReady;
    private Vector3 lastPulleyPos;

    [Header("SFX")]
    private AudioSource source;
    public AudioClip clip_Release;
    public AudioClip clip_Looping;

    [Header("References")]
    public PlayerAddForce addForce_Launch;
    public Transform pulleyStartPos;
    [ShowIf(nameof(pulleyType), PulleyType.swing)] public Transform pulleyMiddlePos;
    [ShowIf(nameof(pulleyType), PulleyType.swing)] public Transform pulleySwingBase;
    public Transform pulleyEndPos;
    public Transform pulleyVisual;

    [Header("Wire")]
    public bool useLineRenderer;
    public LineRenderer wireLineRenderer;
    public List<Transform> wirePoints;

    [Header("Managers")]
    private PlayerPassivesManager pP;
    private PlayerComponentManager cM;
    private Rigidbody rb;
    private ModularCustomGravity playerGravity;
    private PlayerGrabbable grabbable;
    private PlayerGrabbableSystem playerGrabbable;

    private void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance;
    }

    private void Start()
    {
        rb = cM.playerRB;
        playerGravity = pP.GetPassive<ModularCustomGravity>();
        grabbable = GetComponentInChildren<PlayerGrabbable>();
        playerGrabbable = pP.GetPassive<PlayerGrabbableSystem>();
        source = GetComponent<AudioSource>();
        DOTween.defaultUpdateType = UpdateType.Fixed;
        if (useLineRenderer) { SpawnPulleyWire(); }
        ResetPulleyState();
    }

    public void SpawnPulleyWire()
    {
        if (pulleyType == PulleyType.straight)
        {
            if (wirePoints == null || wirePoints.Count == 0) return;

            wireLineRenderer.positionCount = wirePoints.Count;

            for (int i = 0; i < wirePoints.Count; i++)
            {
                if (wirePoints[i] != null)
                    wireLineRenderer.SetPosition(i, wirePoints[i].position);
            }
        }
        //if (pulleyType == PulleyType.swing)
        //{
        //    //wireLineRenderer.SetPosition(1, pulleySwingBase.position);
        //}
    }

    public void UpdatePulleyWireStartLocation()
    {
        if (useLineRenderer) { wireLineRenderer.SetPosition(0, pulleyVisual.position); }
    }

    private void Update()
    {
        if (isPulleyActive && !playerGrabbable.isGrabbing)
        {
            DetachFromPulley();
        }
    }

    private void FixedUpdate()
    {
        if (isPulleyActive)
        {
            rb.linearVelocity = Vector3.zero;
            playerGravity.DisableGravity();
            Vector3 pulleyDelta = pulleyVisual.position - lastPulleyPos;
            rb.MovePosition(rb.position + pulleyDelta);
            lastPulleyPos = pulleyVisual.position;
            UpdatePulleyWireStartLocation();
        }
    }

    public void AttachToPulley()
    {
        if (isPulleyReady == false) { return; }

        if (!playerGrabbable.isGrabbing)
        {
            DetachFromPulley();
            return;
        }

        grabbable.enabled = false; //Prevent regrab
        PulleyActive(true);
        PulleyReady(false);
        playerGravity.DisableGravity();
        addForce_Launch.ResetVelocity(); //We reset velocity here once upon entering.
        lastPulleyPos = pulleyVisual.position;
        MoveToEnd();
        PlaySFX(clip_Looping, true);
    }

    public void DetachFromPulley()
    {
        PulleyActive(false);
        MoveToStart();
        PlaySFX(clip_Release, false);
        playerGrabbable.ResetState();
        addForce_Launch.StartAddForce();
    }

    void ResetPulleyState()
    {
        pulleyVisual.transform.position = pulleyStartPos.position;
        UpdatePulleyWireStartLocation();
        grabbable.enabled = true;
        PulleyReady(true);
    }

    void PulleyReady(bool state)
    {
        isPulleyReady = state;
    }

    void PulleyActive(bool state)
    {
        isPulleyActive = state;
    }

    private Tween tween_MoveToEnd;
    void MoveToEnd()
    {
        KillActiveTweens();
        if (pulleyType == PulleyType.straight)
        {
            Vector3 endLocation = new Vector3(pulleyEndPos.position.x, pulleyEndPos.position.y + pulleyEndYOffset, pulleyEndPos.position.z);
            tween_MoveToEnd = pulleyVisual.transform.DOMove(endLocation, pulleyMoveTime).SetEase(easeType)
      .OnComplete(() => DetachFromPulley());
        }
        if (pulleyType == PulleyType.swing)
        {
            Vector3[] path = new Vector3[] { pulleyStartPos.position, pulleyMiddlePos.position, pulleyEndPos.position };

            tween_MoveToEnd = pulleyVisual.DOPath(path, pulleyMoveTime, PathType.CatmullRom)
                .SetEase(easeType)
                .SetLookAt(0.01f, Vector3.forward)
                .OnComplete(() => DetachFromPulley());
        }
    }

    private Tween tween_MoveToStart;
    void MoveToStart()
    {
        KillActiveTweens();

        if (pulleyType == PulleyType.straight)
        {
            tween_MoveToStart = pulleyVisual.transform.DOMove(pulleyStartPos.position, 0.2f).SetEase(easeType)
        .OnComplete(() => ResetPulleyState());
        }
        if (pulleyType == PulleyType.swing)
        {
            Vector3[] path = new Vector3[] { pulleyEndPos.position, pulleyMiddlePos.position, pulleyStartPos.position };

            tween_MoveToStart = pulleyVisual.DOPath(path, pulleyMoveTime, PathType.CatmullRom)
                .SetEase(easeType)
                .SetLookAt(0.01f, Vector3.forward)
                    .OnComplete(() => ResetPulleyState());
        }
    }

    public void PlaySFX(AudioClip clip, bool loop)
    {
        source.clip = clip;
        source.loop = loop;
        source.Play();
    }

    void KillActiveTweens()
    {
        if (tween_MoveToEnd != null) { tween_MoveToEnd.Kill(); }
        if (tween_MoveToStart != null) { tween_MoveToStart.Kill(); }
    }
}
