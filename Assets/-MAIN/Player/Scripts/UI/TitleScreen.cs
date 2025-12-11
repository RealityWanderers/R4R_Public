using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(AudioSource))]
public class TitleScreen : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip clip_Intro;
    public AudioClip clip_BGMusic;

    [Header("Settings")]
    public bool skipTitleScreen = true;
    public float titleStartDelay;
    [Range(0, 1)] public float volume_Intro;
    [Range(0, 1)] public float volume_BG;

    [Header("State")]
    [ReadOnly] public bool hasPlayedIntro;
    [ReadOnly] public bool startTitleScreen;

    [Header("TitleAnimation")]
    public List<RectTransform> titleAnimationFrames;

    public float titleIntroAnimationEndDelay = 1.5f;
    public float delayBetweenAnimations = 0.2f;
    public float titleIntroScaleMulti = 1.3f;
    public float titleIntroScaleDuration = 0.2f;
    public Ease titleIntroEaseType;

    [Header("Refs")]
    private AudioSource audioSource;
    public PlayerUICalibration uICalibration;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        clip_Intro.LoadAudioData();
        clip_BGMusic.LoadAudioData();
        StopTitleAnimation();
        if (skipTitleScreen)
        {
            Invoke(nameof(SkipTitleScreen), titleStartDelay);
        }
        else
        {
            Invoke(nameof(StartTitleScreen), titleStartDelay);
        }
    }

    public void Update()
    {
        if (startTitleScreen)
        {
            if (!audioSource.isPlaying)
            {
                if (!hasPlayedIntro)
                {
                    PlayIntroClip();
                }
                else
                {
                    PlayBackgroundMusic();
                }
            }
        }
    }

    [Button]
    public void StartTitleScreen()
    {
        startTitleScreen = true;
        uICalibration.LoadFirstPanel();
        StartTitleAnimation();
    }

    public void SkipTitleScreen()
    { 
        uICalibration.LoadSecondPanel();
    }

    public void PlayIntroClip()
    {
        hasPlayedIntro = true;
        audioSource.clip = clip_Intro;
        audioSource.volume = volume_Intro;
        audioSource.loop = false;
        audioSource.Play();
        //Debug.Log("PlayIntro");
    }

    public void PlayBackgroundMusic()
    {
        audioSource.clip = clip_BGMusic;
        audioSource.volume = volume_BG;
        audioSource.loop = true;
        audioSource.Play();
        //Debug.Log("PlayBG");
    }

    [Button]
    public void ResetTitleScreen()
    {
        hasPlayedIntro = false;
        startTitleScreen = false;
        audioSource.loop = false;
        audioSource.Stop();
        StopTitleAnimation();
        //uICalibration.LoadFirstPanel();
    }

    private Tween tween_TitleAnimation;
    public void StartTitleAnimation()
    {
        if (tween_TitleAnimation == null)
        {
            Sequence sequence = DOTween.Sequence();
            tween_TitleAnimation = sequence;

            foreach (RectTransform frame in titleAnimationFrames)
            {
                sequence.Append(frame.DOScale(Vector3.one * titleIntroScaleMulti, titleIntroScaleDuration).SetEase(titleIntroEaseType))
                        .Append(frame.DOScale(Vector3.one, titleIntroScaleDuration).SetEase(titleIntroEaseType))
                        .AppendInterval(delayBetweenAnimations);
            }
            sequence.AppendInterval(titleIntroAnimationEndDelay);

            //Needed as Join without extra sequence causes it to start together with the Interval call rather than waiting on it to finish. 
            Sequence fadeOutSequence = DOTween.Sequence();
            foreach (RectTransform frame in titleAnimationFrames)
            {
                fadeOutSequence.Join(frame.DOScale(Vector3.zero, 0.1f).SetEase(Ease.Linear));
            }

            //Append the fade-out sequence to ensure it runs AFTER the delay
            sequence.Append(fadeOutSequence);

            sequence.OnComplete(() => uICalibration.NextPanel());

        }
    }

    public void StopTitleAnimation()
    {
        if (tween_TitleAnimation != null)
        {
            tween_TitleAnimation.Kill();
            tween_TitleAnimation = null;
        }

        HideTitle();
    }

    public void HideTitle()
    {
        foreach (var title in titleAnimationFrames)
        {
            title.localScale = Vector3.zero;
        }
    }
}
