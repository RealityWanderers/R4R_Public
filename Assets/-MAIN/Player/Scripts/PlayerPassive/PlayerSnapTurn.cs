using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerSnapTurn : PlayerPassive
{
    [Header("Settings")]
    public float snapTurnCooldown = 0.15f;
    public float smoothTurnSpeedMulti = 5; 
    [ReadOnly] public float currentSnapTurnDegree;
    private bool canSnap;
    private float timeStamp;

    [Header("Haptics")]
    [PropertyRange(0, 1)]
    public float hapticAmplitude = 0.5f;
    [PropertyRange(0, 1)]
    public float hapticDuration = 0.1f;
    [Header("Refs")]
    private PlayerComponentManager cM;
    private PlayerInputManager pI;

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pI = PlayerInputManager.Instance;
    }

    void Update()
    {
        if (PlayerPrefs.GetInt(PlayerSaveData.TurnTypeKey) == (int)PlayerSaveData.TurnType.SnapTurn)
        {
            //Debug.Log("TurnType: Snap");

            if (Time.time > timeStamp + snapTurnCooldown)
            {
                canSnap = true;
            }

            float snapTurnDegree = PlayerPrefs.GetInt("SnapTurnAngle");
            if (snapTurnDegree == 0) return; //Not yet set. 
            if (canSnap && pI.stickAxis_X_R <= -0.5f)
            {
                SnapRight(snapTurnDegree);
            }
            if (canSnap && pI.stickAxis_X_R >= 0.5f)
            {
                SnapLeft(snapTurnDegree);
            }

        }
        else if (PlayerPrefs.GetInt(PlayerSaveData.TurnTypeKey) == (int)PlayerSaveData.TurnType.SmoothTurn)
        {
            //Debug.Log("TurnType: Smooth");

            float smoothTurnSpeed = PlayerPrefs.GetFloat("SmoothTurnSpeed");
            if (smoothTurnSpeed == 0) return; //Not yet set. 
            if (pI.stickAxis_X_R <= -0.5f)
            {
                SmoothTurnRight(smoothTurnSpeedMulti * smoothTurnSpeed * Time.deltaTime);
            }
            if (pI.stickAxis_X_R >= 0.5f)
            {
                SmoothTurnLeft(smoothTurnSpeedMulti * smoothTurnSpeed * Time.deltaTime);
            }
        }
    }

    void SnapLeft(float snapTurnAmount)
    {
        Vector3 pivotPoint = cM.transform_MainCamera.position;
        cM.transform_MainCameraOffset.RotateAround(pivotPoint, Vector3.up, -snapTurnAmount);
        StartCoolDown();
        TriggerHaptics();
    }

    void SnapRight(float snapTurnAmount)
    {
        Vector3 pivotPoint = cM.transform_MainCamera.position;
        cM.transform_MainCameraOffset.RotateAround(pivotPoint, Vector3.up, snapTurnAmount);
        StartCoolDown();
        TriggerHaptics();
    }

    void SmoothTurnLeft(float turnSpeed)
    {
        Vector3 pivotPoint = cM.transform_MainCamera.position;
        cM.transform_MainCameraOffset.RotateAround(pivotPoint, Vector3.up, -turnSpeed);
        TriggerHaptics();
    }

    void SmoothTurnRight(float turnSpeed)
    {
        Vector3 pivotPoint = cM.transform_MainCamera.position;
        cM.transform_MainCameraOffset.RotateAround(pivotPoint, Vector3.up, turnSpeed);
        TriggerHaptics();
    }

    void TriggerHaptics()
    {
        pI.playerHaptic_R.SendHapticImpulse(hapticAmplitude, hapticDuration);
    }

    void StartCoolDown()
    {
        timeStamp = Time.time;
        canSnap = false;
    }

    public override void ResetPassive()
    {
        base.ResetPassive();

        timeStamp = 0;
        canSnap = true;
    }
}
