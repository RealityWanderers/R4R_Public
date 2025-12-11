using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDebugSequence : MonoBehaviour
{
    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerPassivesManager pP;
    private PlayerInputManager pI; 
    [Header("Refs")]
    private PlayerCameraSteer cameraSteer;
    private ModularCustomGravity customGravity;

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance;
        pI = PlayerInputManager.Instance; 
    }

    private void Start()
    {
        customGravity = pP.GetPassive<ModularCustomGravity>();
        cameraSteer = pP.GetPassive<PlayerCameraSteer>(); 
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (pI.playerInput.Debug.DebugSequence.WasPressedThisFrame())
        {
            StartCoroutine(DoSequence());
        }
#endif
    }

    public IEnumerator DoSequence()
    {
        cM.playerRB.AddForce(cameraSteer.GetHorizontalCameraDirection() * 6, ForceMode.VelocityChange);
        cM.playerRB.AddForce(Vector3.up * 12, ForceMode.VelocityChange);
        //customGravity.TempDisableGravity(0.5f); 
        yield return new WaitForSeconds(0);
    }
}
