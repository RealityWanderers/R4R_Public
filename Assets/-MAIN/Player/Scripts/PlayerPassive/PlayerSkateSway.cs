using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkateSway : PlayerPassive
{
    [Header("Settings")]
    public float swayAmount = 1.5f;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerPassivesManager pP; 
    [Header("Refs")]
    private PlayerCameraSteer cameraSteer;
    private PlayerSoftSpeedCap softSpeedCap;

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance;
    }

    private void Start()
    {
        cameraSteer = pP.GetPassive<PlayerCameraSteer>();
        softSpeedCap = pP.GetPassive<PlayerSoftSpeedCap>();
    }

    public void SwayLeft(float swayMulti)
    {
        float currentSwayAmount = swayAmount * swayMulti * (softSpeedCap.GetSpeedPercentage() + 0.2f); 
        Vector3 leftBoostDirection = Quaternion.Euler(0, -60, 0) * cameraSteer.GetHorizontalCameraDirection();
        cM.playerRB.AddForce(leftBoostDirection * currentSwayAmount, ForceMode.VelocityChange);
    }

    public void SwayRight(float swayMulti)
    {
        float currentSwayAmount = swayAmount * swayMulti * (softSpeedCap.GetSpeedPercentage() + 0.2f);
        Vector3 rightBoostDirection = Quaternion.Euler(0, 60, 0) * cameraSteer.GetHorizontalCameraDirection();
        cM.playerRB.AddForce(rightBoostDirection * currentSwayAmount, ForceMode.VelocityChange);
    }

    public override void ResetPassive()
    {
        base.ResetPassive();
    }
}
