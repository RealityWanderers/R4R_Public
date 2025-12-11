using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerRecenter : PlayerPassive
{
    [Header("Refs")]
    private PlayerComponentManager cM;

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
    }

    [Button]
    public void RecenterPlayer(bool recenterRotation)
    {
        Vector3 offset = cM.transform_MainCamera.position - cM.transform_MainCameraOffset.position;
        offset.y = 0;
        cM.transform_MainCameraOffset.position = cM.transform_XRRig.position - offset;

        if (recenterRotation)
        {
            Vector3 rigForward = cM.transform_XRRig.forward;
            rigForward.y = 0;
            Vector3 cameraForward = cM.transform_MainCamera.forward;
            cameraForward.y = 0;

            float angle = Vector3.SignedAngle(cameraForward, rigForward, Vector3.up);
            cM.transform_MainCameraOffset.RotateAround(cM.transform_MainCamera.position, Vector3.up, angle);
        }
    }
}
