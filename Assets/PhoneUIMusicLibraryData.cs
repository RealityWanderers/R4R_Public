using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[RequireComponent(typeof(PhoneUIMusicLibraryAnimation))]
public class PhoneUIMusicLibraryData : MonoBehaviour, IPhonePanel
{
    [Header("Playlists")]
    //public GameObject customPlaylistPrefab;
    public List<MusicPlaylistData> builtInPlaylists = new();
    private List<MusicPlaylistData> customPlaylists = new(); // Runtime generated
    [ShowInInspector] [ReadOnly] public List<MusicPlaylistData> allPlaylists = new();
    public GameObject playlistUIPrefab;
    [ShowInInspector] [ReadOnly] public List<GameObject> instancedPlaylistPrefabs { get; private set; } = new();

    [Header("UI")]
    public GridLayoutGroup gridLayoutGroup;
    public RectTransform rect_GridLayout;
    [SerializeField] private Texture2D defaultPlaylistImage;

    [Header("Data")]
    [SerializeField] [ReadOnly] private int currentIndex;
    [SerializeField] [ReadOnly] private int currentRow;
    [SerializeField] [ReadOnly] private int previousRow;
    [SerializeField] [ReadOnly] private int maxRows;

    [Header("References")]
    private PlayerPhone playerPhone;
    private PlayerMusic playerMusic;
    private PhoneUIMusicLibraryAnimation musicLibraryAnimation;

    public void OnPanelShown()
    {
        Initialize();
        OpenMusicLibrary();
        UpdatePlaylistInfo();
    }

    public void OnPanelHidden()
    {

    }

    private void Initialize()
    {
        playerPhone = PlayerPhone.Instance;
        playerMusic = PlayerMusic.Instance;
        musicLibraryAnimation = GetComponent<PhoneUIMusicLibraryAnimation>();
        LoadAllCustomPlaylists();
    }

    public void OpenMusicLibrary()
    {
        musicLibraryAnimation.ShowPlaylistLoadingScreen();
        FillInGrid();
    }

    //[Button]
    public void NavigateLeft()
    {
        ChangeCurrentIndex(currentIndex - 1);
    }

    //[Button]
    public void NavigateRight()
    {
        ChangeCurrentIndex(currentIndex + 1);
    }

    //[Button]
    public void NavigateUp()
    {
        if (currentRow == 0)
        {
            //Blocked Sound
            return;
        }
        ChangeCurrentIndex(currentIndex - gridLayoutGroup.constraintCount);
    }

    //[Button]
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
        if (!HasAnyPlaylists()) return;

        newIndexValue = Mathf.Clamp(newIndexValue, 0, allPlaylists.Count - 1);
        currentIndex = newIndexValue;

