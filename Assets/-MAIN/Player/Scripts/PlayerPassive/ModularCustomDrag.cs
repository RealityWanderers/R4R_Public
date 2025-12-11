using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ModularCustomDrag : PlayerPassive
{
    [Header("Settings")]
    public float defaultDrag;
    public float airDragMulti; 
    [ReadOnly] private float currentDrag;
    [Header("Refs")]
    private PlayerPassivesManager pP; 
    private Rigidbody rb;
    private ModularGroundedDetector groundedDetector;

    private void Awake()
    {
        pP = PlayerPassivesManager.Instance;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        groundedDetector = pP.GetPassive<ModularGroundedDetector>(); 
        ResetDragToDefault(); 
    }

    private void Update()
    {
        //COULD BE CHANGED SO IT ONLY UPDATES WHENEVER THE GROUNDED STATE CHANGES.
        if (groundedDetector.isGrounded)
        {
            ChangeDrag(currentDrag); 
        }
        else
        {
            ChangeDrag(currentDrag * airDragMulti);
        }
    }

    [Button]
    public void ResetDragToDefault()
    {
        rb.linearDamping = defaultDrag;
        currentDrag = rb.linearDamping;
    }

    [Button]
    public void ChangeDrag(float dragAmount)
    {
        rb.linearDamping = dragAmount;
        currentDrag = rb.linearDamping; 
    }

    public override void ResetPassive()
    {
        base.ResetPassive();

        ResetDragToDefault(); 
    }
}
