using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(PhoneUIMusicWidgetAnimation))]
public class PhoneUIMusicWidgetData : MonoBehaviour, IPhonePanel
{
    [Header("TrackSkip")]
    public float doubleTapThreshold = 0.25f;
    private float lastTapTime = -Mathf.Infinity;
    private int tapCount = 0;

    [Header("References")]
    private PlayerMusic playerMusic;
    private PlayerInputManager input;
    private PhoneUIMusicWidgetAnimation widgetAnimation;

    public void OnPanelShown()
    {
        Initialize(); 
        playerMusic.OnTrackChanged += UpdateAllWidgetUI;
        UpdateAllWidgetUI(); //Manually update in case a song was already playing. 
    }

    public void Initialize()
    {
        playerMusic = PlayerMusic.Instance;
        input = PlayerInputManager.Instance;
        widgetAnimation = GetComponent<PhoneUIMusicWidgetAnimation>(); 
    }

    public void OnPanelHidden()
    {
        lastTapTime = -Mathf.Infinity;
        playerMusic.OnTrackChanged -= UpdateAllWidgetUI;
    }

    public void Update()
    {
        if (PlayerPhone.Instance.currentPanel != PlayerPhone.PhonePanelType.Home) { return; }

        //"Y" button = Pause/Resume
        if (input.playerInput.Left.Secondary.WasPerformedThisFrame())
        {
            if (playerMusic.GetMusicPlayerState() == PlayerMusic.MusicPlayerState.paused)
                ResumeMusic();
            else
                PauseMusic();
        }

        //"X" button = Tap to go next / double-tap to go previous
        if (input.playerInput.Left.Primary.WasPerformedThisFrame())
        {
            float currentTime = Time.time;

            if (currentTime - lastTapTime <= doubleTapThreshold)
            {
                tapCount++;

                if (tapCount == 2)
                {
                    PreviousSong();
                    tapCount = 0; // Reset after double tap
                    lastTapTime = -Mathf.Infinity;
                }
            }
            else
            {
                tapCount = 1;
                lastTapTime = currentTime;
                Invoke(nameof(HandleSingleTap), doubleTapThreshold); // Wait to see if double tap happens
            }
        }

        UpdateSongProgress();
    }

    private void HandleSingleTap()
    {
        // If we only got one tap, go to next song
        if (tapCount == 1)
        {
            NextSong();
        }
        tapCount = 0;
    }

    [Button]
    public void PauseMusic()
    {
        if (playerMusic.GetMusicPlayerState() != PlayerMusic.MusicPlayerState.playing) { return; }
        playerMusic.PauseMusic();
        UpdateButtonVisualState(); 
    }

    [Button]
    public void ResumeMusic()
    {
        if (playerMusic.GetMusicPlayerState() != PlayerMusic.MusicPlayerState.paused) { return; }
        playerMusic.ResumeMusic();
        UpdateButtonVisualState(); 
    }

    public void UpdateButtonVisualState()
    {
        bool isPaused = playerMusic.GetMusicPlayerState() == PlayerMusic.MusicPlayerState.paused;
        widgetAnimation.UpdatePauseResumeButton(isPaused);
    }


    [Button]
    public void NextSong()
    {
        widgetAnimation.UpdatePauseResumeButton(false);
        playerMusic.PlayNextTrack();
    }
    [Button]
    public void PreviousSong()
    {
        widgetAnimation.UpdatePauseResumeButton(false);
        playerMusic.PlayPreviousTrack();
    }


    public void UpdateAllWidgetUI()
    {
        UpdatePlaylistCover();
        UpdateSongInfo();
        UpdateSongProgress();
        widgetAnimation.UpdatePauseResumeButton(false);
    }

    public void UpdatePlaylistCover()
    {
        if (playerMusic.currentMusicPlaylist == null) { return; }
        widgetAnimation.UpdatePlaylistCover(playerMusic.currentMusicPlaylist.playListImage);
    }

    public void UpdateSongInfo()
    {
        string playlistSongCount = playerMusic.GetPlaylistSongCount().ToString();
        string currentSongIndex; 
        if (playerMusic.GetPlaylistSongCount() == 0)
        {
            currentSongIndex = 0.ToString(); 
        }
        else
        {
            currentSongIndex = (playerMusic.GetCurrentTrackIndex() + 1).ToString();
        }
        
        string playlistProgress = currentSongIndex + "/" + playlistSongCount;
        widgetAnimation.UpdateSongInfoText(playerMusic.GetSongName(), playlistProgress);
    }

    public void UpdateSongProgress()
    {
        widgetAnimation.UpdateSongProgress(playerMusic.GetSongProgress());
    }
}
