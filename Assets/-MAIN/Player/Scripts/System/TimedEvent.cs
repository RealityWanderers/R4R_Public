using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class TimedEvent : MonoBehaviour
{
    [Header("Settings")]
    public float timerDuration;
    [ReadOnly] public bool isTimerRunning; 
    private float timeStamp; 

    public UnityEvent onTimerStart;
    public UnityEvent onTimerEnd;

    public void StartTimer()
    {
        isTimerRunning = true; 
        onTimerStart.Invoke();
        timeStamp = Time.time; 
    }

    public void EndTimer()
    {
        isTimerRunning = false; 
        onTimerEnd.Invoke();
    }

    void Update()
    {
        if (isTimerRunning && Time.time > timeStamp + timerDuration)
        {
            EndTimer(); 
        }
    }
}
