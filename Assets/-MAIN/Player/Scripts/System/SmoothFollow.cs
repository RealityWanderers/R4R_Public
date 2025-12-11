using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    public Transform target;
    [Range(0, 1)] public float positionDamping;
    public Vector3 positionOffset; 
    [Range(0, 1)] public float rotationDamping;

    void Start()
    {
        ReTarget(); 
    }

    public void ReTarget()
    {
        transform.SetPositionAndRotation(target.position, target.rotation);
    }

    void Update()
    {
        // Calculate the target's world-space offset based on its local rotation
        Vector3 relativeOffset = target.right * positionOffset.x +
                                 target.up * positionOffset.y +
                                 target.forward * positionOffset.z;

        // Apply the offset in world space
        Vector3 offsetPosition = target.position + relativeOffset;
        transform.SetPositionAndRotation(Vector3.Lerp(transform.position, offsetPosition, positionDamping), Quaternion.Lerp(transform.rotation, target.rotation, rotationDamping)); 
    }
}
