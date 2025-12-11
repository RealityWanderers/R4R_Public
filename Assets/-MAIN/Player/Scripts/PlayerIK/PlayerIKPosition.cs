using UnityEngine;

public class PlayerIKPosition : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // The HMD or head transform
    public Vector3 offset;   // Offset in local space

    private void LateUpdate()
    {
        SetPosition();
    }

    private void SetPosition()
    {
        if (target == null) return;

        // Offset should be applied relative to the rig's current rotation
        Vector3 targetPosition = target.position + transform.rotation * offset;
        transform.position = targetPosition; 
    }
}