using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerFreezeFrame : PlayerPassive
{
    [ReadOnly] public bool isFreezeFramed;
    private Vector3 previousVelocity;
    private float previousMagnitude;
    private float freezeFrameTimeStamp;
    private float freezeFrameDuration;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerPassivesManager pP;
    [Header("Refs")]
    private ModularCustomGravity customGravity;


    public void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance;
    }

    private void Start()
    {
        customGravity = pP.GetPassive<ModularCustomGravity>(); 
    }

    void Update()
    {
        if (isFreezeFramed)
        {
            customGravity.DisableGravity();
            cM.playerRB.linearVelocity = Vector3.zero;
        }
        if (isFreezeFramed && Time.time > freezeFrameTimeStamp + freezeFrameDuration)
        {
            ResetFreezeFrame(); 
        }
    }

    public void StartFreezeFrame(float duration)
    {
        previousVelocity = cM.playerRB.linearVelocity.normalized;
        previousMagnitude = cM.playerRB.linearVelocity.magnitude;
        freezeFrameTimeStamp = Time.time;
        freezeFrameDuration = duration;
        isFreezeFramed = true; 
    }

    public void ResetFreezeFrame()
    {
        isFreezeFramed = false;
        customGravity.EnableGravity();
        cM.playerRB.linearVelocity = previousVelocity * previousMagnitude;
    }

    public override void ResetPassive()
    {
        base.ResetPassive();

        ResetFreezeFrame(); 
    }
}
