using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class OnPlayerEnterArea : MonoBehaviour
{
    public UnityEvent onEnterEvent;
    public UnityEvent onLeaveEvent;

    private void OnEnter()
    {
        //Debug.Log("Enter");
    }

    private void OnLeave()
    {
        //Debug.Log("Leave");
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerColliderObject>() != null)
        {
            OnEnter(); 
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerColliderObject>() != null)
        {
            OnLeave();
        }
    }
}
