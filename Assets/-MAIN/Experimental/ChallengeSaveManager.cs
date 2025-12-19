using System.Collections.Generic;
using System.Linq;
using UnityEngine; 

public class ChallengeSaveManager : MonoBehaviour
{
    public static ChallengeSaveManager Instance { get; private set; }
    private const string ChallengeDataKey = "ChallengeSaveData";

    public List<ChallengeSaveEntry> savedEntries = new List<ChallengeSaveEntry>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveChallengeData(string challengeID, float time, ChallengeSaveEntry.Rank rank)
    {
        var existing = savedEntries.Find(e => e.challengeID == challengeID);

        if (existing == null)
        {
            // First time saving this challenge — always add
            savedEntries.Add(new ChallengeSaveEntry
            {
                challengeID = challengeID,
                bestTime = time,
                bestRank = rank
            });
        }
        else
        {
            // Only update if the time or rank is better
            bool isNewTimeBetter = time < existing.bestTime;
            bool isNewRankBetter = rank > existing.bestRank;

            if (!isNewTimeBetter && !isNewRankBetter)
                return; // No improvements, skip saving

            if (isNewTimeBetter) existing.bestTime = time;
            if (isNewRankBetter) existing.bestRank = rank;
        }

        string json = JsonUtility.ToJson(new Wrapper { entries = savedEntries });
        PlayerPrefs.SetString(ChallengeDataKey, json);
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        if (PlayerPrefs.HasKey(ChallengeDataKey))
        {
            string json = PlayerPrefs.GetString(ChallengeDataKey);
            try
            {
                Wrapper wrapper = JsonUtility.FromJson<Wrapper>(json);
                savedEntries = wrapper?.entries ?? new List<ChallengeSaveEntry>();
            }
            catch
            {
                Debug.LogWarning("Failed to parse ChallengeSaveData JSON.");
                savedEntries = new List<ChallengeSaveEntry>();
            }
        }
        else
        {
            savedEntries = new List<ChallengeSaveEntry>();
        }
    }

    [ContextMenu("Clear Challenge Save Data")]
    public void ClearData()
    {
        PlayerPrefs.DeleteKey(ChallengeDataKey);
        savedEntries.Clear();
        Debug.Log("Challenge save data cleared.");
    }

    [System.Serializable]
    private class Wrapper
    {
        public List<ChallengeSaveEntry> entries;
    }
}