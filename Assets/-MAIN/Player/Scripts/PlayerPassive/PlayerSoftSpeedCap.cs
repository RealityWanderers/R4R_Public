using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class PlayerSoftSpeedCap : PlayerPassive
{
    [Header("Settings")]
    public float currentSoftSpeedCap = 5;
    private float defaultSpeedCap; 
    public float speedDrainAmountWhenAbove = 5f;
    public float amountAboveSpeedCapFactor = 9f;
    public float airborneDrainMulti = 0.9f;
    [Header("Data")]
    [ReadOnly] public float currentPlayerSpeed;
    [ReadOnly] public float currentPlayerHorizontalSpeed;
    [ReadOnly] public float defaultSoftSpeedCap;
    [ReadOnly] public float currentDrain;
    [ReadOnly] public float amountAboveSpeedCap;
    [ReadOnly] public float speedPercentage;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerPassivesManager pP; 
    [Header("Refs")]
    private ModularGroundedDetector groundedDetector; 

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance;
    }

    void Start()
    {
        groundedDetector = pP.GetPassive<ModularGroundedDetector>(); 
        defaultSoftSpeedCap = currentSoftSpeedCap;
    }

    void FixedUpdate()
    {
        if (currentPlayerSpeed > currentSoftSpeedCap)
        {
            cM.playerRB.AddForce(-cM.playerRB.linearVelocity.normalized * currentDrain * Time.fixedDeltaTime);
        }
    }

    void Update()
    {
        currentPlayerHorizontalSpeed = GetHorizontalSpeed(); //Used for UI
        currentPlayerSpeed = GetPlayerSpeed();

        if (currentPlayerSpeed < 0.01f) //Prevents a bug where the playerspeed would be set back to max speed cap.
        {
            currentPlayerSpeed = 0;
            currentPlayerHorizontalSpeed = 0;
            speedPercentage = 0;
        }

        if (currentPlayerSpeed > currentSoftSpeedCap)
        {
            amountAboveSpeedCap = currentPlayerSpeed - currentSoftSpeedCap;
            if (groundedDetector.isGrounded)
            {
                currentDrain = speedDrainAmountWhenAbove * (amountAboveSpeedCap * amountAboveSpeedCapFactor);
            }
            else
            {
                currentDrain = speedDrainAmountWhenAbove * ((amountAboveSpeedCap * amountAboveSpeedCapFactor) * airborneDrainMulti);
            }
        }

        if (currentPlayerSpeed < currentSoftSpeedCap)
        {
            currentDrain = 0;
            amountAboveSpeedCap = 0;
        }

        speedPercentage = GetSpeedPercentage();
    }

    public float GetPlayerSpeed()
    {
        return cM.playerRB.linearVelocity.magnitude;
    }

    public float GetHorizontalSpeed()
    {
        float horizontalSpeed = Vector3.ProjectOnPlane(cM.playerRB.linearVelocity, Vector3.up).magnitude;
        return horizontalSpeed; 
    }

    public float GetSpeedPercentage()
    {
        float speedPercentage = GetHorizontalSpeed() / currentSoftSpeedCap;
        if (speedPercentage < 0.01f)
        {
            speedPercentage = 0;
        }
        return speedPercentage;
    }

    public void IncreaseSpeedCapWithDuration(float multi, float duration)
    {
        StartCoroutine(CoroutineIncreaseSpeedCap(multi, duration)); 
    }

    public void IncreaseSpeedCap(float multi)
    {
        currentSoftSpeedCap = defaultSoftSpeedCap * multi;
    }

    public IEnumerator CoroutineIncreaseSpeedCap(float multi, float duration)
    {
        currentSoftSpeedCap = defaultSoftSpeedCap * multi;
        yield return new WaitForSeconds(duration);
        ResetSpeedCap(); 
    }

    public void ResetSpeedCap()
    {
        currentSoftSpeedCap = defaultSoftSpeedCap;
    }

    public void ResetPlayerVelocity()
    {

    }

    public override void ResetPassive()
    {
        base.ResetPassive();

        ResetSpeedCap();
    }
}
