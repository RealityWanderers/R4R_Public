using Sirenix.OdinInspector;
using UnityEngine;

public class Gimmick_JumpPad : MonoBehaviour
{
    [Header("AddForce")]
    private PlayerAddForce addForce;

    private Transform playerFeet; 

    [Header("Managers")]
    private PlayerAbilityManager pA;
    private PlayerComponentManager cM; 
    private PlayerJump playerJump;

    private void Awake()
    {
        pA = PlayerAbilityManager.Instance;
        cM = PlayerComponentManager.Instance; 
    }

    private void Start()
    {
        playerJump = pA.GetAbility<PlayerJump>();
        playerFeet = cM.transform_PlayerFeet; 
        addForce = GetComponent<PlayerAddForce>(); 
    }

    public bool IsStandingOnJumpPad() //Extra check to ensure the player is on the pad and not next to it.
    {
        Vector3 origin = playerFeet.position;
        Vector3 direction = Vector3.down;

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, 1f))
        {
            if (hit.collider.GetComponentInParent<Gimmick_JumpPad>() != null)
            {
                return true;
            }
        }

        return false;
    }

    public void CheckForJump()
    {
        //If trigger exit and we are jumping.
        if (playerJump.isJumping && IsStandingOnJumpPad())
        {
            playerJump.CancelJump();
            addForce.StartAddForce();
        }
    }
}
