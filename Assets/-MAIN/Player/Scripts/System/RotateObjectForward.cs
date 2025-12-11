using UnityEngine;

public class RotateObjectForward : MonoBehaviour
{
    public void DoRotateForward(float rotateAmount)
    {
        transform.Rotate(Vector3.up * rotateAmount, Space.Self);
    }
}
