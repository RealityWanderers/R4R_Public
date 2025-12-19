using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;
using Sirenix.OdinInspector;

[RequireComponent(typeof(PhoneUIAlbumData))]
[RequireComponent(typeof(PhoneUIAlbumSelectedPicture))]
public class PhoneUIAlbumAnimation : MonoBehaviour
{
    [Header("Animation-Selection")]
    public float selectionIndicatorSpeed = 0.1f;
    public RectTransform selectionIndicatorParent;
    public RectTransform selectionButton;
    public float selectionIndicatorPulseSpeed = 0.3f;
    public Vector3 selectionindicatorPulseScale = new (1.2f,1.2f,1.2f);

    [Header("Animation-Selection")]
    public float rotationAmount = 5;
    public float rotationSpeed = 0.13f;
    public float rotationInterval = 0.22f;

    [Header("Animation-PictureOptions")]
    [Space]
    public RectTransform rect_BackToCamera;
    public RectTransform rect_BackToAlbum;
    [Space]
    public RectTransform rect_TrashCan;
    public Vector3 punchScaleTrashCan = new(1.2f,1.2f,1.2f);
    public float punchScaleTrashCanDuration = 0.25f;
    [Space]
    public RectTransform rect_OpenExplorer;
    public Vector3 punchScaleOpenExplorer = new(1.2f, 1.2f, 1.2f);
    public float punchScaleOpenExplorerDuration = 0.25f;

    [Header("Animation-Notifications")]
    public RectTransform rect_NotificationExplorer;

    [Header("Animation-Scroll")]
    public RectTransform rect_ScrollIndicator;
    public Vector2 scrollIndicatorMinMax = new(21, -15f);
    public float scrollIndicatorMoveTime = 0.05f;

    [Header("Animation-Loading")]
    public RectTransform rect_AlbumLoadingParent;
    public RectTransform rect_AlbumLoadingSpinner;
    public float albumLoadingSpinnerSpeed = 0.5f;

    [Header("Animation-DeleteCross")]
    public Vector3 punchScaleDeleteCross = new Vector3(1.015f, 1.015f, 1.015f);
    public float punchScaleDurationDeleteCross = 0.2f;
    public Ease punchScaleEaseDeleteCross;
    public RectTransform rect_DeleteCross;

    [Header("Animation-NoPictures")]
    public RectTransform rect_NoPictures;

    [Header("References")]
    private PhoneUIAlbumData albumData;
    private PhoneUIAlbumSelectedPicture albumSelectedPicture;

    private void Awake()
    {
        albumData = GetComponent<PhoneUIAlbumData>();
        albumSelectedPicture = GetComponent<PhoneUIAlbumSelectedPicture>();
    }

    private void OnEnable()
    {
        rect_NotificationExplorer.localScale = Vector3.zero;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        rect_OpenExplorer.gameObject.SetActive(true);
#else
rect_OpenExplorer.gameObject.SetActive(false);
#endif
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
        foreach (var picture in albumData.instancedPictures)
        {
            picture.GetComponent<RectTransform>().localScale = Vector3.one;
        }
        RectTransform target = albumData.GetCurrentPictureRect();

        if (target == null) { return; }

        tween_SelectionScale = target.DOScale(1.15f, 0.1f);

        RawImage[] rawImages = target.GetComponentsInChildren<RawImage>(true);
    }

