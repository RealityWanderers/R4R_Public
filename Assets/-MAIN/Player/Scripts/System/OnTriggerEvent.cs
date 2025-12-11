using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class OnTriggerEnterEvent : MonoBehaviour
{
    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerExit;

    [Header("Settings")]
    public bool checkForPlayer = true;
    public bool checkForHand = false;
    public bool checkForHomingAttack = false; 
    public float cooldownDuration = 0f;
    [ReadOnly] public bool onCooldown = false;

    private PlayerAbilityManager pA;
    private PlayerLockOnDash lockOnDash;

    private void Awake()
    {
        if (checkForHomingAttack)
        {
            pA = PlayerAbilityManager.Instance; 
        }
    }

    private void Start()
    {
        if (checkForHomingAttack)
        {
            lockOnDash = pA.GetAbility<PlayerLockOnDash>(); 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (onCooldown) return;

        if (checkForPlayer && other.GetComponentInParent<PlayerObject>() == null)
            return;

        if (checkForHand && other.GetComponentInParent<PlayerGrabbableHand>() == null)
            return;

        if (checkForHomingAttack && !lockOnDash.isDashing)
            return;

        onTriggerEnter.Invoke();
        StartCoroutine(CooldownRoutine());
    }

    private void OnTriggerExit(Collider other)
    {
        if (checkForPlayer && other.GetComponentInParent<PlayerObject>() == null)
            return;

        if (checkForHand && other.GetComponentInParent<PlayerGrabbableHand>() == null)
            return;

        if (checkForHomingAttack && !lockOnDash.isDashing)
            return;

        onTriggerExit.Invoke();
    }

    [Button]
    private void DebugInvokeTriggerEnter()
    {
        onTriggerEnter.Invoke();
    }

    [Button]
    private void DebugInvokeTriggerExit()
    {
        onTriggerExit.Invoke();
    }

    private IEnumerator CooldownRoutine()
    {
        onCooldown = true;
        yield return new WaitForSeconds(cooldownDuration);
        onCooldown = false;
    }
}