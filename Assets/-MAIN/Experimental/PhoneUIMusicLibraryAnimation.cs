using DG.Tweening;
using UnityEngine;
using TMPro; 

[RequireComponent(typeof(PhoneUIMusicLibraryData))]
public class PhoneUIMusicLibraryAnimation : MonoBehaviour
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

    [Header("Animation-Scroll")]
    public RectTransform rect_ScrollIndicator;
    public Vector2 scrollIndicatorMinMax = new(21, -15f);
    public float scrollIndicatorMoveTime = 0.05f;

    [Header("Animation-Loading")]
    public RectTransform rect_AlbumLoadingParent;
    public RectTransform rect_AlbumLoadingSpinner;
    public float albumLoadingSpinnerSpeed = 0.5f;

    [Header("Animation-AlbumInfo")]
    public TextMeshProUGUI text_AlbumName;
    public TextMeshProUGUI text_AlbumSongCount;

    [Header("Animation-Notifications")]
    public RectTransform rect_NotificationExplorer;
    //public RectTransform rect_OpenExplorer;
    //public Vector3 punchScaleOpenExplorer = new(1.2f, 1.2f, 1.2f);
    //public float punchScaleOpenExplorerDuration = 0.25f;

    [Header("References")]
    private PhoneUIMusicLibraryData musicLibraryData;

    private void Awake()
    {
        musicLibraryData = GetComponent<PhoneUIMusicLibraryData>();
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        //sequence_HighlightRotation?.Kill();
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
        foreach (var picture in musicLibraryData.instancedPlaylistPrefabs)
        {
            picture.GetComponent<RectTransform>().localScale = Vector3.one;
        }
        RectTransform target = musicLibraryData.GetCurrentPlaylistRect();

        if (target == null) { return; }

        tween_SelectionScale = target.DOScale(1.15f, 0.1f);
    }

    private Sequence sequence_HighlightRotation;
    public void AnimateSelectionRotation()
    {
        sequence_HighlightRotation?.Kill();
        sequence_HighlightRotation = DOTween.Sequence();
        foreach (var picture in musicLibraryData.instancedPlaylistPrefabs)
        {
            picture.GetComponent<RectTransform>().localRotation = Quaternion.identity;
        }
        RectTransform target = musicLibraryData.GetCurrentPlaylistRect();

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
        float scrollProgress = musicLibraryData.GetScrollProgress();
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
    public void ShowPlaylistLoadingScreen()
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
    public void HidePlaylistLoadingScreen()
    {
        //Debug.Log("Closing");
        sequence_HideLoadingScreen?.Kill();
        sequence_HideLoadingScreen = DOTween.Sequence();
        sequence_HideLoadingScreen
            //.AppendCallback(() => albumSelectedPicture.ClosePicture())
            .AppendInterval(.35f) //Small minimum time that it always shows the loading screen.
            .Append(rect_AlbumLoadingParent.DOScale(Vector3.zero, 0.15f).From(Vector3.one))
            .AppendCallback(() => rect_AlbumLoadingParent.gameObject.SetActive(false))
            .AppendCallback(() => tween_LoadingSpinner?.Kill());
    }



    public void UpdatePlaylistInfoText(string albumName, string songCount)
    {
        text_AlbumName.SetText(albumName);
        text_AlbumSongCount.SetText(songCount); 
    }



    public Sequence sequence_OpenExplorer { get; private set; }
    public void OpenExplorerSequence()
    {
        sequence_OpenExplorer?.Kill();
        sequence_OpenExplorer = DOTween.Sequence();

        sequence_OpenExplorer
            //.Append(rect_OpenExplorer.DOScale(punchScaleOpenExplorer, punchScaleOpenExplorerDuration * 0.5f).From(Vector3.one))
            //.Append(rect_OpenExplorer.DOScale(Vector3.one, punchScaleOpenExplorerDuration * 0.5f))
            //.AppendCallback(() => musicLibraryData.OpenCurrentPlaylistInExplorer())
            .AppendCallback(() => rect_NotificationExplorer.gameObject.SetActive(true))
            .Append(rect_NotificationExplorer.DOScale(Vector3.one, 0.3f).From(Vector3.zero))
            .AppendInterval(1f)
            .Append(rect_NotificationExplorer.DOScale(Vector3.zero, 0.3f))
            .AppendCallback(() => rect_NotificationExplorer.gameObject.SetActive(false));
    }
}
