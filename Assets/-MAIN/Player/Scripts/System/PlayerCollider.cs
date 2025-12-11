using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class PlayerCollider : MonoBehaviour
{
    [Header("Refs")]
    private PlayerComponentManager cM;
    [HideInInspector] public CapsuleCollider bodyCollider;
    //public SphereCollider feetCollider;
    private Vector3 colliderCenter; 

    private void Awake()
    {
        cM = PlayerComponentManager.Instance;
        bodyCollider = GetComponent<CapsuleCollider>(); 
    }

    private void Update()
    {
        //MoveFeet();
        MoveBody();
    }

    public void MoveBody()
    {
        //Vector3 xrRigPos = cM.transform_XRRig.position;
        //Vector3 newColliderPosition = cM.transform_MainCamera.position;
        //newColliderPosition.x = cM.transform_MainCamera.position.x;  
        //newColliderPosition.z = cM.transform_MainCamera.position.z; 
        //float cameraHeight = cM.transform_MainCamera.position.y - xrRigPos.y;
        //newColliderPosition.y = xrRigPos.y;
        //bodyCollider.transform.position = newColliderPosition;
        //bodyCollider.height = cameraHeight;
        //bodyCollider.center = new Vector3(0, cameraHeight / 2, 0);

        //Vector3 cameraPos = cM.transform_MainCamera.localPosition;
        //bodyCollider.height = Mathf.Clamp(cameraPos.y, 0.3f, 2.2f);
        //bodyCollider.center = new Vector3(cameraPos.x, bodyCollider.height / 2, cameraPos.z);

        // Get the camera's position in world space
        Vector3 cameraWorldPos = cM.transform_MainCamera.position;

        // Transform the camera's world position into the local space of the bodyCollider
        Vector3 cameraLocalPos = bodyCollider.transform.InverseTransformPoint(cameraWorldPos);

        // Update the collider height and center based on the camera's local position
        bodyCollider.height = Mathf.Clamp(cameraLocalPos.y, 0.3f, 2.2f);
        colliderCenter = new Vector3(cameraLocalPos.x, bodyCollider.height / 2, cameraLocalPos.z);
        bodyCollider.center = colliderCenter; 
    }

    //public void MoveFeet()
    //{
    //    Vector3 xrRigPos = cM.transform_XRRig.position;
    //    Vector3 newColliderPosition = cM.transform_MainCamera.position;
    //    newColliderPosition.x = cM.transform_MainCamera.position.x;
    //    newColliderPosition.z = cM.transform_MainCamera.position.z;
    //    newColliderPosition.y = xrRigPos.y;
    //    feetCollider.transform.position = newColliderPosition;
    //}
}
