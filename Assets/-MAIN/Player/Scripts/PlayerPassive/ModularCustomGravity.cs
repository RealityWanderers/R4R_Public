using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ModularCustomGravity : PlayerPassive
{
    [Header("Settings")]
    [ShowInInspector] [ReadOnly] private bool enableGravity;
    [SerializeField] private float defaultGravity = 14;
    private float currentGravity;
    private float currentGravityMulti = 1; 
    private Vector3 gravityDirection;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerPassivesManager pP;
    [Header("References")]
    private ModularGroundedDetector groundedDetector;
    private ModularSlopeDetector slopeDetector;
    private Rigidbody rb;

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance;
    }

    void Start()
    {
        rb = cM.playerRB;
        rb.useGravity = false;
        groundedDetector = pP.GetPassive<ModularGroundedDetector>();
        slopeDetector = pP.GetPassive<ModularSlopeDetector>();
        ResetPassive();
    }

    void FixedUpdate()
    {
        if (!enableGravity) return;
        if (cM.playerRB.isKinematic) return;
        if (!groundedDetector.isGrounded || slopeDetector.slopeType == ModularSlopeDetector.SlopeType.DownHill)
        {
            //Debug.Log("ApplyingGravity"); 
            // Apply custom gravity when airborne
            rb.AddForce(Vector3.down * (currentGravity * currentGravityMulti), ForceMode.Acceleration);
            //Debug.Log("IsaddingForce"); 
        }
        else
        {
            //Debug.Log("NOTApplyingGravity");
            // Reset velocity upon landing
            if (rb.linearVelocity.y < 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            }
        }
    }

    public void TempDisableGravity(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(_TempDisableGravity(duration));
        //Debug.Log("Gravity Disabled"); 
    }

    IEnumerator _TempDisableGravity(float duration)
    {
        DisableGravity(); 
        yield return new WaitForSeconds(duration);
        EnableGravity();
        //Debug.Log("Gravity Enabled");
    }

    public float GetDefaultGravity()
    {
        return defaultGravity; 
    }

    public void EnableGravity()
    {
        enableGravity = true; 
    }
    public void DisableGravity()
    {
        enableGravity = false; 
    }

    public void ChangeGravityMulti(float multi)
    {
        currentGravityMulti = multi; 
    }
    public void ResetGravityMulti()
    {
        currentGravityMulti = 1; 
    }

    public void ChangeGravityAmount(float newGravity)
    {
        currentGravity = newGravity;
    }
    public void ResetGravityAmount()
    {
        currentGravity = defaultGravity;
    }

    public override void ResetPassive()
    {
        base.ResetPassive();

        EnableGravity(); 
        ResetGravityMulti(); 
        ResetGravityAmount();
    }
}
