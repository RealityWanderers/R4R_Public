using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System.Collections;

public class OnTimerEvent : MonoBehaviour
{
    public UnityEvent onTimerElapsed;
    private Coroutine timerRoutine;

    [Header("Settings")]    
    public float timerDuration = 1f;
    public bool startOnAwake = true;

    void Start()
    {
        if (startOnAwake)
        {
            StartTimer();
        }
    }

    public void StartTimer()
    {
        if (timerRoutine == null)
            timerRoutine = StartCoroutine(TimerLoop());
    }

    public void StopTimer()
    {
        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }
    }

    private IEnumerator TimerLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(timerDuration);
            onTimerElapsed?.Invoke();
        }
    }
}