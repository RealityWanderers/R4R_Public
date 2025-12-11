using UnityEngine;
using UnityEngine.Splines;

public class PlayerRailGrindDetector : MonoBehaviour
{
    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerAbilityManager pA;
    [Header("Refs")]
    private PlayerRailGrind playerRailGrind; 

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pA = PlayerAbilityManager.Instance;
    }

    private void Start()
    {        
        playerRailGrind = pA.GetAbility<PlayerRailGrind>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        //Debug.Log("TriggerEnter");
        //Debug.Log("Isgrinding" + !cM.playerRailGrind.isRailGrinding);
        //Debug.Log("IsFalling" + cM.playerRB.linearVelocity.y);
        //Debug.Log("OnCooldown" + !cM.playerRailGrind.onCoolDown);

        if (!playerRailGrind.isRailGrinding && cM.playerRB.linearVelocity.y <= 0 && !playerRailGrind.onCoolDown)
        {
            CheckNearbyRails(collision);
        }
    }

    void CheckNearbyRails(Collider collision)
    {
        //Debug.Log("Checking");
        if (collision.TryGetComponent(out RailGrindObject railGrind))
        {
            playerRailGrind.splineContainer = railGrind.GetComponent<SplineContainer>();
            playerRailGrind.AttachToSpline();
            //Debug.Log("RailFound");
        }
    }
}