        SetCurrentRow();
        UpdatePlaylistInfo();
        //Debug.Log(currentIndex);
    }

    public void UpdateSelectionIndicator()
    {
        musicLibraryAnimation.UpdateSelectionIndicator(GetCurrentLocalPositionForIndicator());
        musicLibraryAnimation.AnimateSelectionRotation();
        musicLibraryAnimation.AnimateSelectionScale();
        musicLibraryAnimation.AnimateScrollProgress();
        //Debug.Log("Animate at" + GetCurrentPicturePosition());
    }

    private IEnumerator DelayedUpdateSelectionIndicator()
    {
        yield return null; // Wait 1 frame so hierarchy is rebuilt
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect_GridLayout);

        UpdateSelectionIndicator();
    }

    public Vector2 GetCurrentLocalPositionForIndicator()
    {
        if (currentIndex < 0 || currentIndex >= allPlaylists.Count)
        {
            return Vector2.zero;
        }

        RectTransform targetRect = instancedPlaylistPrefabs[currentIndex].GetComponent<RectTransform>();
        RectTransform indicatorParentRect = musicLibraryAnimation.selectionIndicatorParent.parent as RectTransform;

        // Convert the target’s local position to indicatorParent’s local space:
        Vector2 localPoint = indicatorParentRect.InverseTransformPoint(targetRect.position);

        return localPoint;
    }

    public RectTransform GetCurrentPlaylistRect()
    {
        if (instancedPlaylistPrefabs.Count == 0 || currentIndex < 0 || currentIndex >= allPlaylists.Count)
        {
            Debug.LogWarning("Tried to get picture rect, but no valid pictures available!");
            return null;
        }

        return instancedPlaylistPrefabs[currentIndex].GetComponent<RectTransform>();
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

    public void FillInGrid()
    {
        foreach (Transform child in rect_GridLayout)
        {
            Destroy(child.gameObject);
        }

        instancedPlaylistPrefabs.Clear();

        allPlaylists.Clear();
        allPlaylists.AddRange(builtInPlaylists);
        allPlaylists.AddRange(customPlaylists);

        foreach (var playlistData in builtInPlaylists)
        {
            CreatePlaylistUI(playlistData);
        }

        foreach (var customPlaylistData in customPlaylists)
        {
            CreatePlaylistUI(customPlaylistData);
        }

        maxRows = Mathf.CeilToInt(allPlaylists.Count / (float)gridLayoutGroup.constraintCount) - 1;
        maxRows = Mathf.Max(maxRows, 0);
        currentIndex = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, allPlaylists.Count - 1));

        StartCoroutine(DelayedUpdateSelectionIndicator());
        musicLibraryAnimation.HidePlaylistLoadingScreen();

        if (!HasAnyPlaylists())
        {
            // Optionally hide or disable grid/indicator
        }
        else
        {
            musicLibraryAnimation.ShowSelectIndicator();
            musicLibraryAnimation.AnimateSelectionRotation();
            musicLibraryAnimation.AnimateScrollProgress();
            SetCurrentRow();
        }
    }

    void CreatePlaylistUI(MusicPlaylistData playlistData)
    {
        GameObject uiInstance = Instantiate(playlistUIPrefab, rect_GridLayout);
        instancedPlaylistPrefabs.Add(uiInstance);

        var playlistCover = uiInstance.GetComponentInChildren<PlayerPhoneUIPlaylistCoverObject>();
        if (playlistCover != null)
        {
            RawImage imageComponent = playlistCover.GetComponent<RawImage>();
            if (imageComponent != null)
            {
                imageComponent.texture = playlistData.playListImage;
            }
            else
            {
                Debug.LogWarning("PlayerPhoneUIPlaylistCoverObject exists but has no RawImage component.");
            }
        }
        else
        {
            Debug.LogWarning("No cover object found on instantiated playlist UI.");
        }
    }

    public void UpdatePlaylistInfo()
    {
        if (currentIndex < 0 || currentIndex >= allPlaylists.Count)
        {
            Debug.LogWarning("Invalid index for UpdatedPlaylistInfo");
            return;
        }

        MusicPlaylistData playList = allPlaylists[currentIndex];
        musicLibraryAnimation.UpdatePlaylistInfoText(playList.playListName, playList.songs.Count.ToString());
    }

    //[Button]
    public void StartPlaylist()
    {
        if (currentIndex < 0 || currentIndex >= allPlaylists.Count)
        {
            Debug.LogWarning("Invalid index for StartPlaylist");
            return;
        }

        MusicPlaylistData playList = allPlaylists[currentIndex];

        if (currentIndex == 0) //NOTE THIS IS A BIT MESSY BUT INDEX 0 SHOULD ALWAYS BE THE CUSTOM PLAYLIST OBJECT. 
        {
            CreateCustomPlaylistTemplate();
        }
        else
        {
            playerMusic.ChangePlaylist(playList);
            Debug.Log("StartPlaylist - PhoneUI");
        }
    }

    private string customPlaylistsRootFolderName = "MyPlaylists";
    private string customPlaylistTemplate = "CustomPlaylistExample";
    [Button]
    public void CreateCustomPlaylistTemplate()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, customPlaylistsRootFolderName);
        Directory.CreateDirectory(folderPath);

        string newFolderName = customPlaylistTemplate;
        string fullPath = Path.Combine(folderPath, newFolderName);
        int counter = 1;

        while (Directory.Exists(fullPath)) //Will try to increment folder name to ensure you can make as many custom playlists as you want.
        {
            newFolderName = $"{customPlaylistTemplate} {counter}";
            fullPath = Path.Combine(folderPath, newFolderName);
            counter++;
        }

        Directory.CreateDirectory(fullPath);
        WriteDefaultImageAsync(Path.Combine(fullPath, "ReplaceMe.png"), defaultPlaylistImage);
        Debug.Log($"Created new playlist folder: {newFolderName}");
        OpenCurrentPlaylistInExplorer(fullPath);
        musicLibraryAnimation.OpenExplorerSequence();
        LoadAllCustomPlaylists();
    }

    async void WriteDefaultImageAsync(string imagePath, Texture2D texture)
    {
        if (texture == null) return;

        byte[] pngData = texture.EncodeToPNG();
        if (pngData == null) return;

        try
        {
            await Task.Run(() => File.WriteAllBytes(imagePath, pngData));
            Debug.Log($"Image written to {imagePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write image: {e.Message}");
        }
    }

    public void OpenCurrentPlaylistInExplorer(string path)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        string normalizedPath = Path.GetFullPath(path).Replace('/', '\\');
        Debug.Log($"Opening Explorer at: {normalizedPath}");

        if (Directory.Exists(normalizedPath))
        {
            System.Diagnostics.Process.Start("explorer.exe", $"\"{normalizedPath}\"");
        }
        else
        {
            Debug.LogWarning($"Directory does not exist: {normalizedPath}");
        }
