using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using TMPro;
using System;

public class SetRefreshRate : MonoBehaviour
{
    [Header("Settings")]
    public bool onStart;

    [Header("Data")]
    [ReadOnly] public float adjustedTimeStep;
    [ReadOnly] public float[] refreshRates;

    [Header("Refs")]
    public TextMeshProUGUI debugText;

    private void Start()
    {
        if (onStart)
        {
            TrySetRefreshRate();
        }
    }

    [Button]
    void TrySetRefreshRate()
    {
        refreshRates = OVRPlugin.systemDisplayFrequenciesAvailable;
        if (refreshRates.Length == 0)
        {
            if (debugText != null) { debugText.SetText("No VR Display found / using PCVR"); }
            adjustedTimeStep = Mathf.Round((1f / 120f) * 10000f) / 10000f; //Defaults to a timestep matching 120HZ display. 
            Time.fixedDeltaTime = 0.0083f;
            return;
        }
        else
        {
            string result = "RefreshRates: ";
            foreach (var item in refreshRates)
            {
                result += item.ToString() + ", ";
            }
            if (debugText != null) { debugText.SetText(result); }
            //Debug.Log("Two");
            float highestRefreshRate = refreshRates.Max();
            OVRPlugin.systemDisplayFrequency = highestRefreshRate;
            adjustedTimeStep = Mathf.Round((1f / highestRefreshRate) * 10000f) / 10000f;
            Time.fixedDeltaTime = adjustedTimeStep;
        }
    }
}
