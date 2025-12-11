using Sirenix.OdinInspector;
using UnityEngine;

public class ModularSetToTransform : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 offset;

    [Header("UpdateType")]
    public UpdateType currentUpdateType;
    public enum UpdateType { Update, FixedUpdate, LateUpdate }

    void Update()
    {
        if (currentUpdateType == UpdateType.Update)
        {
            SetPosition();
        }
    }

    void FixedUpdate()
    {
        if (currentUpdateType == UpdateType.FixedUpdate)
        {
            SetPosition();
        }
    }

    void LateUpdate()
    {
        if (currentUpdateType == UpdateType.LateUpdate)
        {
            SetPosition();
        }
    }

    private void SetPosition()
    {
        if (target == null) return;
        Vector3 targetPosition = target.position + offset;
        transform.position = targetPosition;
    }
}
