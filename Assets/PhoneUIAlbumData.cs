using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(PhoneUIAlbumAnimation))]
[RequireComponent(typeof(PhoneUIAlbumSelectedPicture))]
public class PhoneUIAlbumData : MonoBehaviour
{
    [Header("ImageReferences")]
    public GameObject picturePrefab;
    public GridLayoutGroup gridLayoutGroup;
    public RectTransform rect_GridLayout;

    [Header("Images")]
    private Dictionary<string, Texture2D> pictures = new();
    public List<string> imagePaths { get; private set; } = new();
    public List<GameObject> instancedPictures { get; private set; } = new();

    [Header("Data")]
    [SerializeField] [ReadOnly] private int currentIndex;
    [SerializeField] [ReadOnly] private int currentRow;
    [SerializeField] [ReadOnly] private int previousRow;
    [SerializeField] [ReadOnly] private int maxRows;

    //public event System.Action OnPicturesUpdated; //Selected picture calls this to update the grid

    private PhoneUIAlbumAnimation albumAnimation;
    private PlayerPhone playerPhone;
    private PhoneUIAlbumSelectedPicture selectedPicture;

    private void Awake()
    {
        albumAnimation = GetComponent<PhoneUIAlbumAnimation>();
        selectedPicture = GetComponent<PhoneUIAlbumSelectedPicture>();
        playerPhone = PlayerPhone.Instance;
    }

    private void OnEnable()
    {
        if (playerPhone.currentPanel == PlayerPhone.PhonePanelType.Album) //Extra safety check otherwise errors can come from unity starting up the scene.
        {
            OpenAlbum();
        }
    }

    public void NavigateLeft()
    {
        ChangeCurrentIndex(currentIndex - 1);
    }

    public void NavigateRight()
    {
        ChangeCurrentIndex(currentIndex + 1);
    }

    public void NavigateUp()
    {
        if (currentRow == 0)
        {
            //Blocked Sound
            return;
        }
        ChangeCurrentIndex(currentIndex - gridLayoutGroup.constraintCount);
    }

    public void NavigateDown()
    {
        if (currentRow == maxRows)
        {
            //Blocked Sound
            return;
        }
        ChangeCurrentIndex(currentIndex + gridLayoutGroup.constraintCount);
    }

    private void ChangeCurrentIndex(int newIndexValue)
    {
        if (imagePaths.Count == 0) return;

        newIndexValue = Mathf.Clamp(newIndexValue, 0, imagePaths.Count - 1);
        currentIndex = newIndexValue;

        SetCurrentRow();
        //Debug.Log(currentIndex);
    }

    public void UpdateSelectionIndicator()
    {
        albumAnimation.UpdateSelectionIndicator(GetCurrentPictureLocalPositionForIndicator());
        albumAnimation.AnimateSelectionRotation();
        albumAnimation.AnimateSelectionScale();
        albumAnimation.AnimateScrollProgress();
        //Debug.Log("Animate at" + GetCurrentPicturePosition());
    }

    private IEnumerator DelayedUpdateSelectionIndicator()
    {
        yield return null; // Wait 1 frame so hierarchy is rebuilt
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect_GridLayout);

