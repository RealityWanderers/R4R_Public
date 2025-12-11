using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Follow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.3F;
    private Vector3 velocity = Vector3.zero;
    public Vector3 offset;

    void Update()
    {
        Vector3 targetPosition = target.TransformPoint(offset);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        Vector3 lookDirection = target.position - transform.position;
        transform.rotation = Quaternion.LookRotation(-lookDirection, Vector3.up);
    }
}

