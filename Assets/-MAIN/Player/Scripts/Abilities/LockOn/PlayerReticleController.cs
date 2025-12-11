using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class PlayerReticleController : PlayerPassive
{
    [Header("Hide")]
    public float hideScaleTime = 0.2f;
    public Ease hideScaleEaseType = Ease.InExpo;
    private bool isHidden;

    [Header("LockOn")]
    public float lockOnConfirmScaleTime = 0.1f;
    public Ease lockOnConfirmEaseType = Ease.InExpo;
    public Vector3 lockOnConfirmScale = new Vector3(0.7f, 0.7f, 0.7f);
    public AudioClip sfx_LockOnConfirm;
    [Range(0, 1)] public float sfx_LockOnConfirmVolume = 0.3f;
    [Space]
    public float crossAppearPunchTime;
    public Vector3 crossAppearPunchFactor = new Vector3(0.3f, 0.2f, 0.3f);
    public Ease crosAppear_PunchEase = Ease.InExpo;

    [Header("Appear Cross")]
    public float crossAppearTime = 0.2f;
    public Ease crossAppearEase = Ease.InExpo;
    public Vector3 crossAppearEndScale = new(0.3f, 0.3f, 0.3f);
    [Space]
    public AudioClip sfx_CrossAppear;
    [Range(0, 1)] public float sfx_CrossAppearVolume = 0.3f;

    [Header("Appear Circle")]
    public float circleAppearTime = 0.2f;
    public Ease circleAppearEase = Ease.InExpo;
    public Vector3 circleAppearStartScale = new(2f, 2f, 2f);
    public Vector3 circleAppearEndScale = new(0.6f, 0.6f, 0.6f);

    [Header("Hover")]
    public Vector3 hoverMaxScale = new(1f, 1f, 1f);
    public float rotationSpeed = 0.5f;
    public Ease scaleEaseType = Ease.InOutExpo;

    [Header("Visuals")]
    public Transform reticleCross;
    public Transform reticleCircle;
    public Transform reticleVisualPivot;

    [Header("Refs")]
    private PlayerComponentManager cM;
    private PlayerSFX pSFX;

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pSFX = PlayerSFX.Instance;
    }

    private Tween tweenHide;
    [Button]
    public void Hide()
    {
        KillAllTweens();
        tweenHide = reticleVisualPivot.DOScale(Vector3.zero, hideScaleTime).SetEase(hideScaleEaseType);
        isHidden = true; 
    }

    private Sequence appearSequence;
    private Vector3 targetLocation;
    private float objectRadius; 
    [Button]
    public void Appear(Vector3 location, float radius)
    {
        //Debug.Log("Appear");
        KillAllTweens();
        appearSequence = DOTween.Sequence();

        isHidden = false;
        targetLocation = location; //Gets used in update below. 
        objectRadius = radius; 
        reticleCross.localScale = Vector3.zero;
        reticleCircle.localScale = Vector3.zero;

        appearSequence
           .Append(reticleVisualPivot.DOScale(Vector3.one, 0.05f))
           .Append(reticleCross.DOScale(crossAppearEndScale, crossAppearTime).SetEase(crossAppearEase))
           .AppendCallback(() => PlayAppearSFX())
           .AppendCallback(() => reticleCircle.localScale = circleAppearStartScale)
           .Append(reticleCircle.DOScale(circleAppearEndScale, circleAppearTime).SetEase(circleAppearEase))
           .OnComplete(() => HoverRepeat());
        appearSequence.Play();
    }

    public void PlayAppearSFX()
    {
        pSFX.PlaySFX(sfx_CrossAppear, sfx_CrossAppearVolume);
    }

    private Sequence hoverSequence;
    private Tween tweenRotation;
    [Button]
    public void HoverRepeat()
    {
        //Debug.Log("Repeat");
        hoverSequence?.Kill();
        tweenRotation?.Kill();
        hoverSequence = DOTween.Sequence();

        // Reset rotation to ensure initial state is correct
        reticleCircle.localRotation = Quaternion.Euler(0f, 0f, 0f); // Set initial local rotation

        // Infinite rotation around the Z-axis (local rotation)
        tweenRotation = reticleCircle.DORotate(new Vector3(0f, 0f, -360), 1f / rotationSpeed, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);

        // Infinite pulsing scale
        hoverSequence.Append(reticleCircle.DOScale(hoverMaxScale, 0.5f)
            .SetEase(Ease.InOutSine))
        .Append(reticleCircle.DOScale(circleAppearEndScale, 0.5f)
            .SetEase(Ease.InOutSine))
        .SetLoops(-1, LoopType.Yoyo); // Infinite loop

        hoverSequence.Play();
    }


    private Sequence tweenLockOn;
    [Button]
    public void LockOnActivate()
    {
        //Debug.Log("LockOn");
        tweenRotation?.Kill();
        hoverSequence?.Kill();
        tweenLockOn?.Kill();
        tweenLockOn = DOTween.Sequence();
        tweenLockOn
            .Append(reticleVisualPivot.DOScale(lockOnConfirmScale, lockOnConfirmScaleTime).SetEase(lockOnConfirmEaseType))
            .Append(reticleCross.DOPunchScale(crossAppearPunchFactor, crossAppearPunchTime, 1, 1).SetEase(crosAppear_PunchEase));  
    }

    public void KillAllTweens()
    {
        appearSequence?.Kill();
        hoverSequence?.Kill();
        tweenRotation?.Kill();
        tweenHide?.Kill();
        tweenLockOn?.Kill();
    }

    public void Update()
    {
        if (!isHidden)
        {
            RotateTowardsPlayer();
            SetToObject();
        }
    }

    public void UpdatePosition(Vector3 location) //Called from the lock on script
    {
        targetLocation = location; 
        SetToObject();
        RotateTowardsPlayer(); 
    }

    public void SetToObject()
    {
        // Get direction from object to player
        Vector3 directionToPlayer = (cM.transform_XRRig.position - targetLocation).normalized;
        directionToPlayer.y = 0; // Keep reticle on the same Y level if necessary

        // Calculate offset position
        Vector3 offsetPosition = targetLocation + directionToPlayer * objectRadius;

        // Apply position
        transform.position = offsetPosition;
    }

    public void RotateTowardsPlayer()
    {
        Vector3 directionToPlayer = cM.transform_XRRig.position - transform.position;
        directionToPlayer.y = 0;
        if (directionToPlayer.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = targetRotation;
        }
    }
}
