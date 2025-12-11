using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChallengeManager : MonoBehaviour
{
    [ReadOnly, LabelText("Challenge ID"), InfoBox("Auto-generated from the GameObject name")]
    public string challengeID;
    [SerializeField] private ChallengeSaveEntry.ChallengeType challengeType;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Only update if ID is empty 
        if (string.IsNullOrEmpty(challengeID))
        {
            challengeID = $"{gameObject.scene.name}_{gameObject.name}";
            EditorUtility.SetDirty(this); // Ensure it saves in the editor
        }

        PopulateChallengeList();
    }
#endif

    [Button("Refresh ID")]
    private void RefreshID()
    {
        challengeID = $"{gameObject.scene.name}_{gameObject.name}";
        EditorUtility.SetDirty(this); // Ensure it saves in the editor
    }

    [System.Serializable]
    public class ChallengeObjectivesList
    {
        public bool completed;
        public ChallengeObject challengeObject;
    }

    public List<ChallengeObjectivesList> challengeObjectives;

    [System.Serializable]
    public class RankBracket
    {
        public ChallengeSaveEntry.Rank rank;
        public float maxTime; // Max time to earn this rank
    }

    [Header("Ranks")]
    [TableList] public List<RankBracket> rankBrackets = new List<RankBracket>();

    [Header("State")]
    [ReadOnly] public bool isChallengeStarted;

    [Header("Timer")]
    [ReadOnly] public float timer;
    [ReadOnly] public bool timerStarted;

    [Header("Location")]
    public Transform startTransform;

    [Header("References")]
    private PlayerTeleporter teleporter;
    private PlayerPhone playerPhone;
    private PlayerChallengeUI challengeUI;
    private PlayerChallengeSystem challengeSystem;
    private PlayerRamenDeliverySystem deliverySystem; 

    private void Awake()
    {
        teleporter = PlayerTeleporter.Instance;
        playerPhone = PlayerPhone.Instance;
        challengeUI = PlayerChallengeUI.Instance;
        challengeSystem = PlayerChallengeSystem.Instance;
        deliverySystem = PlayerRamenDeliverySystem.Instance; 
    }

    private void Start()
    {
        ResetChallenge();
    }

    [Button("Populate Challenge List")]
    public void PopulateChallengeList()
    {
        challengeObjectives = new List<ChallengeObjectivesList>();

        ChallengeObject[] allChallenges = GetComponentsInChildren<ChallengeObject>(true);

        foreach (var challenge in allChallenges)
        {
            ChallengeObjectivesList newEntry = new ChallengeObjectivesList
            {
                completed = false,
                challengeObject = challenge
            };

            challengeObjectives.Add(newEntry);
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public void Update()
    {
        if (timerStarted)
        {
            timer += Time.deltaTime;
            //challengeUI.UpdateTimerText(timer);
        }

        if (GetChallengeCompletionPercent() == 1 && isChallengeStarted)
        {
            OnChallengeComplete();
        }
    }

    [Button]
    public void SaveChallengeData(float time)
    {
        ChallengeSaveManager.Instance.SaveChallengeData(challengeID, time, GetRankForTime(time));
    }

    public ChallengeSaveEntry.Rank GetRankForTime(float time)
    {
        // Sort brackets just in case they aren't already sorted
        rankBrackets.Sort((a, b) => a.maxTime.CompareTo(b.maxTime));

        foreach (var bracket in rankBrackets)
        {
            if (time <= bracket.maxTime)
            {
                if (bracket.rank == ChallengeSaveEntry.Rank.S)
                {
                    if (challengeType == ChallengeSaveEntry.ChallengeType.Delivery)
                    {
                        //If thrown boxes == ramendeliverpoints
                        if (deliverySystem.thrownBoxesAmount == challengeObjectives.Count)
                        {
                            return ChallengeSaveEntry.Rank.S_Plus;
                        }
                       
                    }
                }
                return bracket.rank;
            }
        }

        // If time is worse than all brackets, default to D rank
        return ChallengeSaveEntry.Rank.D;
    }

    public void MarkChallengeObjectiveComplete(ChallengeObject obj)
    {
        var entry = challengeObjectives.FirstOrDefault(c => c.challengeObject == obj);
        if (entry != null && !entry.completed)
        {
            entry.completed = true;
            UpdateChallengeProgressText();
        }
    }

    public void MarkAllChallengeObjectivesUnCompleted()
    {
        foreach (var challengeObjective in challengeObjectives)
        {
            if (challengeObjective != null)
            {
                challengeObjective.completed = false;
            }
        }
    }

    public void UpdateChallengeProgressText()
    {
        challengeUI.UpdateChallengeProgressText(GetChallengeCompletedAmount(), challengeObjectives.Count);
    }

    public void StartTimer()
    {
        ResetTimer();
        timerStarted = true;
    }

    public void ResetTimer()
    {
        timerStarted = false;
        timer = 0;
    }

    public void ResetProgress()
    {
        MarkAllChallengeObjectivesUnCompleted();
    }

    public float GetChallengeCompletionPercent()
    {
        int completedCount = challengeObjectives.Count(c => c.completed);
        return (float)completedCount / challengeObjectives.Count;
    }

    public int GetChallengeCompletedAmount()
    {
        int completedCount = challengeObjectives.Count(c => c.completed);
        return completedCount;
    }

    [Button]
    public void OnChallengeStart()
    {
        ResetChallenge();

        challengeSystem.CancelAllChallenges();

        UnsubscribeFromCountdown();
        SubscribeToCountdown();

        challengeUI.StartCountDown();
        isChallengeStarted = true;

        EnableChallengeObjectives();
        teleporter.TeleportToTransform(startTransform);
        playerPhone.DeActivatePhone();
        challengeUI.ShowProgressText(true);
    }

    [Button]
    public void OnChallengeComplete()
    {
        SaveChallengeData(timer);
        challengeUI.SetRank(GetRankForTime(timer));
        challengeUI.ShowResults(timer);
        ResetChallenge();
        deliverySystem.ResetDeliverySystemState(); 
    }

    public void OnChallengeCancel()
    {
        ResetChallenge();
    }

    public void ResetObjectiveCompletion()
    {
        foreach (var objective in challengeObjectives)
        {
            objective.completed = false;
        }
    }

    public void EnableChallengeObjectives()
    {
        for (int i = 0; i < challengeObjectives.Count; i++)
        {
            challengeObjectives[i].challengeObject.gameObject.SetActive(true);
        }
    }

    public void DisableChallengeObjectives()
    {
        for (int i = 0; i < challengeObjectives.Count; i++)
        {
            challengeObjectives[i].challengeObject.gameObject.SetActive(false);
        }
    }

    [Button]
    public void ResetChallenge()
    {
        challengeUI.CancelCountdown();
        ResetTimer();
        ResetProgress();
        ResetObjectiveCompletion();
        challengeUI.ShowProgressText(false);
        //challengeUI.ShowTimerText(false);
        DisableChallengeObjectives();
        isChallengeStarted = false;
        UnsubscribeFromCountdown();
    }

    private void OnDestroy()
    {
        UnsubscribeFromCountdown();
    }

    private void SubscribeToCountdown()
    {
        challengeUI.OnCountdownComplete += StartTimer;
        challengeUI.OnCountdownComplete += UpdateChallengeProgressText;
    }

    private void UnsubscribeFromCountdown()
    {
        challengeUI.OnCountdownComplete -= StartTimer;
        challengeUI.OnCountdownComplete -= UpdateChallengeProgressText;
    }
}
