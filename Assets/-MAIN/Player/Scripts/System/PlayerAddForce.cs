using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAddForce : MonoBehaviour
{
    [Header("ForceAmount")]
    public float forwardForceAmount = 6f;
    public float forwardForceDelay = 0; 
    public float upwardsForceAmount = 12f;
    public float upwardsForceDelay = 0; 
    private bool canAddForce;

    [Header("ForceDirection")]
    public ForceDirection currentForceDirection;
    public enum ForceDirection { Camera, CameraInvert, ObjectForward, ObjectBackward, RelativeKnockback }

    [Header("ResetQuickDash")]
    public bool resetQuickDashCharges = true;

    [Header("ResetVelocity")]
    public bool resetVelocity;

    [Header("Gravity")]
    public bool tempDisableGravity = false;
    public float tempDisableGravityDuration = 0.3f;

    [Header("CameraSteer")]
    public bool tempDisableCameraSteer = false;
    public float tempDisableCameraSteerDuration = 0.5f;

    [Header("FreezeFrame")]
    public float freezeFrameDuration = 0.05f;
    private float freezeFrameTimeStamp;

    [Header("Loop")]
    public float loopInterval = 0.01f; // Time between loops
    [ReadOnly] public bool currentlyLooping = false;
    private float lastLoopTime;

    [Header("IncreaseSpeedCap")]
    public bool IncreaseSpeedCap;
    [ShowIf("IncreaseSpeedCap")] [Range(0, 3)] public float speedCapIncreaseMulti;
    [ShowIf("IncreaseSpeedCap")] public float speedCapIncreaseDuration = 0.5f;

    [Header("Cooldown")]
    public float cooldownDuration = 0f;
    [ReadOnly] public bool onCoolDown;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerAbilityManager pA;
    private PlayerPassivesManager pP;
    [Header("Refs")]
    private PlayerFreezeFrame freezeFrame;
    private PlayerSoftSpeedCap softSpeedCap;
    private PlayerCameraSteer cameraSteer;
    private ModularCustomGravity customGravity;
    private PlayerQuickDash quickDash;

    public void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pA = PlayerAbilityManager.Instance;
        pP = PlayerPassivesManager.Instance;
    }

    private void Start()
    {
        quickDash = pA.GetAbility<PlayerQuickDash>();
        freezeFrame = pP.GetPassive<PlayerFreezeFrame>();
        softSpeedCap = pP.GetPassive<PlayerSoftSpeedCap>();
        cameraSteer = pP.GetPassive<PlayerCameraSteer>();
        customGravity = pP.GetPassive<ModularCustomGravity>();
    }

    public void StartAddForce()
    {
        if (onCoolDown) { return; }
        if (freezeFrameDuration != 0)
        {
            freezeFrame.StartFreezeFrame(freezeFrameDuration);
            freezeFrameTimeStamp = Time.time;
        }
        canAddForce = true;
        //Debug.Log("StartedForce"); 
    }

    private void FixedUpdate()
    {
        if (canAddForce && Time.time > freezeFrameTimeStamp + freezeFrameDuration && !freezeFrame.isFreezeFramed)
        {
            AddForce();
        }

        if (currentlyLooping && Time.time >= lastLoopTime + loopInterval && !onCoolDown)
        {
            AddForce();
            lastLoopTime = Time.time;
        }
    }

    public void StartLoopingForce()
    {
        currentlyLooping = true;
    }

    public void StopLoopingForce()
    {
        currentlyLooping = false;
    }

    private void AddForce()
    {
        canAddForce = false;

        if (resetVelocity)
        {
            ResetVelocity();
        }

        if (IncreaseSpeedCap)
        {
            softSpeedCap.IncreaseSpeedCapWithDuration(speedCapIncreaseMulti, speedCapIncreaseDuration);
        }

        if (tempDisableCameraSteer)
        {
            cameraSteer.TempDisable(tempDisableCameraSteerDuration);
        }

        StartCoroutine(ApplyForwardForceWithDelay());
        StartCoroutine(ApplyUpwardForceWithDelay());

        quickDash.ResetCharges();
        StartCoroutine(StartCoolDown());

        if (tempDisableGravity)
        {
            customGravity.TempDisableGravity(tempDisableGravityDuration);
        }
    }

    private IEnumerator ApplyForwardForceWithDelay()
    {
        if (forwardForceDelay > 0f)
            yield return new WaitForSeconds(forwardForceDelay);

        switch (currentForceDirection)
        {
            case ForceDirection.Camera:
                cM.playerRB.AddForce(cameraSteer.GetHorizontalCameraDirection() * forwardForceAmount, ForceMode.VelocityChange);
                break;
            case ForceDirection.CameraInvert:
                cM.playerRB.AddForce(-cameraSteer.GetHorizontalCameraDirection() * forwardForceAmount, ForceMode.VelocityChange);
                break;
            case ForceDirection.ObjectForward:
                cM.playerRB.AddForce(transform.forward * forwardForceAmount, ForceMode.VelocityChange);
                break;
            case ForceDirection.ObjectBackward:
                cM.playerRB.AddForce(-transform.forward * forwardForceAmount, ForceMode.VelocityChange);
                break;
            case ForceDirection.RelativeKnockback:
                Vector3 knockbackDirection = (cM.transform_PlayerFeet.position - transform.position).normalized;
                cM.playerRB.AddForce(knockbackDirection * forwardForceAmount, ForceMode.VelocityChange);
                break;
        }
    }

    private IEnumerator ApplyUpwardForceWithDelay()
    {
        if (upwardsForceDelay > 0f)
            yield return new WaitForSeconds(upwardsForceDelay);

        cM.playerRB.AddForce(Vector3.up * upwardsForceAmount, ForceMode.VelocityChange);
    }

    public void ResetVelocity() //Can also be called from a trigger enter in niche cases, like wanting reset velocity on trigger but then loop add force without reset velocity every frame.
    {
        cM.playerRB.linearVelocity = Vector3.zero;
    }

    public IEnumerator StartCoolDown()
    {
        onCoolDown = true;
        yield return new WaitForSeconds(cooldownDuration);
        onCoolDown = false;
    }

    void OnDrawGizmosSelected()
    {
        //Very rough estimation to get an idea of the trajectory. 
        //We do force * 0.4f to make the length more comparable to in game.
        //Also only works well when the velocity is set to reset upon adding force. 
        if (currentForceDirection == ForceDirection.ObjectForward)
        {
            Vector3 forwardVector = transform.forward * (forwardForceAmount * 0.4f);
            Vector3 upwardVector = Vector3.up * (upwardsForceAmount * 0.4f);
            Vector3 combinedVector = forwardVector + upwardVector;

            Debug.DrawRay(transform.position, combinedVector, Color.red);
        }
    }
}
