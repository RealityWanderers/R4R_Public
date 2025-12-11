using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class PlayerRailGrindCornerDetector : PlayerPassive
{
    [Header("Settings")]
    [Range(0.001f, 1f)] public float sampleSpacing = 0.1f; // Smaller values provide more precise results
    public float angleThreshold = 90f; // Angle in degrees to define a "sharp corner"

    [Header("Refs")]
    private PlayerComponentManager cM;
    public List<float> cornerTValues = new List<float>();

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
    }

    public void ClearCornerTValues()
    {
        cornerTValues.Clear();
    }

    public void AnalyzeSpline(Spline spline, bool isMovingForward)
    {
        ClearCornerTValues(); 

        int totalSamples = Mathf.FloorToInt(1f / sampleSpacing);
        Vector3 previousTangent = Vector3.zero;
        bool firstIteration = true;

        // Iterate the spline differently based on movement direction
        for (int i = (isMovingForward ? 0 : totalSamples); (isMovingForward ? i <= totalSamples : i >= 0); i += (isMovingForward ? 1 : -1))
        {
            float t = i * sampleSpacing;
            t = Mathf.Clamp01(t);
            Vector3 tangent = spline.EvaluateTangent(t);
            tangent.Normalize();
            if (!firstIteration)
            {
                float angle = Vector3.Angle(previousTangent, tangent);

                if (angle >= angleThreshold)
                {
                    cornerTValues.Add(t);
                }
            }
            previousTangent = tangent;
            firstIteration = false;
        }
    }
}
