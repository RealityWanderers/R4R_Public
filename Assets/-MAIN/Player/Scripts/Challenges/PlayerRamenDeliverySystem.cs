using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRamenDeliverySystem : MonoBehaviour
{
    [Header("Controls")]
    private bool ramenBoxInHand_L;
    private bool ramenBoxInHand_R;

    private GameObject heldRamenBox_L;
    private GameObject heldRamenBox_R;

    private bool holdingGrabInput_L;
    private bool holdingGrabInput_R;

    [Header("RamenBox")]
    public GameObject ramenBoxPrefab;
    public List<GameObject> spawnedRamenBoxes; 

    [Header("Throw")]
    public Vector2 throwMinMaxSpeed; 
    public float throwHomingStrength;
    public float forceMultiplier;
    private PlayerRamenDeliveryPoint currentDeliveryPoint;
    private ControllerPositionBuffer positionBuffer_L = new ControllerPositionBuffer();
    private ControllerPositionBuffer positionBuffer_R = new ControllerPositionBuffer();
    [Header("Throw Timing")]
    [Range(1, 59)] public int throwStartFrameOffset = 20;
    [Range(1, 59)] public int throwEndFrameOffset = 4;
    [ReadOnly] public int thrownBoxesAmount; 

    [Header("References")]
    private PlayerInputManager input;
    private PlayerPassivesManager pP;
    private PlayerComponentManager cM;
    private PlayerLockOnSystemRamenDelivery lockOnSystem;
    private PlayerSoftSpeedCap softSpeedCap;

    public static PlayerRamenDeliverySystem Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }

        input = PlayerInputManager.Instance;
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance;

        positionBuffer_L = new ControllerPositionBuffer();
        positionBuffer_R = new ControllerPositionBuffer();
    }

    private void Start()
    {
        lockOnSystem = pP.GetPassive<PlayerLockOnSystemRamenDelivery>();
        softSpeedCap = pP.GetPassive<PlayerSoftSpeedCap>(); 
    }

    private void Update()
    {
        positionBuffer_L.AddPosition(cM.transform_Controller_L.position);
        positionBuffer_R.AddPosition(cM.transform_Controller_R.position);

        // Left hand grab
        if (!holdingGrabInput_L && input.triggerValue_L > 0.7f)
        {
            holdingGrabInput_L = true;

            if (!ramenBoxInHand_L)
                SpawnRamenBox(cM.transform_Controller_L, true);
        }

        // Right hand grab
        if (!holdingGrabInput_R && input.triggerValue_R > 0.7f)
        {
            holdingGrabInput_R = true;

            if (!ramenBoxInHand_R)
                SpawnRamenBox(cM.transform_Controller_R, false);
        }

        // Left hand release
        if (holdingGrabInput_L && input.gripValue_L < 0.7f && input.triggerValue_L < 0.7f)
        {
            holdingGrabInput_L = false;

            if (ramenBoxInHand_L)
            {
                ThrowRamenBox(true);
            }
        }

        // Right hand release
        if (holdingGrabInput_R && input.gripValue_R < 0.7f && input.triggerValue_R < 0.7f)
        {
            holdingGrabInput_R = false;

            if (ramenBoxInHand_R)
            {
                ThrowRamenBox(false);
            }
        }
    }

    public void SpawnRamenBox(Transform handParent, bool isLeftHand)
    {
        GameObject ramenBoxInstance = Instantiate(ramenBoxPrefab, handParent);
        spawnedRamenBoxes.Add(ramenBoxInstance);

        ramenBoxInstance.transform.localPosition = Vector3.zero;
        ramenBoxInstance.transform.localRotation = Quaternion.identity;

        Rigidbody rb = ramenBoxInstance.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;

        if (isLeftHand)
        {
            ramenBoxInHand_L = true;
            heldRamenBox_L = ramenBoxInstance;
        }
        else
        {
            ramenBoxInHand_R = true;
            heldRamenBox_R = ramenBoxInstance;
        }
    }

    public void ThrowRamenBox(bool isLeftHand)
    {
        if (isLeftHand && !ramenBoxInHand_L) return;
        if (!isLeftHand && !ramenBoxInHand_R) return;

        if (isLeftHand) ramenBoxInHand_L = false;
        else ramenBoxInHand_R = false;

        ControllerPositionBuffer buffer = isLeftHand ? positionBuffer_L : positionBuffer_R;
        GameObject ramenBoxInstance = isLeftHand ? heldRamenBox_L : heldRamenBox_R; if (ramenBoxInstance == null) return;
        if (isLeftHand) heldRamenBox_L = null;
        else heldRamenBox_R = null;

        ramenBoxInstance.transform.parent = null;

        Rigidbody rb = ramenBoxInstance.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = false;

        RamenBox ramenBoxScript = ramenBoxInstance.GetComponent<RamenBox>();

        Vector3 throwStart = buffer.GetPositionFramesAgoWorld(throwStartFrameOffset);
        Vector3 throwEnd = buffer.GetPositionFramesAgoWorld(throwEndFrameOffset);
        Vector3 throwDir = (throwEnd - throwStart).normalized;
        float throwMagnitude = (throwEnd - throwStart).magnitude / ((throwStartFrameOffset - throwEndFrameOffset) * Time.fixedDeltaTime);
        //Debug.Log(throwMagnitude);
        throwMagnitude = Mathf.Clamp(throwMagnitude, throwMinMaxSpeed.x, throwMinMaxSpeed.y); 
        float adjustedForceMulti = forceMultiplier * softSpeedCap.GetSpeedPercentage(); //As we increase our speed we need more throw velocity to match.
        Vector3 launchVelocity = throwDir * throwMagnitude * forceMultiplier;

        thrownBoxesAmount++; 

        currentDeliveryPoint = lockOnSystem.isLockedOn ? lockOnSystem.ramenDeliveryPoint : null;
        if (currentDeliveryPoint != null)
        {
            ramenBoxScript.Launch(throwEnd,
                                  launchVelocity,
                                  currentDeliveryPoint.transform,
                                  throwHomingStrength,
                                  () =>
                                  {
                                      lockOnSystem.ResetLockOn();
                                      currentDeliveryPoint.OnCatch();
                                  });
        }
        else
        {
            ramenBoxScript.Launch(throwEnd, launchVelocity);
        }
    }

    public void ChallengeStart()
    {
        ResetDeliverySystemState(); 
    }

    public void ResetDeliverySystemState()
    {
        thrownBoxesAmount = 0;
        foreach (var ramenBox in spawnedRamenBoxes)
        {
            if (ramenBox != null)
            {
                Destroy(ramenBox);
            }
        }
        spawnedRamenBoxes.Clear(); 
    }
}
