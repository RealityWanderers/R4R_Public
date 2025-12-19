using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;
using Sirenix.OdinInspector;

[RequireComponent(typeof(PhoneUIHomeMenuData))]
public class PhoneUIHomeMenuAnimation : MonoBehaviour
{
    [Header("Animation-Selection")]
    public float selectionIndicatorSpeed = 0.1f;
    public RectTransform selectionIndicatorParent;
    public RectTransform selectionButton;
    public float selectionIndicatorPulseSpeed = 0.3f;
    public Vector3 selectionindicatorPulseScale = new(1.2f, 1.2f, 1.2f);

    [Header("Animation-Selection")]
    public float rotationAmount = 5;
    public float rotationSpeed = 0.13f;
    public float rotationInterval = 0.22f;

    [Header("References")]
    private PhoneUIHomeMenuData homeMenuData;

    private void Awake()
    {
        homeMenuData = GetComponent<PhoneUIHomeMenuData>();
    }

    private void OnDisable()
    {
        sequence_HighlightRotation?.Kill();
    }

    public void ShowSelectIndicator()
    {
        selectionIndicatorParent.gameObject.SetActive(true);
    }

    public void HideSelectIndicator()
    {
        selectionIndicatorParent.gameObject.SetActive(false);
    }

    private Tween tween_SelectionScale;
    public void AnimateSelectionScale()
    {
        tween_SelectionScale?.Kill();
        tween_SelectionScale = DOTween.Sequence();
        foreach (var picture in homeMenuData.instancedIcons)
        {
            picture.GetComponent<RectTransform>().localScale = Vector3.one;
        }
        RectTransform target = homeMenuData.GetCurrentIconRect();

        if (target == null) { return; }

        tween_SelectionScale = target.DOScale(1.15f, 0.1f);

        RawImage[] rawImages = target.GetComponentsInChildren<RawImage>(true);
    }

    private Sequence sequence_HighlightRotation;
    public void AnimateSelectionRotation()
    {
        sequence_HighlightRotation?.Kill();
        sequence_HighlightRotation = DOTween.Sequence();
        foreach (var picture in homeMenuData.instancedIcons)
        {
            picture.GetComponent<RectTransform>().localRotation = Quaternion.identity;
        }
        RectTransform target = homeMenuData.GetCurrentIconRect();

        if (target == null) { return; }

        sequence_HighlightRotation.Append(target.DOLocalRotate(new Vector3(0, 0, rotationAmount), rotationSpeed))
            .AppendInterval(rotationInterval)
            .Append(target.DOLocalRotate(new Vector3(0, 0, -rotationAmount), rotationSpeed))
            .AppendInterval(rotationInterval)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);
    }

    public void UpdateSelectionIndicator(Vector2 pos)
    {
        selectionIndicatorParent.DOAnchorPos(pos, selectionIndicatorSpeed);
        PulseSelectionIndicator();
        //Debug.Log("Move to" + pos);
    }

    private Tween tween_PulseSelectionIndicator;
    public void PulseSelectionIndicator()
    {
        tween_PulseSelectionIndicator?.Kill();
        selectionButton.localScale = Vector3.one;
        tween_PulseSelectionIndicator = selectionButton.DOScale(selectionindicatorPulseScale, selectionIndicatorPulseSpeed)
    .SetEase(Ease.Linear)
    .SetLoops(-1, LoopType.Yoyo);
    }
}
