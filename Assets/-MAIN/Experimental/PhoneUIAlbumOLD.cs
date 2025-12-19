using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PhoneUIAlbumOLD : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridSizeX = 3;
    public int gridSizeY = 4;

    [Header("References")]
    public GameObject albumUI;
    public RectTransform gridContainer;
    public RawImage imageSlotPrefab;

    [Header("Scroll")]
    public float scrollCooldown = 0.15f;
    private float scrollTimer = 0f;

    [Header("ScrollAnimation")]
    public Vector2 scrollMinMax = new Vector2(-8.5f, -37.5f);
    public RectTransform scrollIndicatorTransform;

    [Header("ArrowAnimation")]
    public Vector3 punchScaleScrollArrow = new Vector3(1.015f, 1.015f, 1.015f);
    public float punchScaleDurationArrow = 0.2f;
    public Ease punchScaleEaseArrow;
    public RectTransform scrollArrowLeft;
    public RectTransform scrollArrowRight;

    [Header("DeleteCrossAnimation")]
    public Vector3 punchScaleDeleteCross = new Vector3(1.015f, 1.015f, 1.015f);
    public float punchScaleDurationDeleteCross = 0.2f;
    public Ease punchScaleEaseDeleteCross;
    public RectTransform rect_DeleteCross;

    [Header("SelectedPicutre")]
    [ReadOnly] [ShowInInspector] private bool selectedPictureActive;
    public GameObject selectedPictureGameObject;
    public RectTransform rect_SelectedPicture;
    public RawImage selectedPictureRawImage;
    public GameObject selectedPictureBG;

    [Header("SelectedPictureIcons")]
    public GameObject selectedPictureIconsGameObject;
    public RectTransform rect_TrashCan;
    public Vector3 punchScaleTrashCan;
    public float punchScaleTrashCanDuration = 0.05f;

    [Header("No Pictures")]
    public GameObject noPictures;

    [Header("Navigation")]
    public float deadZone = 0.7f;
    [ReadOnly] public int currentNavigationIndex;

    [Header("Data")]
    private List<RawImage> albumSlots = new();
    private List<string> imagePaths = new();
    private int topVisibleIndex = 0;
    private int itemsPerPage => gridSizeX * gridSizeY;

    [Header("References")]
    private PlayerInputManager input;
    private PlayerPhone playerPhone;

    private void Awake()
    {
        input = PlayerInputManager.Instance;
        playerPhone = PlayerPhone.Instance;
    }

    private void Update()
    {
        if (playerPhone.currentPanel != PlayerPhone.PhonePanelType.Album) { return; }
        if (imagePaths.Count == 0) { return; }

        scrollTimer -= Time.deltaTime;
        Vector2 stickInput = new Vector2(input.stickAxis_X_L, input.stickAxis_Y_L);
        if (stickInput.magnitude > deadZone)
        {
            if (scrollTimer <= 0f)
            {
                if (input.stickAxis_Y_L >= 0.5f)
                {
                    if (!selectedPictureActive)
                    {
                        NavigateGridUp();
                        UpdateScrollBar();
                    }
                    else
                    {

                    }
                    scrollTimer = scrollCooldown;


                }
                else if (input.stickAxis_Y_L <= -0.5f)
                {
                    if (!selectedPictureActive)
                    {
                        NavigateGridDown();
                        UpdateScrollBar();
                    }
                    else
                    {

                    }
                    scrollTimer = scrollCooldown;
                }

                if (input.stickAxis_X_L >= 0.5f)
                {
                    if (!selectedPictureActive)
                    {
                        NavigateGridLeft();
                        UpdateScrollBar();
                    }
                    else if ((topVisibleIndex + currentNavigationIndex) > 0)
                    {
                        IncrementIndex(-1);
                        AnimateNextPicture(true);
                        AnimateScrollArrow(scrollArrowLeft);
                    }
                    scrollTimer = scrollCooldown;
                }
                else if (input.stickAxis_X_L <= -0.5f)
                {
                    if (!selectedPictureActive)
                    {
                        NavigateGridRight();
                        UpdateScrollBar();
                    }
                    else if ((topVisibleIndex + currentNavigationIndex) < imagePaths.Count - 1)
                    {
                        IncrementIndex(1);
                        AnimateNextPicture(false);
                        AnimateScrollArrow(scrollArrowRight); 
                    }
                    scrollTimer = scrollCooldown;
                }
            }
        }

        if (input.playerInput.Left.Secondary.WasPerformedThisFrame())
        {
            if (selectedPictureActive)
            {
                selectedPictureActive = false;
                HideSelectedPicture();
            }
            else
            {
                selectedPictureActive = true;
                ShowSelectedPicture();
            }
        }

        if (input.playerInput.Left.Primary.WasPerformedThisFrame())
        {
            if (selectedPictureActive)
            {
                DeletePictureSequence();
            }
        }
    }

    public void IncrementIndex(int value)
    {
        currentNavigationIndex += value;
        ShowSelectedPicture();
    }

    private Tween tween_punchScaleArrow;
    public void AnimateScrollArrow(RectTransform arrow)
    {
        tween_punchScaleArrow?.Kill();
        arrow.localScale = Vector3.one;
        tween_punchScaleArrow = arrow.DOPunchScale(punchScaleScrollArrow, punchScaleDurationArrow).SetEase(punchScaleEaseArrow);
    }

    public void UpdateScrollBar()
    {
        int absoluteIndex = topVisibleIndex + currentNavigationIndex;
        int currentRow = absoluteIndex / gridSizeX;
        //Debug.Log("CurrentRow: " + currentRow);

        int totalRows = Mathf.CeilToInt((float)imagePaths.Count / gridSizeX);
        //Debug.Log("TotalRows: " + totalRows);

        float rowProgress = (totalRows > 1)
            ? (float)currentRow / (totalRows - 1)
            : 0f;
        //Debug.Log("RowProgress: " + rowProgress);

        float yPos = Mathf.Lerp(scrollMinMax.x, scrollMinMax.y, rowProgress);
        scrollIndicatorTransform.DOAnchorPos(new Vector2(scrollIndicatorTransform.anchoredPosition.x, yPos), 0.1f, true);
    }

    public Sequence sequence_AnimateActivePicture;
    [Button]
    public void AnimateNextPicture(bool leftScroll)
    {
        sequence_AnimateActivePicture?.Kill();
        sequence_AnimateActivePicture = DOTween.Sequence();

        float dir = leftScroll ? 1 : -1;

        sequence_AnimateActivePicture
            .Append(rect_SelectedPicture.DOAnchorPosX(0, 0f, true))
            .Append(rect_SelectedPicture.DOAnchorPosX(40 * dir, 0.1f, true))
            .Append(rect_SelectedPicture.DOAnchorPosX(-40 * dir, 0f, true))
            .Append(rect_SelectedPicture.DOAnchorPosX(0, 0.1f, true));
    }

    [Button]
    public void NavigateGridUp()
    {
        int nextIndex = currentNavigationIndex - gridSizeX;

        if (nextIndex >= 0)
        {
            currentNavigationIndex = nextIndex;
            MoveNavigationCursor(true);
        }
        else
        {
            if (topVisibleIndex >= gridSizeX)
            {
                topVisibleIndex -= gridSizeX; // Scroll up one row
                StartCoroutine(ShowGridWindowCoroutine());
                MoveNavigationCursor(true);
            }
            else
            {
                MoveNavigationCursor(false); // No more to scroll
            }
        }
    }

    [Button]
    public void NavigateGridDown()
    {
        //Debug.Log("NavigateDown");

        int nextIndex = currentNavigationIndex + gridSizeX;
        int absoluteNextIndex = topVisibleIndex + nextIndex;

        // Simple move down within current grid
        if (nextIndex < albumSlots.Count && absoluteNextIndex < imagePaths.Count)
        {
            currentNavigationIndex = nextIndex;
            MoveNavigationCursor(true);
            //Debug.Log("Moved down inside current grid");
        }
        else
        {
            int nextRowStartIndex = topVisibleIndex + gridSizeX * gridSizeY;

            // Check if ANY image exists in the next row
            bool hasImageInNextRow = false;
            for (int i = 0; i < gridSizeX; i++)
            {
                int checkIndex = nextRowStartIndex + i;
                if (checkIndex < imagePaths.Count)
                {
                    hasImageInNextRow = true;
                    break;
                }
            }

            if (hasImageInNextRow)
            {
                topVisibleIndex += gridSizeX;
                StartCoroutine(ShowGridWindowCoroutine());

                int localX = currentNavigationIndex % gridSizeX;
                int targetAbsoluteIndex = topVisibleIndex + (gridSizeX * (gridSizeY - 1)) + localX;
                int maxAbsoluteIndex = imagePaths.Count - 1;

                if (targetAbsoluteIndex <= maxAbsoluteIndex)
                {
                    currentNavigationIndex = gridSizeX * (gridSizeY - 1) + localX;
                }
                else
                {
                    // Clamp to last valid image slot in visible grid
                    int lastVisibleSlot = Mathf.Min(albumSlots.Count, imagePaths.Count - topVisibleIndex) - 1;
                    currentNavigationIndex = lastVisibleSlot;
                }


                MoveNavigationCursor(true);
                //Debug.Log($"Scrolled down and clamped to index {currentNavigationIndex}");
            }
            else
            {
                // No more rows below — stay put if already in the last visible row
                int lastVisibleSlot = Mathf.Min(albumSlots.Count, imagePaths.Count - topVisibleIndex) - 1;

                int currentRow = currentNavigationIndex / gridSizeX;
                int lastRow = lastVisibleSlot / gridSizeX;

                if (currentRow < lastRow)
                {
                    // You're above the last row, so we can safely move down to the last row
                    int localX = currentNavigationIndex % gridSizeX;
                    int targetIndex = Mathf.Min(lastVisibleSlot, lastRow * gridSizeX + localX);

                    currentNavigationIndex = targetIndex;
                    MoveNavigationCursor(true);
                    //Debug.Log($"Moved to last row at index {currentNavigationIndex}");
                }
                else
                {
                    // You're already in the last row — don't move
                    MoveNavigationCursor(false);
                    //Debug.Log("Already at last row, can't move further");
                }
            }
        }
    }

    [Button]
    public void NavigateGridLeft()
    {
        int absoluteCurrentIndex = topVisibleIndex + currentNavigationIndex;

        // If we're already at the first image, don't move left
        if (absoluteCurrentIndex <= 0)
        {
            MoveNavigationCursor(false);
            //Debug.Log("Already at first image, cannot move left");
            return;
        }

        // If we can just move left inside current visible grid
        if (currentNavigationIndex > 0)
        {
            currentNavigationIndex--;
            MoveNavigationCursor(true);
            //Debug.Log("Moved left within grid");
        }
        else
        {
            // Try to scroll up one row if possible
            int prevTopVisibleIndex = topVisibleIndex - gridSizeX;

            if (prevTopVisibleIndex >= 0)
            {
                topVisibleIndex = prevTopVisibleIndex;
                StartCoroutine(ShowGridWindowCoroutine());

                // Jump cursor to top-right slot of the new visible grid
                int targetIndex = gridSizeX - 1;  // top-right column in the first row of grid

                // Clamp to remaining images in case top row isn't full
                int remainingImages = imagePaths.Count - topVisibleIndex;
                currentNavigationIndex = Mathf.Min(targetIndex, remainingImages - 1);

                MoveNavigationCursor(true);
                //Debug.Log($"Scrolled up, jumping to top-right index {currentNavigationIndex}");
            }
            else
            {
                MoveNavigationCursor(false);
                //Debug.Log("No more rows to scroll up to");
            }
        }
    }

    [Button]
    public void NavigateGridRight()
    {
        int absoluteCurrentIndex = topVisibleIndex + currentNavigationIndex;

        // If we're already on the last image, don't move right
        if (absoluteCurrentIndex >= imagePaths.Count - 1)
        {
            MoveNavigationCursor(false);
            //Debug.Log("Already at last image, cannot move right");
            return;
        }

        int nextIndex = currentNavigationIndex + 1;
        int absoluteNextIndex = topVisibleIndex + nextIndex;

        // Move right inside current visible grid if valid image exists
        if (nextIndex < albumSlots.Count && absoluteNextIndex < imagePaths.Count)
        {
            currentNavigationIndex = nextIndex;
            MoveNavigationCursor(true);
            //Debug.Log("Next");
        }
        else
        {
            int nextTopVisibleIndex = topVisibleIndex + gridSizeX;

            // Check if we can scroll down one full row
            if (nextTopVisibleIndex < imagePaths.Count)
            {
                // Scroll down one row
                topVisibleIndex = nextTopVisibleIndex;
                StartCoroutine(ShowGridWindowCoroutine());

                // Jump cursor to first slot of last visible row
                currentNavigationIndex = gridSizeX * (gridSizeY - 1);

                // Clamp currentNavigationIndex if last row is incomplete
                int remainingImages = imagePaths.Count - topVisibleIndex;
                currentNavigationIndex = Mathf.Min(currentNavigationIndex, remainingImages - 1);

                MoveNavigationCursor(true);
                //Debug.Log($"Scrolled down, jumping to index {currentNavigationIndex}");
            }
            else
            {
                MoveNavigationCursor(false);
                //Debug.Log("No more rows to scroll down to or move right");
            }
        }
    }

    private Sequence sequence_HighlightRotation;
    public void MoveNavigationCursor(bool validMovement)
    {
        if (imagePaths.Count == 0) { return; }

        if (validMovement)
        {
            //Plays normal sound
        }
        else
        {
            //Plays more blocky bumpy sound
        }

        sequence_HighlightRotation?.Kill();
        HideNavigationCursor();

        foreach (var item in albumSlots)
        {
            item.rectTransform.DOScale(Vector3.one, 0.05f);
            item.rectTransform.localRotation = Quaternion.identity;
        }

        RawImage selectedImage = albumSlots[currentNavigationIndex];
        RawImage highlightBorder = GetFirstChildRawImage(selectedImage.gameObject);
        selectedImage.rectTransform.DOScale(1.1f, 0.05f);
        highlightBorder.gameObject.SetActive(true);

        sequence_HighlightRotation = DOTween.Sequence();

        RectTransform target = selectedImage.rectTransform;

        sequence_HighlightRotation.Append(target.DOLocalRotate(new Vector3(0, 0, 5), 0.1f))
            .AppendInterval(0.22f)
            .Append(target.DOLocalRotate(new Vector3(0, 0, -5), 0.1f))
            .AppendInterval(0.22f)
            .SetLoops(-1, LoopType.Restart);
    }

    public void HideNavigationCursor()
    {
        foreach (var item in albumSlots)
        {
            GetFirstChildRawImage(item.gameObject).gameObject.SetActive(false);
        }
    }

    RawImage GetFirstChildRawImage(GameObject obj)
    {
        return obj.GetComponentsInChildren<RawImage>(true)
                  .FirstOrDefault(r => r.transform != obj.transform);
    }

    private void OnEnable()
    {
        OpenAlbum();
    }

    void GenerateGrid()
    {
        foreach (Transform child in gridContainer)
            Destroy(child.gameObject);
        albumSlots.Clear();

        // Create new grid
        int totalSlots = itemsPerPage;
        for (int i = 0; i < totalSlots; i++)
        {
            RawImage slot = Instantiate(imageSlotPrefab, gridContainer);
            albumSlots.Add(slot);
        }

        GridLayoutGroup layout = gridContainer.GetComponent<GridLayoutGroup>();
        if (layout != null)
        {
            float spacing = layout.spacing.x;
            float width = gridContainer.rect.width;
            float height = gridContainer.rect.height;

            Vector2 cellSize = new(
                (width - spacing * (gridSizeX - 1)) / gridSizeX,
                (height - spacing * (gridSizeY - 1)) / gridSizeY
            );
            layout.cellSize = cellSize;
        }
    }

    void LoadAllPhotoPaths()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "MyPhotos");
        if (!Directory.Exists(folderPath)) return;

        imagePaths = new List<string>(Directory.GetFiles(folderPath, "*.png"));
        imagePaths.Sort();
    }

    private IEnumerator ShowGridWindowCoroutine()
    {
        for (int i = 0; i < albumSlots.Count; i++)
        {
            int imageIndex = topVisibleIndex + i;
            if (imageIndex < imagePaths.Count)
            {
                string path = imagePaths[imageIndex];
                if (!textureCache.TryGetValue(path, out Texture2D tex))
                {
                    byte[] fileData = File.ReadAllBytes(path);
                    tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    tex.LoadImage(fileData);
                    tex.filterMode = FilterMode.Point;
                    textureCache[path] = tex;
                }

                RawImage slot = albumSlots[i];
                slot.enabled = true;
                slot.texture = tex;
                float delay = i * 0.02f;
                slot.rectTransform.DOScale(Vector3.one, 0.2f).SetDelay(delay);

                yield return new WaitForSeconds(0.05f);
            }
            else
            {
                RawImage slot = albumSlots[i];
                slot.texture = null;
                slot.enabled = false;
                slot.rectTransform.localScale = Vector3.one;
            }
        }

        yield return null;
    }

    [Button]
    public void ScrollDownOneRow()
    {
        int maxStartIndex = imagePaths.Count - itemsPerPage;
        int rowOffset = gridSizeX;

        if (topVisibleIndex + rowOffset <= maxStartIndex)
        {
            topVisibleIndex += rowOffset;
            StartCoroutine(ShowGridWindowCoroutine());
        }
    }

    [Button]
    public void ScrollUpOneRow()
    {
        int rowOffset = gridSizeX;

        if (topVisibleIndex - rowOffset >= 0)
        {
            topVisibleIndex -= rowOffset;
            StartCoroutine(ShowGridWindowCoroutine());
        }
    }

    private Dictionary<string, Texture2D> textureCache = new();
    private IEnumerator PreloadAllPhotosCoroutine()
    {
        int loadedCount = 0;

        for (int i = 0; i < imagePaths.Count; i++)
        {
            string path = imagePaths[i];

            if (!textureCache.ContainsKey(path))
            {
                byte[] fileData = File.ReadAllBytes(path);

                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.LoadImage(fileData);
                tex.filterMode = FilterMode.Point;
                textureCache[path] = tex;

                loadedCount++;
            }

            if (loadedCount % 3 == 0) // Every 3 images, yield to keep things smooth
                yield return null;
        }

        // Once preloaded, show the first grid page
        StartCoroutine(ShowGridWindowCoroutine());
        MoveNavigationCursor(true);
    }

    [Button]
    public void ShowSelectedPicture()
    {
        int absoluteIndex = topVisibleIndex + currentNavigationIndex;

        if (absoluteIndex >= 0 && absoluteIndex < imagePaths.Count)
        {
            string path = imagePaths[absoluteIndex];

            if (!textureCache.TryGetValue(path, out Texture2D tex))
            {
                byte[] fileData = File.ReadAllBytes(path);
                tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.LoadImage(fileData);
                tex.filterMode = FilterMode.Point;
                textureCache[path] = tex;
            }

            selectedPictureRawImage.texture = tex;
            selectedPictureBG.SetActive(true);
            selectedPictureGameObject.SetActive(true);
            selectedPictureIconsGameObject.SetActive(true);
            rect_SelectedPicture.DOScale(Vector3.one, 0.1f).From(Vector3.zero);
            rect_DeleteCross.localScale = Vector3.zero; 
            rect_DeleteCross.gameObject.SetActive(true);
        }
    }

    public void HideSelectedPicture()
    {
        selectedPictureBG.SetActive(false);
        selectedPictureGameObject.SetActive(false);
        selectedPictureIconsGameObject.SetActive(false);
        rect_DeleteCross.gameObject.SetActive(false);
        selectedPictureRawImage.texture = null;

        int absoluteIndex = topVisibleIndex + currentNavigationIndex;

        // Snap grid to include this image
        // Snap to nearest row while clamping to max valid starting index
        int maxTopVisibleIndex = Mathf.Max(0, imagePaths.Count - itemsPerPage);
        topVisibleIndex = Mathf.Min((absoluteIndex / gridSizeX) * gridSizeX, maxTopVisibleIndex);

        currentNavigationIndex = absoluteIndex - topVisibleIndex;

        StartCoroutine(ShowGridWindowCoroutine());
        MoveNavigationCursor(true);
        UpdateScrollBar(); 
    }

    [Button]
    public void OpenAlbum()
    {
        LoadAllPhotoPaths();
        GenerateGrid();
        albumUI.SetActive(true);
        ResetAllAlbumSlotsVisibility();
        HideSelectedPicture();
        StartCoroutine(PreloadAllPhotosCoroutine());

        if (imagePaths.Count == 0)
        {
            currentNavigationIndex = 0;
            noPictures.SetActive(true);
        }
        else
        {
            noPictures.SetActive(false);
        }
        //Debug.Break(); 
    }

    private void ResetAllAlbumSlotsVisibility()
    {
        foreach (var slot in albumSlots)
        {
            slot.rectTransform.localScale = Vector3.one;
            slot.enabled = false;
        }
    }

    private Sequence sequence_DeletePicture;
    [Button]
    public void DeletePictureSequence()
    {
        sequence_DeletePicture?.Kill();
        sequence_DeletePicture = DOTween.Sequence();
        sequence_DeletePicture
            .Append(rect_TrashCan.DOScale(punchScaleTrashCan, punchScaleTrashCanDuration * 0.5f).From(Vector3.one))
            .AppendInterval(punchScaleTrashCanDuration * 0.5f)
            .Append(rect_TrashCan.DOScale(Vector3.one, punchScaleTrashCanDuration * 0.5f))
            .Append(rect_DeleteCross.DOScale(Vector3.one, 0.05f).From(Vector3.zero))
            .Append(rect_DeleteCross.DOPunchScale(punchScaleDeleteCross, punchScaleDurationDeleteCross, 1, 0).SetEase(punchScaleEaseDeleteCross))
            .Append(rect_DeleteCross.DOScale(Vector3.zero, 0.05f))
            .Append(rect_SelectedPicture.DOScale(Vector3.zero, 0.1f))
            .OnComplete(() =>
            {
                DeletePicture();
            });
    }

    public void DeletePicture()
    {
        int absoluteIndex = topVisibleIndex + currentNavigationIndex;

        if (absoluteIndex >= 0 && absoluteIndex < imagePaths.Count)
        {
            string path = imagePaths[absoluteIndex];

            // Step 1: Delete from disk
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            // Step 2: Remove from cache
            if (textureCache.ContainsKey(path))
            {
                textureCache.Remove(path);
            }

            // Step 3: Remove from list
            imagePaths.RemoveAt(absoluteIndex);

            // Step 4: Decide what to display next
            int totalImages = imagePaths.Count;

            if (totalImages == 0)
            {
                // No images left — close viewer
                selectedPictureActive = false;
                HideSelectedPicture();
                noPictures.SetActive(true);
                HideNavigationCursor();
            }
            else
            {
                // Adjust index after deletion
                if (absoluteIndex >= totalImages)
                {
                    // Deleted last image, move to previous
                    absoluteIndex = totalImages - 1;
                }

                // Set topVisibleIndex and currentNavigationIndex accordingly
                topVisibleIndex = Mathf.Max(0, absoluteIndex - (absoluteIndex % itemsPerPage));
                currentNavigationIndex = absoluteIndex - topVisibleIndex;
                ShowSelectedPicture();
            }
        }
    }

    [Button]
    public void CloseAlbum()
    {
        albumUI.SetActive(false);
    }
}
