using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerChallengeUI : MonoBehaviour
{
    public static PlayerChallengeUI Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }

    [Header("Countdown")]
    public Vector2 delayMinMax = new(2, 2.5f);
    public TextMeshProUGUI text_Ready;
    public Vector3 punchScale_Ready = new(1.2f, 1.2f, 1.2f);
    public float punchScaleDuration_Ready = 0.1f;
    public TextMeshProUGUI text_Go;
    public Vector3 punchScale_Go = new(1.2f, 1.2f, 1.2f);
    public float punchScaleDuration_Go = 0.1f;
    [Space]
    public AudioSource source_Countdown;
    public AudioClip clip_Ready;
    public AudioClip clip_Go;

    [Header("ChallengeProgress")]
    public TextMeshProUGUI text_ChallengeProgress;

    //[Header("Timer")]
    //public TextMeshProUGUI text_Timer;

    [Header("ChallengeResult Rank")]
    public List<ChallengeRankImage> rankImages;
    [System.Serializable]
    public class ChallengeRankImage
    {
        public ChallengeSaveEntry.Rank rank;
        public GameObject rankImage;
    }

    [Header("Results-Timer")]
    public float resultTimerScaleInTime = 0.1f;
    public Ease resultTimerScaleInEaseType = Ease.InBack;
    public Ease resultTimerCountUpEaseType = Ease.InBack;
    public float resultTimerCountUpTime = 1f; 
    [Header("Results-Rank")]
    public float resultRankScaleInTime = 0.1f;
    public Ease resultRankScaleInEaseType = Ease.InBack;
    [Header("Results-Dissapear")]
    public float resultsDisappearDelay; 
    public float resultScaleOutTime = 0.1f;
    public Ease resultScaleOutEaseType = Ease.InBack;

    public TextMeshProUGUI text_ResultTimer;

    public void Start()
    {
        HideAllElements(); 
    }

    public void HideAllElements()
    {
        text_Ready.rectTransform.gameObject.SetActive(false);
        text_Go.rectTransform.gameObject.SetActive(false);
        text_ResultTimer.rectTransform.gameObject.SetActive(false);
        text_ChallengeProgress.rectTransform.gameObject.SetActive(false);
        HideRanks(); 
    }

    //public void UpdateTimerText(float time)
    //{
    //    int minutes = Mathf.FloorToInt(time / 60F);
    //    int seconds = Mathf.FloorToInt(time % 60F);
    //    float fraction = time % 1F;
    //    string formattedTime = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, Mathf.FloorToInt(fraction * 100));
    //    text_Timer.SetText(formattedTime);
    //}

    public void UpdateChallengeProgressText(int challengeCompletedAmount, int challengeObjectiveCount)
    {
        text_ChallengeProgress.SetText(challengeCompletedAmount.ToString() + "/" + challengeObjectiveCount.ToString());
    }

    private Sequence sequence_Countdown;
    public event Action OnCountdownComplete;
    [Button]
    public void StartCountDown()
    {
        sequence_Countdown?.Kill();
        sequence_Countdown = DOTween.Sequence();
        sequence_Countdown.AppendInterval(0.15f);
        sequence_Countdown.AppendCallback(() => { text_Ready.rectTransform.gameObject.SetActive(true); });
        sequence_Countdown.Append(text_Ready.rectTransform.DOScale(Vector3.one, 0.1f).From(Vector3.zero));
        sequence_Countdown.Append(text_Ready.rectTransform.DOPunchScale(punchScale_Ready, punchScaleDuration_Ready));
        sequence_Countdown.AppendCallback(() => PlayCountdownSFX(clip_Ready));
        sequence_Countdown.AppendInterval(UnityEngine.Random.Range(delayMinMax.x, delayMinMax.y));
        sequence_Countdown.Append(text_Ready.rectTransform.DOScale(Vector3.zero, 0.1f));
        sequence_Countdown.AppendCallback(() => { text_Go.rectTransform.gameObject.SetActive(true); });
        sequence_Countdown.Append(text_Go.rectTransform.DOScale(Vector3.one, 0.1f).From(Vector3.zero));
        sequence_Countdown.Append(text_Go.rectTransform.DOPunchScale(punchScale_Go, punchScaleDuration_Go));
        sequence_Countdown.AppendCallback(() => PlayCountdownSFX(clip_Go));
        //sequence_Countdown.AppendCallback(() => ShowTimerText(true));
        sequence_Countdown.AppendCallback(() => ShowProgressText(true));
        sequence_Countdown.AppendCallback(() => OnCountdownComplete?.Invoke());
        sequence_Countdown.AppendInterval(1.45f);
        sequence_Countdown.Append(text_Go.rectTransform.DOScale(Vector3.zero, 0.1f));
    }

    public void CancelCountdown()
    {
        sequence_Countdown?.Kill();
        text_Ready.rectTransform.localScale = Vector3.zero;
        text_Go.rectTransform.localScale = Vector3.zero;
    }

    public void PlayCountdownSFX(AudioClip clip)
    {
        source_Countdown.PlayOneShot(clip, 0.3f);
    }

    //public void ShowTimerText(bool state)
    //{
    //    text_Timer.SetText("");
    //    text_Timer.gameObject.SetActive(state);
    //}

    public void ShowProgressText(bool state)
    {
        text_ChallengeProgress.SetText("");
        text_ChallengeProgress.gameObject.SetActive(state);
    }

    public void SetRank(ChallengeSaveEntry.Rank type)
    {
        foreach (var image in rankImages)
        {
            bool isActive = image.rank == type;
            image.rankImage.SetActive(isActive);

            if (isActive)
            {
                image.rankImage.transform.localScale = Vector3.zero;
            }
        }
    }

    public void HideRanks()
    {
        foreach (var image in rankImages)
        {
            image.rankImage.SetActive(false);
        }
    }

    private Sequence sequence_Results; 
    [Button]
    public void ShowResults(float timerResult)
    {
        sequence_Results?.Kill(); 

        float currentTime = 0;
        text_ResultTimer.text = FormatTime(currentTime);

        sequence_Results = DOTween.Sequence();

        sequence_Results.AppendCallback(() => { text_ResultTimer.rectTransform.gameObject.SetActive(true); });
        sequence_Results.Append(text_ResultTimer.transform.DOScale(Vector3.one, resultTimerScaleInTime).From(Vector3.zero).SetEase(resultTimerScaleInEaseType));

        sequence_Results.Append(DOTween.To(() => currentTime, x =>
        {
            currentTime = x;
            text_ResultTimer.text = FormatTime(currentTime);
        }, timerResult, resultTimerCountUpTime).SetEase(resultTimerCountUpEaseType));

        sequence_Results.Append(text_ResultTimer.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 6, 1f));

        sequence_Results.AppendInterval(0.3f);

        GameObject activeRank = rankImages.Find(r => r.rankImage.activeSelf)?.rankImage;

        sequence_Results.AppendCallback(() =>
        {
            if (activeRank != null)
            {
                activeRank.transform.localScale = Vector3.zero;
                sequence_Results.AppendCallback(() => { activeRank.SetActive(true); });
                sequence_Results.Append(activeRank.transform.DOScale(Vector3.one, resultRankScaleInTime).From(Vector3.zero).SetEase(resultRankScaleInEaseType));
            }
        });

        sequence_Results.AppendInterval(resultsDisappearDelay);

        sequence_Results.Append(text_ResultTimer.transform.DOScale(Vector3.zero, resultScaleOutTime).SetEase(resultScaleOutEaseType));

        if (activeRank != null)
        {
            sequence_Results.Append(activeRank.transform.DOScale(Vector3.zero, resultScaleOutTime).SetEase(resultScaleOutEaseType));
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time % 1f) * 100f);
        return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }
}
