using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening.Core.Easing;

public class PlayerGrabbableSystem : PlayerPassive
{
    [Header("Settings")]
    public float moveTime = 0.2f;
    public float delayAfterMoveComplete = 0.2f; //Time between move complete and grab event. 

    [Header("Controls")]
    [ReadOnly] public bool holdingGrabInput_L;
    [ReadOnly] public bool holdingGrabInput_R;

    [Header("State")]
    [ReadOnly] public bool isGrabbing;
    [ReadOnly] public bool canDash;
    private bool isTraveling;

    [Header("References")]
    private PlayerInputManager pI;
    private PlayerPassivesManager pP;
    private PlayerComponentManager cM;
    private Rigidbody rb;
    private ModularCustomGravity playerGravity;
    private PlayerGrabbableLockOnSystem lockOnSystem;
    public AudioSource source;
    private PlayerGrabbable currentGrabbable;

    private void Awake()
    {
        pI = PlayerInputManager.Instance;
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance;
    }

    private void Start()
    {
        rb = cM.playerRB;
        playerGravity = pP.GetPassive<ModularCustomGravity>();
        lockOnSystem = pP.GetPassive<PlayerGrabbableLockOnSystem>();
        ResetState();
    }

    public void Update()
    {
        if (!holdingGrabInput_L && pI.gripValue_L > 0.7f /*&& pI.triggerValue_L > 0.7f*/)
        {
            holdingGrabInput_L = true;
            CheckForValidTarget(cM.transform_Controller_L.position);
        }
        if (!holdingGrabInput_R && pI.gripValue_R > 0.7f /*&& pI.triggerValue_R > 0.7f*/)
        {
            holdingGrabInput_R = true;
            CheckForValidTarget(cM.transform_Controller_R.position);
        }

        if (holdingGrabInput_L && pI.gripValue_L < 0.7f /*&& pI.triggerValue_L < 0.7f*/)
        {
            holdingGrabInput_L = false;
            StopGrab();
        }
        if (holdingGrabInput_R && pI.gripValue_R < 0.7f /*&& pI.triggerValue_R < 0.7f*/)
        {
            holdingGrabInput_R = false;
            StopGrab();
        }

        if (isTraveling)
        {
            playerGravity.DisableGravity();
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        if (isTraveling)
        {
            // Calculate the time since the dash started
            float elapsedTime = Time.time - dashStartTime;

            // Calculate hand position and grabbable position for offset calculation
            Vector3 grabbableWorldPos = currentGrabbable.transform.position;
            //Debug.Log("Grabbable" + grabbableWorldPos);
            //Debug.Log("HandPos" + handPos);
            Vector3 offsetNeeded = grabbableWorldPos - handPos;
            //Debug.Log("Offsetneeded" + offsetNeeded);
            Vector3 finalTarget = startPos + offsetNeeded; // Determine final position relative to startPos
            //Debug.Log("Target" + grabbableWorldPos);

            if (elapsedTime < moveTime)
            {
                // Interpolate position smoothly from startPos to finalTarget
                float t = Mathf.Clamp01(elapsedTime / moveTime);  // Progress from 0 to 1 based on elapsed time
                Vector3 interpolatedPos = Vector3.Lerp(startPos, finalTarget, t); // Simple Lerp, no easing

                // Move the Rigidbody towards the interpolated position
                rb.MovePosition(interpolatedPos);
            }
            else
            {
                // Once moveTime is complete, directly set the player at the final target
                rb.MovePosition(finalTarget);

                // After reaching the final position, grab the object
                Grab();
            }
        }
    }

    public void CheckForValidTarget(Vector3 controllerPos)
    {
        if (lockOnSystem.isLockedOn && canDash)
        {
            if (lockOnSystem.grabbable != null)
            {
                currentGrabbable = lockOnSystem.grabbable;
                DashToGrabbable(/*controllerPos, lockOnSystem.endLocation*/);
            }
        }
    }

    public void StartGrab()
    {
        if (isGrabbing == true)
        {
            //Debug.Log("AlreadyActive!");
        }
        else
        {
            isGrabbing = true;
            //Debug.Log("StartingGrabbable");
        }
    }

    public void StopGrab()
    {
        isGrabbing = false;
        //Debug.Log("Stop");
    }

    private Sequence dashSequence;
    private float dashStartTime;
    private Vector3 startPos;
    private Vector3 handPos; 
    public void DashToGrabbable()
    {
        StartGrab();
        dashSequence?.Kill();

        canDash = false;

        currentGrabbable.onDashStart.Invoke();
        source.Play();

        startPos = rb.position;
        handPos = (holdingGrabInput_L) ? cM.transform_Controller_L.position : cM.transform_Controller_R.position;
        dashStartTime = Time.time;
        isTraveling = true;
    }

    public void Grab()
    {
        currentGrabbable.onGrab.Invoke();
        isTraveling = false;
        //ResetState();
    }

    public void ResetState()
    {
        playerGravity.EnableGravity();
        canDash = true;
        isTraveling = false;
    }

    public bool CheckForActiveGrab()
    {
        return isGrabbing;
    }
}
