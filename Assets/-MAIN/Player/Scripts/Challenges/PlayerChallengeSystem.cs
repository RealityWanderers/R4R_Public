using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class PlayerChallengeSystem : MonoBehaviour
{
    [System.Serializable]
    public class Challenge
    {
        [ReadOnly] public ChallengeManager challengeManager; 
    }

    public List<Challenge> challenges; 

    public static PlayerChallengeSystem Instance { get; private set; }
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

    public void OnValidate()
    {
        GetAllChallenges(); 
    }

    [Button]
    public void GetAllChallenges()
    {
        challenges.Clear();

        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out ChallengeManager challengeManager))
            {
                Challenge challenge = new Challenge
                {
                    challengeManager = challengeManager
                };

                challenges.Add(challenge);
            }
        }
    }

    [Button]
    public void CancelAllChallenges()
    {
        foreach (var challenge in challenges)
        {
            challenge.challengeManager.ResetChallenge(); 
        }
    }
}
