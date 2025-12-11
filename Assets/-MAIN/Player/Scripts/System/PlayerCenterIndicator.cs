using UnityEngine;

public class PlayerCenterIndicator : PlayerPassive
{
    [Header("Settings")]
    public float rotationSpeed = 5;
    public bool followPlayer;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerPassivesManager pP; 
    [Header("References")]
    private PlayerCameraSteer cameraSteer; 
    private PlayerCollider playerCollider; 

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance; 
    }

    private void Start()
    {        
        playerCollider = cM.playerCollider;
        cameraSteer = pP.GetPassive<PlayerCameraSteer>(); 
    }

    void Update()
    {
        if (followPlayer)
        {
            float colliderFeetHeight = playerCollider.bodyCollider.center.y - (playerCollider.bodyCollider.height / 2);
            Vector3 colliderFeet = new Vector3(playerCollider.bodyCollider.center.x, colliderFeetHeight + 0.01f, playerCollider.bodyCollider.center.z);
            Vector3 worldFeetPosition = cM.playerCollider.bodyCollider.transform.TransformPoint(colliderFeet);
            transform.position = worldFeetPosition;
        }

        Vector3 forward = cameraSteer.GetHorizontalCameraDirection();
        if (forward.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(forward, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
