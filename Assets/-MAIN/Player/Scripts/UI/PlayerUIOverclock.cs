using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; 

public class PlayerUIOverclock : PlayerUI
{
    [Header("Bar-Settings")]
    public float startX = -1.01f;
    public float endX = -0.378f; 
    public Ease easeTypeBar;
    public float fillDurationBar;

    [Header("Segment-Settings")]
    public float segmentScaleInAmount = 1.3f; 
    public Ease easeTypeSegment;
    public float segmentScaleInDuration;
    public float segmentReadyPulseAmount;
    public float segmentReadyPulseSpeed; 

    [Header("Punch-Settings")]
    public Vector3 rootPunchScale;
    public float rootPunchScaleDuration = 0.4f;

    [Header("Refs")]
    public RectTransform overClockUIRoot; 
    public Image image_BarFill;
    public Image image_Segment1;
    public Image image_Segment2;
    public Image image_Segment3;
    private PlayerComponentManager cM;
    private Tweener tween_PunchScale; 

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
    }

    [Button]
    public void UpdateSegments(float readySegments)
    {
        // Scale in or out each segment based on the current readySegments value
        ScaleSegment(image_Segment1, readySegments >= 1);
        ScaleSegment(image_Segment2, readySegments >= 2);
        ScaleSegment(image_Segment3, readySegments >= 3);
    }

    //private void ScaleSegment(Image segment, bool shouldScaleIn)
    //{
    //    // Get the target scale based on whether the segment should scale in or out
    //    Vector3 targetScale = shouldScaleIn ? (Vector3.one * segmentScaleInAmount) : Vector3.zero;

    //    // Stop any previous animations on this rectTransform
    //    segment.rectTransform.DOKill();

    //    if (shouldScaleIn)
    //    {
    //        // Scale in the segment
    //        segment.rectTransform.DOScale(targetScale, segmentScaleInDuration).SetEase(easeTypeSegment)
    //            .OnComplete(() =>
    //            {
    //                segment.rectTransform.DOScale(Vector3.one, 0.2f).OnComplete(() =>
    //                {
    //                    // Start pulsating loop after scaling in
    //                    segment.rectTransform.DOScale(Vector3.one * segmentReadyPulseAmount, segmentReadyPulseSpeed)
    //                    .SetEase(Ease.InOutSine)
    //                    .SetLoops(-1, LoopType.Yoyo);

    //                });
    //            });
    //    }
    //    else
    //    {
    //        // Scale out the segment (stop pulsating if scaling out)
    //        segment.rectTransform.DOScale(targetScale, segmentScaleInDuration).SetEase(easeTypeSegment);
    //    }
    //}

    private void ScaleSegment(Image segment, bool shouldScaleIn)
    {
        // Track whether the segment is currently scaled in or out
        bool isCurrentlyScaledIn = segment.rectTransform.localScale.magnitude > 0.1f; // Using magnitude to avoid float precision issues

        // Get the target scale based on whether the segment should scale in or out
        Vector3 targetScale = shouldScaleIn ? (Vector3.one * segmentScaleInAmount) : Vector3.zero;

        if (shouldScaleIn)
        {
            // Skip if the segment is already scaled in
            if (isCurrentlyScaledIn) return;

            // Stop any previous animations on this rectTransform
            segment.rectTransform.DOKill();

            // Scale in the segment
            segment.rectTransform.DOScale(targetScale, segmentScaleInDuration).SetEase(easeTypeSegment)
                .OnComplete(() =>
                {
                    segment.rectTransform.DOScale(Vector3.one, 0.2f).OnComplete(() =>
                    {
                    // Start pulsating loop after scaling in
                    segment.rectTransform.DOScale(Vector3.one * segmentReadyPulseAmount, segmentReadyPulseSpeed)
                            .SetEase(Ease.InOutSine)
                            .SetLoops(-1, LoopType.Yoyo);
                    });
                });
        }
        else
        {
            // Skip if the segment is already scaled out
            if (!isCurrentlyScaledIn) return;

            // Scale out the segment (stop pulsating if scaling out)
            segment.rectTransform.DOKill();
            segment.rectTransform.DOScale(targetScale, segmentScaleInDuration).SetEase(easeTypeSegment);
        }
    }

    [Button]
    public void UpdateBar(float percentage)
    {
        float percentageToXPos =  Mathf.Lerp(startX, endX, percentage);
        image_BarFill.rectTransform.DOAnchorPosX(percentageToXPos, fillDurationBar).SetEase(easeTypeSegment);
    }

    public void DoPunchScale()
    {
        if (tween_PunchScale != null) { tween_PunchScale.Kill(); }
        overClockUIRoot.localScale = Vector3.one;
        tween_PunchScale = overClockUIRoot.DOPunchScale(rootPunchScale, rootPunchScaleDuration, 2); 
    }

    public override void ResetUI()
    {
        base.ResetUI();
        UpdateBar(0);
        UpdateSegments(0);
    }
}