        UpdateSelectionIndicator();
    }

    public Vector2 GetCurrentPictureLocalPositionForIndicator()
    {
        if (instancedPictures.Count == 0
            || currentIndex < 0
            || currentIndex >= instancedPictures.Count)
        {
            return Vector2.zero;
        }

        RectTransform targetRect = instancedPictures[currentIndex].GetComponent<RectTransform>();
        RectTransform indicatorParentRect = albumAnimation.selectionIndicatorParent.parent as RectTransform;

        // Convert the target’s local position to indicatorParent’s local space:
        Vector2 localPoint = indicatorParentRect.InverseTransformPoint(targetRect.position);

        return localPoint;
    }

    public float GetScrollProgress()
    {
        if (maxRows == 0) return 0f; // Safety check to avoid division by zero
        return (float)currentRow / maxRows; //Have to add (float) otherwise always returns 0 as we are dividing two ints. 
    }

    public void SetCurrentRow()
    {
        currentRow = Mathf.FloorToInt(currentIndex / (float)gridLayoutGroup.constraintCount);

        if (currentRow != previousRow) // Only scroll if we changed rows!
        {
            bool down = currentRow > previousRow;

            ScrollGrid(down);
            //Debug.Log((down ? "ScrollDown" : "ScrollUp") + " | CurrentRow: " + currentRow + " PreviousRow: " + previousRow);

            previousRow = currentRow; // Only update after scroll
        }
        else
        {
            //Debug.Log("No Scroll | CurrentRow: " + currentRow + " PreviousRow: " + previousRow);
            UpdateSelectionIndicator(); //Immediately update the indicator, otherwise we update it after a succesfull scroll. 
        }
    }

    private Tween tween_ScrollGrid;
    public void ScrollGrid(bool down)
    {
        float gridYSize = gridLayoutGroup.cellSize.y;
        float gridSpacing = gridLayoutGroup.spacing.y;
        float rowHeight = gridYSize + gridSpacing;

        // Clamp row value just in case.
        currentRow = Mathf.Clamp(currentRow, 0, maxRows);

        // Absolute Y position based on currentRow.
        float targetYPos = rowHeight * currentRow;

        tween_ScrollGrid?.Kill();
        tween_ScrollGrid = rect_GridLayout.DOAnchorPosY(targetYPos, 0.1f).OnComplete(() =>
        {
            UpdateSelectionIndicator();
        });
    }

    public void OpenAlbum()
    {
        albumAnimation.ShowAlbumLoadingScreen();
        LoadAllPhotoPaths();
        StartCoroutine(PreloadAllPhotosCoroutine());
    }

    private void LoadAllPhotoPaths()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "MyPhotos");
        if (!Directory.Exists(folderPath)) return;

        imagePaths = new List<string>(Directory.GetFiles(folderPath, "*.png"));
        imagePaths.Sort();
    }

    private IEnumerator LoadTextureAsync(string path)
    {
        using (UnityEngine.Networking.UnityWebRequest uwr = UnityEngine.Networking.UnityWebRequestTexture.GetTexture("file://" + path))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Texture2D tex = UnityEngine.Networking.DownloadHandlerTexture.GetContent(uwr);
                tex.filterMode = FilterMode.Point;
                pictures[path] = tex;
            }
            else
            {
                Debug.LogWarning($"Failed to load texture: {path} | Error: {uwr.error}");
            }
        }
    }

    private IEnumerator PreloadAllPhotosCoroutine()
    {
        int loadedCount = 0;

        for (int i = 0; i < imagePaths.Count; i++)
        {
            string path = imagePaths[i];

            if (!pictures.ContainsKey(path))
            {
                yield return StartCoroutine(LoadTextureAsync(path));
                loadedCount++;
            }

            //// Optional yield every few loads to give Unity some time
            //if (loadedCount % 3 == 0)
            //{
            //    yield return null;
            //}
        }

        FillInGrid();
    }

    public void FillInGrid()
    {
        foreach (Transform child in rect_GridLayout)
        {
            Destroy(child.gameObject);
        }

        instancedPictures.Clear();

        for (int i = 0; i < imagePaths.Count; i++)
        {
            string path = imagePaths[i];
            GameObject instancedObject = Instantiate(picturePrefab, rect_GridLayout);
            instancedPictures.Add(instancedObject);
            RawImage instancedRawImage = instancedObject.GetComponent<RawImage>();
            instancedRawImage.texture = pictures[path];
        }

        maxRows = Mathf.CeilToInt(imagePaths.Count / (float)gridLayoutGroup.constraintCount) - 1;
        maxRows = Mathf.Max(maxRows, 0); // Safety clamp just in case no pictures

        //Clamp AGAIN just in case imagePaths.Count changed during load
        currentIndex = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, imagePaths.Count - 1));

        StartCoroutine(DelayedUpdateSelectionIndicator());
        albumAnimation.HideAlbumLoadingScreen();

        if (imagePaths.Count == 0)
        {
            albumAnimation.ShowNoPicturesIcon();
            albumAnimation.HideSelectIndicator();
            selectedPicture.ClosePicture();
        }
        else
        {
            albumAnimation.HideNoPicturesIcon();
            albumAnimation.ShowSelectIndicator();
            albumAnimation.AnimateSelectionRotation();
            albumAnimation.AnimateScrollProgress();
            selectedPicture.RefreshPicture(); 

            SetCurrentRow();
        }
    }

    public Texture GetCurrentPictureTexture()
    {
        return pictures[imagePaths[currentIndex]];
    }

    public RectTransform GetCurrentPictureRect()
    {
        if (instancedPictures.Count == 0 || currentIndex < 0 || currentIndex >= instancedPictures.Count)
        {
            Debug.LogWarning("Tried to get picture rect, but no valid pictures available!");
            return null;
        }

        return instancedPictures[currentIndex].GetComponent<RectTransform>();
    }

    public void DeletePicture()
    {
        if (imagePaths.Count == 0) return;

        int index = currentIndex;
        string path = imagePaths[index];

        if (File.Exists(path))
            File.Delete(path);

        pictures.Clear();
        LoadAllPhotoPaths();

        currentIndex = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, imagePaths.Count - 1));

        //selectedPicture.ClosePicture();

        if (imagePaths.Count == 0)
        {
            currentIndex = 0;
            albumAnimation.ShowNoPicturesIcon();
            FillInGrid();
            return;
        }

        StartCoroutine(PreloadAllPhotosCoroutine());
        Debug.Log("Delete");
    }

    public void OpenCurrentPictureInExplorer()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (imagePaths.Count == 0) return;

        string currentFile = Path.GetFullPath(imagePaths[currentIndex]).Replace('/', '\\');
        string parentFolder = Path.GetDirectoryName(currentFile);

        Debug.Log($"Trying to open Explorer for file: {currentFile}");

        if (!File.Exists(currentFile))
        {
            Debug.LogWarning($"File does not exist: {currentFile}");
            System.Diagnostics.Process.Start("explorer.exe", $"\"{parentFolder}\"");
            return;
        }

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"/select,\"{currentFile}\"",
            UseShellExecute = true
        };

        try
        {
            System.Diagnostics.Process.Start(psi);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Explorer failed with /select, opening folder instead. Exception: {ex.Message}");
            System.Diagnostics.Process.Start("explorer.exe", $"\"{parentFolder}\"");
        }
#endif
    }
}