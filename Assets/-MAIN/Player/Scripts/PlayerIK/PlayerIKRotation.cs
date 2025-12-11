using UnityEngine;

public class PlayerIKRotation : MonoBehaviour
{
    [Header("Settings")]
    [Range(0,1)] public float cameraSkew;

    [Header("Refs")]
    public Transform playerCamera; //Manually asign  these refs to ensure they work in calibrations screen when cM is inactive. 
    public Transform controller_L;
    public Transform controller_R; 

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 controllerPos_L = controller_L.position;
        Vector3 controllerPos_R = controller_R.position;
        Vector3 controllerMidPoint = (controllerPos_L + controllerPos_R) / 2;

        Vector3 cameraDirection = Vector3.ProjectOnPlane(playerCamera.forward, Vector3.up).normalized;
        Vector3 finalMidPoint = Vector3.Lerp(controllerMidPoint, controllerMidPoint + cameraDirection, cameraSkew);
        Vector3 directionToFace = finalMidPoint - transform.position;

        directionToFace.y = 0; 

        if (directionToFace != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToFace);
        }
    }
}
