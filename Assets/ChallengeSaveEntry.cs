using UnityEngine;
using System;

[Serializable]
public class ChallengeSaveEntry
{
    public string challengeID;
    public float bestTime;

    public enum Rank { None, D, C, B, A, S, S_Plus}
    public Rank bestRank = Rank.None;

    public enum ChallengeType { Delivery, Misc}
    public ChallengeType challengeType = ChallengeType.Delivery;
}