    private Sequence sequence_HighlightRotation;
    public void AnimateSelectionRotation()
    {
        sequence_HighlightRotation?.Kill();
        sequence_HighlightRotation = DOTween.Sequence();
        foreach (var picture in albumData.instancedPictures)
        {
            picture.GetComponent<RectTransform>().localRotation = Quaternion.identity;
        }
        RectTransform target = albumData.GetCurrentPictureRect();

        if (target == null) { return; }

        sequence_HighlightRotation.Append(target.DOLocalRotate(new Vector3(0, 0, rotationAmount), rotationSpeed))
            .AppendInterval(rotationInterval)
            .Append(target.DOLocalRotate(new Vector3(0, 0, -rotationAmount), rotationSpeed))
            .AppendInterval(rotationInterval)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);
    }

    public void AnimateScrollProgress()
    {
        float scrollProgress = albumData.GetScrollProgress();
        float scrollPos = Mathf.Lerp(scrollIndicatorMinMax.x, scrollIndicatorMinMax.y, scrollProgress);
        //Debug.Log(scrollPos);
        rect_ScrollIndicator.DOAnchorPosY(scrollPos, scrollIndicatorMoveTime);
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



    private Tween tween_LoadingSpinner;
    public void ShowAlbumLoadingScreen()
    {
        tween_LoadingSpinner?.Kill();
        rect_AlbumLoadingParent.gameObject.SetActive(true);
        rect_AlbumLoadingSpinner.localRotation = Quaternion.identity;
        rect_AlbumLoadingParent.localScale = Vector3.one;
        tween_LoadingSpinner = rect_AlbumLoadingSpinner.DOLocalRotate(new Vector3(0, 0, 360f), albumLoadingSpinnerSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1);
    }

    private Sequence sequence_HideLoadingScreen;
    public void HideAlbumLoadingScreen()
    {
        Debug.Log("Closing");
        sequence_HideLoadingScreen?.Kill();
        sequence_HideLoadingScreen = DOTween.Sequence();
        sequence_HideLoadingScreen
            //.AppendCallback(() => albumSelectedPicture.ClosePicture())
            .AppendInterval(.35f) //Small minimum time that it always shows the loading screen.
            .Append(rect_AlbumLoadingParent.DOScale(Vector3.zero, 0.15f).From(Vector3.one))
            .AppendCallback(() => rect_AlbumLoadingParent.gameObject.SetActive(false))
            .AppendCallback(() => tween_LoadingSpinner?.Kill());
    }



    private Tween tween_ShowSelectedPicture;
    private Tween tween_ShowSelectedPictureBG;
    public void AnimateShowSelectedPicture()
    {
        albumSelectedPicture.ToggleSelectedPictureActiveState(true);
        albumSelectedPicture.selectedPicture.SetActive(true);
        albumSelectedPicture.selectedPictureBG.SetActive(true);
        SwapBackButtonIcon(false);

        tween_ShowSelectedPicture?.Kill();
        tween_ShowSelectedPictureBG?.Kill();
        tween_ShowSelectedPicture = albumSelectedPicture.selectedPictureRawImage.rectTransform.DOScale(Vector3.one, 0.1f).From(Vector3.zero);
        tween_ShowSelectedPictureBG = albumSelectedPicture.selectedPictureBGRawImage.rectTransform.DOScale(Vector3.one, 0.1f).From(Vector3.zero);
    }

    public void AnimateHideSelectedPicture()
    {
        Debug.Log("Hide");
        tween_ShowSelectedPicture?.Kill();
        tween_ShowSelectedPictureBG?.Kill();
        tween_ShowSelectedPictureBG = albumSelectedPicture.selectedPictureBGRawImage.rectTransform.DOScale(Vector3.zero, 0.1f).From(Vector3.one);
        tween_ShowSelectedPicture = albumSelectedPicture.selectedPictureRawImage.rectTransform.DOScale(Vector3.zero, 0.1f).From(Vector3.one).OnComplete(() =>
        {
            albumSelectedPicture.selectedPicture.SetActive(false);
            albumSelectedPicture.selectedPictureBG.SetActive(false);
            albumSelectedPicture.ToggleSelectedPictureActiveState(false);
            SwapBackButtonIcon(true);
        }); ;

    }

    public void SwapBackButtonIcon(bool swapBackButtonToCamera)
    {
        if (swapBackButtonToCamera)
        {
            rect_BackToCamera.gameObject.SetActive(true);
            rect_BackToAlbum.gameObject.SetActive(false);
        }
        else
        {
            rect_BackToCamera.gameObject.SetActive(false);
            rect_BackToAlbum.gameObject.SetActive(true);
        }
    }

    public Sequence sequence_DeletePicture { get; private set; }
    [Button]
    public void DeletePictureSequence()
    {
        sequence_DeletePicture?.Kill();
        sequence_DeletePicture = DOTween.Sequence();

        //albumSelectedPicture.selectedPictureRawImage.rectTransform.localScale = Vector3.one;
        rect_DeleteCross.gameObject.SetActive(true);

        sequence_DeletePicture
            .Append(rect_TrashCan.DOScale(punchScaleTrashCan, punchScaleTrashCanDuration * 0.5f).From(Vector3.one))
            .Append(rect_TrashCan.DOScale(Vector3.one, punchScaleTrashCanDuration * 0.5f))
            .Append(rect_DeleteCross.DOScale(Vector3.one, 0.05f).From(Vector3.zero))
            .Append(rect_DeleteCross.DOPunchScale(punchScaleDeleteCross, punchScaleDurationDeleteCross, 1, 0).SetEase(punchScaleEaseDeleteCross))
            .Append(rect_DeleteCross.DOScale(Vector3.zero, 0.05f)).OnComplete(() =>
            {
                albumData.DeletePicture();
                rect_DeleteCross.gameObject.SetActive(false);
            });
    }

    public Sequence sequence_OpenExplorer { get; private set; }
    public void OpenExplorerSequence()
    {
        sequence_OpenExplorer?.Kill();
        sequence_OpenExplorer = DOTween.Sequence();

        sequence_OpenExplorer
            .Append(rect_OpenExplorer.DOScale(punchScaleOpenExplorer, punchScaleOpenExplorerDuration * 0.5f).From(Vector3.one))
            .Append(rect_OpenExplorer.DOScale(Vector3.one, punchScaleOpenExplorerDuration * 0.5f))
            .AppendCallback(() => albumData.OpenCurrentPictureInExplorer())
            .Append(rect_NotificationExplorer.DOScale(Vector3.one, 0.3f).From(Vector3.zero))
            .AppendInterval(1f)
            .Append(rect_NotificationExplorer.DOScale(Vector3.zero, 0.3f));
    }



    public void ShowNoPicturesIcon()
    {
        rect_NoPictures.gameObject.SetActive(true);
    }

    public void HideNoPicturesIcon()
    {
        rect_NoPictures.gameObject.SetActive(false);
    }
}
