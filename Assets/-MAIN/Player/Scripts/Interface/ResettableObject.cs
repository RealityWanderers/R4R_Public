using UnityEngine;

public class ResettableObject : MonoBehaviour, IResettable
{
    private Vector3 originalPosition;

    private void Awake()
    {
        originalPosition = transform.position;
        ObjectRespawnManager.RegisterResettable(this);  // Register the object with the RespawnManager
    }

    private void OnDestroy()
    {
        ObjectRespawnManager.UnregisterResettable(this);  // Unregister when destroyed
    }

    public void ResetObject()
    {
        transform.position = originalPosition;  // Reset position to original
        gameObject.SetActive(true);  // Reactivate the object if it was disabled
    }

    public void DisableObject()
    {
        gameObject.SetActive(false);  // Temporarily disable the object (simulate death)
    }
}

