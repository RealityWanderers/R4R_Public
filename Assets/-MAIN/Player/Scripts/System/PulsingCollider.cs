using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PulsingCollider : MonoBehaviour
{
    [Header("Timing")]
    public float pulseInterval = 2f;
    public float activeDuration = 0.5f;

    [Header("Target")]
    public Collider targetCollider;

    [Header("Events")]
    public UnityEvent onPulseStart;
    public UnityEvent onPulseEnd;

    private void Start()
    {
        if (targetCollider == null)
            targetCollider = GetComponent<Collider>();

        StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(pulseInterval);

            // Enable collider & invoke start event
            targetCollider.enabled = true;
            onPulseStart?.Invoke();

            yield return new WaitForSeconds(activeDuration);

            // Disable collider & invoke end event
            targetCollider.enabled = false;
            onPulseEnd?.Invoke();
        }
    }
}