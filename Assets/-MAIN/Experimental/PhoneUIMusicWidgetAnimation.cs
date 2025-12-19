using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhoneUIMusicWidgetAnimation : MonoBehaviour
{
    [Header("Buttons")]
    public GameObject button_Pause;
    public GameObject button_Resume;

    [Header("Playlist Cover")]
    public RawImage image_PlaylistCover;

    [Header("Playlist SongCount")]
    public TextMeshProUGUI rect_SongCount;

    [Header("Song Name")]
    public TextMeshProUGUI text_SongName;

    [Header("Song Progress")]
    public RectTransform rect_ScrollIndicator;
    public Vector2 scrollIndicatorMinMax = new(21, -15f);
    public float scrollIndicatorMoveTime = 0.05f;

    public void UpdatePlaylistCover(Texture2D playlistImage)
    {
        image_PlaylistCover.texture = playlistImage;
    }

    public void UpdateSongInfoText(string songName, string songCount)
    {
        text_SongName.SetText(songName);
        rect_SongCount.SetText(songCount); 
    }

    public void UpdateSongProgress(float progress)
    {
        float scrollPos = Mathf.Lerp(scrollIndicatorMinMax.x, scrollIndicatorMinMax.y, progress);
        rect_ScrollIndicator.DOAnchorPosX(scrollPos, scrollIndicatorMoveTime);
    }

    public void UpdatePauseResumeButton(bool switchToResume)
    {
        button_Pause.SetActive(!switchToResume);
        button_Resume.SetActive(switchToResume);
    }
}
