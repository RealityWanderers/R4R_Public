using Sirenix.OdinInspector;
using UnityEngine;

public class Gimmick_MonkeyBar : MonoBehaviour
{
    [Header("Movement")]
    public bool moveWithBar;
    private Vector3 lastPos;

    [Header("AddForce")]
    public PlayerAddForce addForce_Wall;
    public PlayerAddForce addForce_AwayFromWall;

    [Header("State")]
    [ReadOnly] public bool isActive;

    [Header("SFX")]
    private AudioSource source;
    public AudioClip clip_Attach;

    [Header("Managers")]
    private PlayerPassivesManager pP;
    private PlayerComponentManager cM;
    private ModularCustomGravity playerGravity;
    private PlayerSoftSpeedCap softSpeedCap; 
    private PlayerGrabbable grabbable;
    private PlayerGrabbableSystem playerGrabbable;
    private Rigidbody rb; 

    private void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance;
    }

    private void Start()
    {
        playerGravity = pP.GetPassive<ModularCustomGravity>();
        softSpeedCap = pP.GetPassive<PlayerSoftSpeedCap>();
        grabbable = GetComponentInChildren<PlayerGrabbable>();
        playerGrabbable = pP.GetPassive<PlayerGrabbableSystem>();
        source = GetComponent<AudioSource>();
        rb = cM.playerRB; 
    }

    private void Update()
    {
        if (isActive && !playerGrabbable.isGrabbing)
        {
            Detach();
        }
    }

    private void FixedUpdate()
    {
        if (isActive)
        {
            playerGravity.DisableGravity();
            softSpeedCap.ResetPlayerVelocity();
            
            if (moveWithBar)
            {
                Vector3 movementDelta = transform.position - lastPos;
                rb.MovePosition(rb.position + movementDelta);
                lastPos = transform.position;
            }
        }
    }

    public void Attach()
    {
        if (!playerGrabbable.isGrabbing)
        {
            Detach();
            return;
        }

        lastPos = transform.position;
        PlaySFX(clip_Attach, false);
        ToggleActiveState(true); 
        grabbable.enabled = false; //Prevent regrab
        playerGravity.DisableGravity();
        softSpeedCap.ResetPlayerVelocity(); 
    }

    public void Detach()
    {
        ToggleActiveState(false); 
        PlaySFX(clip_Attach, false);
        playerGrabbable.ResetState();
        grabbable.enabled = true;

        if (IsLookingAtWall())
        {
            addForce_Wall.StartAddForce();
        }
        else
        {
            addForce_AwayFromWall.StartAddForce();
        }    
    }

    public bool IsLookingAtWall()
    {
        Vector3 playerLookDir = cM.transform_MainCamera.forward;
        Vector3 barForward = transform.forward.normalized; // Assuming this is facing *away* from the wall
        float dot = Vector3.Dot(playerLookDir, barForward);

        if (dot < -0.5f)
        {
            return true;
        }
        else
        {
            return false;
        }    
    }

    public void PlaySFX(AudioClip clip, bool loop)
    {
        source.clip = clip;
        source.loop = loop;
        source.Play();
    }

    public void ToggleActiveState(bool active)
    {
        isActive = active; 
    }
}