#endif
    }

    [Button]
    public void LoadAllCustomPlaylists()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, customPlaylistsRootFolderName);

        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning("No playlist root folder found.");
            return;
        }

        customPlaylists.Clear();

        string[] playlistDirectories = Directory.GetDirectories(folderPath);

        foreach (var dir in playlistDirectories)
        {
            string playlistFolderName = Path.GetFileName(dir);

            // Start a coroutine that adds the playlist only if songs are found
            StartCoroutine(LoadAllAudioClipsIntoPlaylist(folderName: playlistFolderName));
        }

        // Final refresh
        FillInGrid(); // Rebuild grid with updated list
    }

    public IEnumerator LoadAllAudioClipsIntoPlaylist(string folderName)
    {
        string folderPath = Path.Combine(Application.persistentDataPath, customPlaylistsRootFolderName, folderName);
        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning("Folder not found: " + folderPath);
            yield break;
        }

        string[] supportedExtensions = { "*.mp3", "*.wav", "*.ogg" };
        List<string> allFiles = new();

        foreach (string ext in supportedExtensions)
            allFiles.AddRange(Directory.GetFiles(folderPath, ext));

        if (allFiles.Count == 0)
        {
            Debug.Log("No Files found in" + folderPath);
            //yield break; // Could skip loading here...
        }
        
        MusicPlaylistData playlistData = ScriptableObject.CreateInstance<MusicPlaylistData>();
        playlistData.playListName = folderName;
        playlistData.songs = new();

        foreach (string file in allFiles)
        {
            string url = "file://" + file;
            AudioType type = GetAudioTypeFromPath(file);

            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, type);
            DownloadHandlerAudioClip dh = new(url, type) { streamAudio = true };
            www.downloadHandler = dh;

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Error loading clip {file}: {www.error}");
                continue;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            if (clip == null) continue;

            clip.name = Path.GetFileNameWithoutExtension(file);

            playlistData.songs.Add(new MusicPlaylistData.Song
            {
                audioClip = clip,
                filePath = file
            });
        }

        string[] imageFiles = Directory.GetFiles(folderPath, "*.*")
            .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (imageFiles.Length > 0)
        {
            string imagePath = imageFiles[0]; // First image found
            string imageUrl = "file://" + imagePath;

            using UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture(imageUrl);
            yield return imageRequest.SendWebRequest();

            if (imageRequest.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(imageRequest);
                texture.filterMode = FilterMode.Point;
                playlistData.playListImage = texture;
                Debug.Log($"Loaded playlist art for {folderName}");
            }
            else
            {
                Debug.LogWarning($"Failed to load playlist image: {imageRequest.error}");
                playlistData.playListImage = defaultPlaylistImage; // Fallback
            }
        }
        else
        {
            // No image files found
            playlistData.playListImage = defaultPlaylistImage;
        }

        //if (playlistData.songs.Count > 0)
        //{
        //    customPlaylists.Add(playlistData);
        //    FillInGrid(); // Only rebuild grid if something changed
        //    Debug.Log($"Added custom playlist: {playlistData.playListName} with {playlistData.songs.Count} songs.");
        //}

        customPlaylists.Add(playlistData);
        FillInGrid(); // Only rebuild grid if something changed
        Debug.Log($"Added custom playlist: {playlistData.playListName} with {playlistData.songs.Count} songs.");
    }

    private AudioType GetAudioTypeFromPath(string path)
    {
        string ext = Path.GetExtension(path).ToLower();

        switch (ext)
        {
            case ".mp3": return AudioType.MPEG;
            case ".wav": return AudioType.WAV;
            case ".ogg": return AudioType.OGGVORBIS;
            default: return AudioType.UNKNOWN;
        }
    }

    private bool HasAnyPlaylists()
    {
        return allPlaylists != null && allPlaylists.Count > 0;
    }
}
