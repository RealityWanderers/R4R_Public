using UnityEngine;

public class PlayerWallGrindDetector : PlayerPassive
{
    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerAbilityManager pM;
    private PlayerPassivesManager pP; 
    [Header("Refs")]
    private PlayerWallGrind wallGrind;
    private ModularGroundedDetector groundedDetector; 

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pM = PlayerAbilityManager.Instance;
        pP = PlayerPassivesManager.Instance;
    }

    private void Start()
    {        
        groundedDetector = pP.GetPassive<ModularGroundedDetector>(); 
        wallGrind = pM.GetAbility<PlayerWallGrind>(); 
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<WallGrindObject>(out WallGrindObject wallGrindObject))
        {
            if (!groundedDetector.isGrounded && !wallGrind.isWallGrinding) { wallGrind.StartWallGrind(collision); }
        }
    }

    public override void ResetPassive()
    {
        base.ResetPassive();
    }
}
