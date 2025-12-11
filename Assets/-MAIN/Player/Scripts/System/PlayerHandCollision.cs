using UnityEngine;

public class PlayerHandCollision : MonoBehaviour
{
    [Header("Settings")]
    public bool isColliding;
    public float offset = 0.2f;
    public LayerMask collisionLayer;

    [Header("Refs")]
    private PlayerComponentManager cM;

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
    }

    public void Update()
    {
        Vector3 direction = (transform.position - cM.transform_MainCamera.position).normalized;
        float rayLength = Vector3.Distance(cM.transform_MainCamera.position, transform.position); 

        Debug.DrawRay(cM.transform_MainCamera.position, direction * (rayLength - offset), Color.green);

        if (Physics.Raycast(cM.transform_MainCamera.position, direction, out RaycastHit hit, rayLength - offset, collisionLayer))
        {
            isColliding = true;
        }
        else
        {
            isColliding = false;
        }
    }
}
