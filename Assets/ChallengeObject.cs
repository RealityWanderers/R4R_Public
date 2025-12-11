using Sirenix.OdinInspector;
using UnityEngine;

public class ChallengeObject : MonoBehaviour
{
    private ChallengeManager challengeManager;

    private void Awake()
    {
        challengeManager = GetComponentInParent<ChallengeManager>();
    }

    [Button]
    public void CompleteChallengeObjective()
    {
        challengeManager?.MarkChallengeObjectiveComplete(this);
    }
}