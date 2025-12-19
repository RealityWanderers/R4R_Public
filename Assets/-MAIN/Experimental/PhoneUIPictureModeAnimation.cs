using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PhoneUIPictureMode))]
public class PhoneUIPictureModeAnimation : MonoBehaviour
{
    [Header("Animation-PictureOptions")]
    public Vector3 punchScaleOptionSelect = new(1.2f, 1.2f, 1.2f);
    public float punchScaleOptionSelectDuration = 0.2f;
    [Space]
    public RectTransform rect_SwapCamera;
    public RectTransform rect_TakePicture;
    public RectTransform rect_OpenAlbum;

    [Header("Animation-Border")]
    public RectTransform rect_BorderUI;

    [Header("Animation-CameraFlash")]
    public RectTransform rect_CameraFlash;

    [Header("Animation-PicturePreview")]
    public RawImage picturePreview;
    public RectTransform rect_PicturePreviewParent;
    public float picturePreviewDuration = 2f;
    public bool picturePreviewActive {get; private set; }

[Header("Animation-Zoom")]
    public float zoomUIHideYPos = 3.15f;
    public float zoomUIShowYPos = -1.2f;
    public RectTransform rect_ZoomParent;
    [Space]
    public Vector2 scrollMinMax = new(-16, -8);
    public RectTransform rect_ZoomScrollProgressIndicator;

    private PhoneUIPictureMode pictureMode;

    private void Awake()
    {
        pictureMode = GetComponent<PhoneUIPictureMode>();
    }

    private void OnEnable()
    {
        HideCameraBorder();
        HideCameraFlashImage();
        HidePicturePreview();
    }

    private Sequence sequence_SwapCamera;
    public void SwapCameraSequence()
    {
        sequence_SwapCamera?.Kill();
        sequence_SwapCamera = DOTween.Sequence();

        sequence_SwapCamera
            .Append(rect_SwapCamera.DOScale(punchScaleOptionSelect, punchScaleOptionSelectDuration).From(Vector3.one))
            .Append(rect_SwapCamera.DOScale(Vector3.one, punchScaleOptionSelectDuration));
    }

    private Sequence sequence_TakePicture;
    public void TakePictureSequence()
    {
        sequence_TakePicture?.Kill();
        sequence_TakePicture = DOTween.Sequence();

        sequence_TakePicture
            .Append(rect_TakePicture.DOScale(punchScaleOptionSelect, punchScaleOptionSelectDuration).From(Vector3.one))
            .Append(rect_TakePicture.DOScale(Vector3.one, punchScaleOptionSelectDuration));
    }

    private Sequence sequence_OpenAlbum;
    public void OpenAlbumSequence()
    {
        sequence_OpenAlbum?.Kill();
        sequence_OpenAlbum = DOTween.Sequence();

        sequence_OpenAlbum
            .Append(rect_OpenAlbum.DOScale(punchScaleOptionSelect, punchScaleOptionSelectDuration).From(Vector3.one))
            .Append(rect_OpenAlbum.DOScale(Vector3.one, punchScaleOptionSelectDuration));
    }

    private Sequence sequence_FlashBorderUI;
    public void FlashCameraBorderUI()
    {
        sequence_FlashBorderUI?.Kill();
        sequence_FlashBorderUI = DOTween.Sequence();
        sequence_FlashBorderUI
            .AppendCallback(() => ShowCameraBorder())
            .Append(rect_BorderUI.DOScale(Vector3.one, 0).From(Vector3.zero))
            .AppendInterval(0.1f)
            .Append(rect_BorderUI.DOScale(Vector3.zero, 0))
            .AppendInterval(0.1f)
            .Append(rect_BorderUI.DOScale(Vector3.one, 0))
            .Append(rect_BorderUI.DOScale(Vector3.zero, 0))
            .AppendInterval(0.1f)
            .Append(rect_BorderUI.DOScale(Vector3.one, 0));
    }
    public void ShowCameraBorder()
    {
        rect_BorderUI.gameObject.SetActive(true);
    }
    public void HideCameraBorder()
    {
        rect_BorderUI.gameObject.SetActive(false);
    }

    private Sequence sequence_CameraFlash;
    public void CameraFlash()
    {
        sequence_CameraFlash?.Kill();
        sequence_CameraFlash = DOTween.Sequence();
        sequence_CameraFlash
            .AppendCallback(() => ShowCameraFlashImage())
            .Append(rect_CameraFlash.DOScale(new Vector3(6.5f, 6.5f, 6.5f), 0.15f).From(Vector3.zero))
            .AppendCallback(() => pictureMode.PlaySFX_CameraFlash())
            .Append(rect_CameraFlash.DOScale(Vector3.zero, 0))
            .AppendCallback(() => HideCameraFlashImage());
    }
    public void ShowCameraFlashImage()
    {
        rect_CameraFlash.gameObject.SetActive(true);
    }
    public void HideCameraFlashImage()
    {
        rect_CameraFlash.gameObject.SetActive(false);
    }

    private Sequence sequence_ShowPicture;
    public void ShowPicture(Texture2D photo)
    {
        if (picturePreview == null)
        {
            Debug.LogWarning("Photo preview UI not assigned!");
            return;
        }

        HideCameraBorder();
        picturePreview.texture = photo;

        sequence_ShowPicture?.Kill();
        sequence_ShowPicture = DOTween.Sequence();
        sequence_ShowPicture
            .AppendCallback(() => ShowPicturePreview())
            .Append(picturePreview.rectTransform.DOAnchorPosX(0, 0))
            .Append(picturePreview.rectTransform.DOScale(1, 0.2f))
            .AppendInterval(picturePreviewDuration)
            .Append(picturePreview.rectTransform.DOAnchorPosX(40, 0.12f))
            .Append(picturePreview.rectTransform.DOScale(0, 0.2f))
                        .OnComplete(() =>
                        {
                            picturePreview.texture = null;
                            ShowCameraBorder();
                            HidePicturePreview();
                        });
    }
    public void ShowPicturePreview()
    {
        picturePreview.gameObject.SetActive(true);
        picturePreviewActive = true; 
    }
    public void HidePicturePreview()
    {
        picturePreview.gameObject.SetActive(false);
        picturePreviewActive = false; 
    }

    public void ShowZoomUI()
    {
        sequence_HideZoomUI?.Kill();
        rect_ZoomParent.DOAnchorPosY(zoomUIShowYPos, 0.1f);
    }

    private Sequence sequence_HideZoomUI;
    public void HideZoomUI()
    {
        sequence_HideZoomUI?.Kill();
        sequence_HideZoomUI = DOTween.Sequence();
        sequence_HideZoomUI
            .AppendInterval(0.5f)
            .Append(rect_ZoomParent.DOAnchorPosY(zoomUIHideYPos, 0.1f));
    }
}
