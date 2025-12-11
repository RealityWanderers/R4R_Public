using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;

public class PlayerHarmful : MonoBehaviour
{
    [Header("Settings")]
    [ShowIf(nameof(harmType), HarmType.Knockback)] public float playerMovementLockoutDuration = 1.2f;

    [Header("Type")]
    public HarmType harmType;
    public enum HarmType { Knockback, SlowDown }

    [ShowIf(nameof(harmType), HarmType.Knockback)] public float backwardForce;
    [ShowIf(nameof(harmType), HarmType.Knockback)] public float upwardForce;

    [ShowIf(nameof(harmType), HarmType.SlowDown)] public float speedLossMulti;

    [Header("Data")]
    private bool canTrigger;
    private float coolDownDuration = 0.15f; 

    [Header("Refs")]
    private PlayerComponentManager cM;
    private PlayerPassivesManager pP;
    private PlayerAbilityManager pA; 
    private PlayerCameraSteer cameraSteer;
    private PlayerQuickDash quickDash; 
    private PlayerInvulnerability invulnerability; 
    private Coroutine coroutine_StartCoolDown; 

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance;
        pA = PlayerAbilityManager.Instance;  
    }

    private void Start()
    {
        cameraSteer = pP.GetPassive<PlayerCameraSteer>();
        invulnerability = pP.GetPassive<PlayerInvulnerability>();
        quickDash = pA.GetAbility<PlayerQuickDash>(); 
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponentInParent<PlayerObject>() != null)
        {
            if (!invulnerability.isInvurn && canTrigger)
            {
                if (harmType == HarmType.Knockback)
                {
                    KnockBackPlayer(backwardForce, upwardForce);
                }
                if (harmType == HarmType.SlowDown)
                {
                    SlowDownPlayer(speedLossMulti);
                }

                if (coroutine_StartCoolDown != null) { StopCoroutine(coroutine_StartCoolDown); }
               coroutine_StartCoolDown = StartCoroutine(StartCoolDown()); 
            }
        }
    }

    void KnockBackPlayer(float backwardForce, float upwardForce)
    {
        cM.playerRB.linearVelocity = Vector3.zero;
        StartCoroutine(DisableScripts()); 
        Vector3 knockbackDirection = -cameraSteer.GetHorizontalCameraDirection() * backwardForce + Vector3.up * upwardForce;
        cM.playerRB.AddForce(knockbackDirection, ForceMode.VelocityChange);
    }

    public IEnumerator DisableScripts()
    {
        pP.DisablePassiveByType<PlayerCameraSteer>();
        pA.DisableAbilityByType<PlayerQuickDash>();     
        yield return new WaitForSeconds(playerMovementLockoutDuration);
        pP.EnablePassiveByType<PlayerCameraSteer>();
        pA.EnableAbilityByType<PlayerQuickDash>();
    }

    void SlowDownPlayer(float speedLossMulti)
    {
        Mathf.Clamp(speedLossMulti, 1, 0);
        cM.playerRB.linearVelocity *= (1 - speedLossMulti);
    }

    private IEnumerator StartCoolDown()
    {
        canTrigger = false;
        yield return new WaitForSeconds(coolDownDuration);
        canTrigger = true;
    }

    private void OnEnable()
    {
        ResetScript(); 
    }

    public void ResetScript()
    {
        StopAllCoroutines();
        canTrigger = true;
    }
}
