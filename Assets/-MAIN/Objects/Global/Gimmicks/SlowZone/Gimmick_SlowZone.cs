using Sirenix.OdinInspector;
using UnityEngine;

public class Gimmick_SlowZone : MonoBehaviour
{
    [Header("Settings")]
    public float slowRate;
    public bool requireGrounded = true;

    [Header("Settings")]
    [ReadOnly] public bool isActive;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerPassivesManager pP;
    [Header("References")]
    private Rigidbody rb;
    private ModularGroundedDetector groundedDetector;

    private void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance;
    }

    void Start()
    {
        rb = cM.playerRB;
        groundedDetector = pP.GetPassive<ModularGroundedDetector>();
    }

    public void ToggleActiveState(bool active)
    {
        isActive = active;
    }

    void FixedUpdate()
    {
        if (isActive && (!requireGrounded || groundedDetector.isGrounded))
        {
            SlowPlayer();
        }
    }

    void SlowPlayer()
    {
        rb.linearVelocity -= rb.linearVelocity.normalized * (slowRate * Time.fixedDeltaTime);

        // Ensure velocity doesn’t reverse or oscillate
        if (rb.linearVelocity.magnitude < 0.01f)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }
}
