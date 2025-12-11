using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using System.Collections;
using System;

[RequireComponent(typeof(AudioSource))]
public class PlayerMusic : MonoBehaviour
{
    private AudioSource source;

    public static PlayerMusic Instance { get; private set; }

    [Header("Music Playlist Settings")]
    public MusicPlaylistData currentMusicPlaylist;
    [Range(0, 1)] public float volume;
    public bool playOnStart;
    private int currentTrackIndex = 0;

    public enum MusicPlayerState {playing, stopped, paused, fading}
    public MusicPlayerState _MusicPlayerState;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f; // Duration of fade out/in

    [Header("References")]
    public TextMeshProUGUI songNameUIText;

    public event Action OnTrackChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ChangeMusicPlayerState(MusicPlayerState.stopped); 
        source = GetComponent<AudioSource>();
        if (currentMusicPlaylist == null || currentMusicPlaylist.songs.Count == 0)
        {
            Debug.LogWarning("No music playlist assigned!");
            return;
        }
        PreLoadTracks();
        if (playOnStart)
        {
            PlayTrack(currentTrackIndex);
        }
        else
        {
            StopPlaying();
        }
    }

    public void PreLoadTracks()
    {
        StartCoroutine(PreLoadTracksInBackground());
    }

    private IEnumerator PreLoadTracksInBackground()
    {
        foreach (var song in currentMusicPlaylist.songs)
        {
            AudioClip clip = song.audioClip;

            if (clip.loadState != AudioDataLoadState.Loaded)
            {
                // Start async loading, ensure LOAD IN BACKGROUND IS ON INSIDE THE WAV
                clip.LoadAudioData();

                // Wait until it's fully loaded
                while (clip.loadState == AudioDataLoadState.Loading)
                {
                    yield return null; // Wait a frame
                }

                if (clip.loadState != AudioDataLoadState.Loaded)
                {
                    Debug.LogWarning($"Failed to load track: {clip.name}");
                }
                else
                {
                    Debug.Log($"Track loaded: {clip.name}");
                }
            }

            // Optional: throttle between songs to avoid bursts of CPU
            yield return new WaitForSeconds(0.05f);
        }

        PlayTrack(0); //Possible different starting song and could also add shuffling the list here. 

        Debug.Log("All tracks loaded in background!");
    }


    private void Update()
    {
        if (GetMusicPlayerState() == MusicPlayerState.playing)
        {
            if (!source.isPlaying && source.clip != null && !fadeTween.IsActive())
            {
                PlayNextTrack();
                //Debug.Log("PlayNext");
            }
        }
    }

    private Tween fadeTween;
    public void PlayTrack(int index)
    {
        if (currentMusicPlaylist == null || currentMusicPlaylist.songs.Count == 0)
            return;

        if (index < 0 || index >= currentMusicPlaylist.songs.Count)
            return;

        //Debug.Log("IsPlayingNext");
        ChangeMusicPlayerState(MusicPlayerState.playing);
        Debug.Log(_MusicPlayerState); 
        currentTrackIndex = index;
        AudioClip nextClip = currentMusicPlaylist.songs[index].audioClip;
        source.volume = volume;
        songNameUIText.SetText(nextClip.name);

        ChangeMusicPlayerState(MusicPlayerState.fading);
        // Fade out, change track, fade back in
        fadeTween = source.DOFade(0, fadeDuration).OnComplete(() =>
        {
            source.clip = nextClip;
            source.Play();
            source.DOFade(volume, fadeDuration).OnComplete(() =>
            {
                ChangeMusicPlayerState(MusicPlayerState.playing);
                OnTrackChanged?.Invoke();
            });
        });
    }

    [Button]
    public void PlayNextTrack()
    {
        if (currentMusicPlaylist == null || currentMusicPlaylist.songs.Count == 0) { return; }
        currentTrackIndex = (currentTrackIndex + 1) % currentMusicPlaylist.songs.Count;
        //Debug.Log(currentTrackIndex); 
        PlayTrack(currentTrackIndex);
    }

    [Button]
    public void PlayPreviousTrack()
    {
        if (currentMusicPlaylist == null || currentMusicPlaylist.songs.Count == 0) { return; }
        currentTrackIndex = (currentTrackIndex - 1 + currentMusicPlaylist.songs.Count) % currentMusicPlaylist.songs.Count;
        PlayTrack(currentTrackIndex);
    }

    [Button]
    public void StopPlaying(float fadeDuration = 0)
    {
        ChangeMusicPlayerState(MusicPlayerState.stopped);
        source.DOFade(0, fadeDuration).OnComplete(() =>
        {
            source.Stop();
            songNameUIText.SetText("----");
        });
    }

    public void ChangePlaylist(MusicPlaylistData playlist)
    {
        StopPlaying();
        currentMusicPlaylist = playlist;
        PreLoadTracks();
        Debug.Log("StartPlayList - MusicPlayer");
    }

    public void ChangeMusicPlayerState(MusicPlayerState musicPlayerState)
    {
        _MusicPlayerState = musicPlayerState;
    }

    public MusicPlayerState GetMusicPlayerState()
    {
        return _MusicPlayerState; 
    }

    public void PauseMusic()
    {
        ChangeMusicPlayerState(MusicPlayerState.paused); 
        source.Pause();
    }

    public void ResumeMusic()
    {
        if (source.clip == null) { return; }
        ChangeMusicPlayerState(MusicPlayerState.playing);
        source.UnPause();
    }

    public float GetSongProgress()
    {
        if (source == null || source.clip == null || source.clip.length == 0f)
            return 0f;

        return Mathf.Clamp01(source.time / source.clip.length);
    }

    public string GetSongName()
    {
        if (source == null || source.clip == null) { return "No Song Selected"; }
            return source.clip.name;
    }

    public int GetPlaylistSongCount()
    {
        if (currentMusicPlaylist == null) { return 0; }
        return currentMusicPlaylist.songs.Count; 
    }

    public int GetCurrentTrackIndex()
    {
        return currentTrackIndex; 
    }
}