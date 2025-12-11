using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerOverclockData : PlayerPassive
{
    [Header("Data")]
    [ReadOnly] public float overclockPercentage;
    [ReadOnly] public int currentReadySegments;

    [Header("Refs")]
    private PlayerComponentManager cM;

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
    }

    public void UpdateOverclockPercentage(float amount)
    { 
        // Check if the percentage was already at maximum (1.0f)
        bool wasMaxedOut = overclockPercentage >= 1f;

        overclockPercentage += amount; //Add to current percentage
        overclockPercentage = Mathf.Clamp(overclockPercentage, 0, 1); //Ensure percentage cannot go over 1 

        currentReadySegments = Mathf.FloorToInt(overclockPercentage * 3); //Convert percentage to threshold for 3 segments
        currentReadySegments = Mathf.Clamp(currentReadySegments, 0, 3); // Clamp to ensure 3 max segments

        // If it was already maxed out before, don't update the bar or segments
        if (wasMaxedOut && overclockPercentage >= 1f)
        {
            return;
        }

        // Update bar and segments
        cM.playerUIOverclock.UpdateBar(overclockPercentage);
        cM.playerUIOverclock.UpdateSegments(currentReadySegments);
    }

    [Button]
    public void DrainOverclockBar(float percentage)
    {
        // Reduce the overclock percentage by the value of one segment
        overclockPercentage -= percentage;
        overclockPercentage = Mathf.Clamp(overclockPercentage, 0, 1); // Ensure it doesn't go below 0

        // Update the number of ready segments
        currentReadySegments--;

        // Update the UI to reflect the changes
        cM.playerUIOverclock.UpdateBar(overclockPercentage);
        cM.playerUIOverclock.UpdateSegments(currentReadySegments);
    }

    public override void ResetPassive()
    {
        base.ResetPassive();

        overclockPercentage = 0;
        currentReadySegments = 0;
    }
}
