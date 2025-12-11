using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class OnStartEvent : MonoBehaviour
{
    public UnityEvent onStartEvent;

    private void Start()
    {
        onStartEvent.Invoke(); 
    }

    [Button]
    private void DebugInvokeTriggerEnter()
    {
        onStartEvent.Invoke();
    }
}
