using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WaveGenerator : MonoBehaviour
{
    [Header("UpdateRate")]
    public float updateRate; //Higher update rates can get quite heavy so adviced to go 0.03f and up. 
    private float updateRateTimer;

    [Header("Data")]
    const int SpectrumSize = 4096; //8192
    readonly float[] _spectrum = new float[SpectrumSize];
    private List<float> viewSpectrum = new List<float>();
    private Vector3 startPosition;
    private Vector3 endPosition; 

    [Header("Wave Settings")]
    public float bandSize = 1.1f;
    public int smoothAmount = 3;
    public int downSampleAmount = 5;
    public float heightModifier = 0.2f;
    public float minFrequency = 20f;  // Minimum frequency to visualize (e.g., bass tones)
    public float maxFrequency = 500f; // Maximum frequency to visualize (e.g., treble tones)

    [Header("Refs")]
    // Start and end points for the line renderer
    public AudioSource _audioSource;
    public Transform startPoint;
    public Transform endPoint;
    public LineRenderer _topLine;

    private void Update()
    {
        if (startPoint == null || endPoint == null) { return; }
        updateRateTimer += Time.deltaTime;
        if (updateRateTimer >= updateRate)
        {
            updateRateTimer = 0;
            UpdateSpectrumData(); //Every update rate we do a full redraw using new spectrum data.
        }

        startPosition = startPoint.position;
        //Debug.Log("Wave" + startPosition);
        endPosition = endPoint.position;
    }

    private void LateUpdate()
    {
        if (startPoint == null || endPoint == null) { return; }
        SetLinePoints(viewSpectrum, _topLine);
    }

    private void UpdateSpectrumData()
    {
        _audioSource.GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);
        // Frequency range filter
        int sampleRate = AudioSettings.outputSampleRate; // Get the audio sample rate
        float freqResolution = sampleRate / 2f / SpectrumSize; // Frequency resolution per bin
        int minIndex = Mathf.Clamp(Mathf.FloorToInt(minFrequency / freqResolution), 0, SpectrumSize - 1);
        int maxIndex = Mathf.Clamp(Mathf.FloorToInt(maxFrequency / freqResolution), 0, SpectrumSize - 1);

        var crossover = bandSize;
        viewSpectrum = new List<float>();
        var b = 0f;

        for (var i = minIndex; i <= maxIndex; i++)
        {
            var d = _spectrum[i];
            b = Mathf.Max(d, b); // Find the max as the peak value in that frequency band.
            if (i > crossover - 3)
            {
                crossover *= bandSize; // Frequency crossover point for each band.
                viewSpectrum.Add(b);
                b = 0;
            }
        }

        viewSpectrum = Downsample(viewSpectrum, downSampleAmount);
        viewSpectrum = SmoothSpectrum(viewSpectrum, smoothAmount);
        //SetLinePoints(viewSpectrum, _topLine);
    }

    private List<float> SmoothSpectrum(List<float> spectrum, int windowSize)
    {
        List<float> smoothedSpectrum = new List<float>();

        for (int i = 0; i < spectrum.Count; i++)
        {
            float sum = 0f;
            int count = 0;

            for (int j = i - windowSize; j <= i + windowSize; j++)
            {
                if (j >= 0 && j < spectrum.Count)
                {
                    sum += spectrum[j];
                    count++;
                }
            }

            smoothedSpectrum.Add(sum / count);
        }

        return smoothedSpectrum;
    }

    private List<float> Downsample(List<float> spectrum, int factor)
    {
        List<float> downsampledSpectrum = new List<float>();

        for (int i = 0; i < spectrum.Count; i += factor)
        {
            downsampledSpectrum.Add(spectrum[i]);
        }

        return downsampledSpectrum;
    }

    //private List<Vector3> InterpolateLine(List<Vector3> points, int interpolationFactor)
    //{
    //    List<Vector3> interpolatedPoints = new List<Vector3>();

    //    for (int i = 0; i < points.Count - 1; i++)
    //    {
    //        interpolatedPoints.Add(points[i]);

    //        for (int j = 1; j < interpolationFactor; j++)
    //        {
    //            float t = j / (float)interpolationFactor;
    //            Vector3 interpolatedPoint = Vector3.Lerp(points[i], points[i + 1], t);
    //            interpolatedPoints.Add(interpolatedPoint);
    //        }
    //    }

    //    interpolatedPoints.Add(points[points.Count - 1]); // Add the last point
    //    return interpolatedPoints;
    //}

    private void SetLinePoints(List<float> viewSpectrum, LineRenderer lineRenderer, float modifier = 1)
    {
        if (startPoint == null || endPoint == null)
        {
            //Debug.LogWarning("StartPoint or EndPoint is not assigned.");
            return;
        }

        // Calculate the length and direction between start and end points
        Vector3 startPosition = startPoint.position;
        Vector3 endPosition = endPoint.position;
        Vector3 direction = (endPosition - startPosition).normalized;
        float totalDistance = Vector3.Distance(startPosition, endPosition);

        // Distance between points based on the spectrum count
        float pointDistance = totalDistance / viewSpectrum.Count;

        // Set the vertex count and positions
        lineRenderer.positionCount = viewSpectrum.Count;
        for (int i = 0; i < viewSpectrum.Count; i++)
        {
            float t = i / (float)(viewSpectrum.Count - 1);
            Vector3 basePosition = Vector3.Lerp(startPosition, endPosition, t);

            // Calculate the vertical offset and apply rotation
            Vector3 localOffset = new Vector3(0, viewSpectrum[i] * 34 * heightModifier, 0);
            Vector3 rotatedOffset = startPoint.TransformDirection(localOffset); // Respect startPoint rotation

            // Add the rotated offset to the base position
            Vector3 finalPosition = basePosition + rotatedOffset;
            lineRenderer.SetPosition(i, finalPosition);
        }
    }

    public void SetStartAndEnd(Transform start, Transform end)
    {
        startPoint = start;
        endPoint = end; 
    }
}
