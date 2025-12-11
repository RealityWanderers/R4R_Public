using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class PlayerModelAnimation : MonoBehaviour
{
    [Header("State")]
    [ReadOnly] public bool handClosed_L;
    [ReadOnly] public bool handClosed_R;

    public UnityEvent onOpenHand_L;
    public UnityEvent onCloseHand_L;
    public UnityEvent onOpenHand_R;
    public UnityEvent onCloseHand_R;

    [Header("Refs")]
    private PlayerComponentManager cM;
    private PlayerInputManager pI; 
    public Animator animator;

    void Start()
    {
        cM = PlayerComponentManager.Instance;
        pI = PlayerInputManager.Instance; 
    }

    public void Update()
    {
        if (!handClosed_L && pI.gripValue_L > 0.7f || !handClosed_L && pI.triggerValue_L > 0.7f)
        {
            CloseHand_L();
            handClosed_L = true; 
        }
        if (handClosed_L && pI.gripValue_L < 0.7f && handClosed_L && pI.triggerValue_L < 0.7f)
        {
            OpenHand_L();
            handClosed_L = false; 
        }

        if (!handClosed_R && pI.gripValue_R > 0.7f || !handClosed_R && pI.triggerValue_R > 0.7f)
        {
            CloseHand_R();
            handClosed_R = true;
        }
        if (handClosed_R && pI.gripValue_R < 0.7f && handClosed_R && pI.triggerValue_R < 0.7f)
        {
            OpenHand_R();
            handClosed_R = false;
        }
    }

    [Button]
    public void OpenHand_L()
    {
        animator.Play("Open_L", 1);
        onOpenHand_L?.Invoke();
    }

    [Button]
    public void CloseHand_L()
    {
        animator.Play("Close_L", 1);
        onCloseHand_L?.Invoke();
    }

    [Button]
    public void OpenHand_R()
    {
        animator.Play("Open_R", 2);
        onOpenHand_R?.Invoke();
    }

    [Button]
    public void CloseHand_R()
    {
        animator.Play("Close_R", 2);
        onCloseHand_R?.Invoke();
    }
}
