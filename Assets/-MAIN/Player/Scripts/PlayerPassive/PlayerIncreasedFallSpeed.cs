using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerIncreasedFallSpeed : PlayerPassive
{
    [Header("ExtraFallingGravity")]
    public float extraGravityActivationSpeedThreshold = -2f;
    public float fallingGravityMulti = 1.2f;

    private float defaultGravity;
    [ReadOnly] public bool extraGravityTriggered;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerPassivesManager pP;
    [Header("Refs")]
    private ModularGroundedDetector groundedDetector;
    private ModularCustomGravity customGravity; 

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance;
    }

    private void Start()
    {        
        groundedDetector = pP.GetPassive<ModularGroundedDetector>();
        customGravity = pP.GetPassive<ModularCustomGravity>(); 
        defaultGravity = customGravity.GetDefaultGravity();
    }

    void Update()
    {
        // Check for extra gravity trigger
        if (!extraGravityTriggered && cM.playerRB.linearVelocity.y <= extraGravityActivationSpeedThreshold && !IsAboutToLand())
        {
            IncreaseGravity();
        }

        // Reset gravity when grounded
        if (groundedDetector.isGrounded || IsAboutToLand())
        {
            ResetDefaultGravity();
        }
    }

    public void ResetDefaultGravity()
    {
        extraGravityTriggered = false;
        customGravity.ResetGravityMulti();
    }

    public void IncreaseGravity()
    {
        extraGravityTriggered = true;
        customGravity.ChangeGravityMulti(fallingGravityMulti);
    }

    private bool IsAboutToLand()
    {
        // Check if the player is close to the ground using a raycast
        RaycastHit hit;
        if (Physics.Raycast(cM.playerRB.position, Vector3.down, out hit, 0.5f)) // Adjust the ray distance as needed
        {
            //Debug.Log("AboutToLand");
            return true;
        }
        return false;
    }

    public override void ResetPassive()
    {
        base.ResetPassive();

        ResetDefaultGravity(); 
    }
}
