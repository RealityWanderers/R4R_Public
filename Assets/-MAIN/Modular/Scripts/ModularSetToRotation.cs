using UnityEngine;

public class ModularSetToRotation : MonoBehaviour
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
            SetToRotation();
        }
    }

    void FixedUpdate()
    {
        if (currentUpdateType == UpdateType.FixedUpdate)
        {
            SetToRotation();
        }
    }

    void LateUpdate()
    {
        if (currentUpdateType == UpdateType.LateUpdate)
        {
            SetToRotation(); 
        }
    }

    private void SetToRotation()
    {
        transform.rotation = target.rotation * Quaternion.Euler(offset);
    }
}
