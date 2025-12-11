using UnityEngine;
using UnityEngine.Events;

public class OnEnableDisable : MonoBehaviour
{
    public UnityEvent onEnableEvent;
    public UnityEvent onDisableEvent;

    private void OnEnable()
    {
        onEnableEvent?.Invoke();
    }

    private void OnDisable()
    {
        onDisableEvent?.Invoke();
    }
}
