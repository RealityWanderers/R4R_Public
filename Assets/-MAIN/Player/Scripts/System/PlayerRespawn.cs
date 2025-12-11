using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Settings")]
    public float playerRespawnLockoutTime;

    //public List<MonoBehaviour> disableScriptsOnRespawnList; 

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerAbilityManager pA;
    private PlayerPassivesManager pP;
    private ObjectRespawnManager oR;
    private PlayerUIManager pU;
    private PlayerTeleporter teleporter; 

    public void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pA = PlayerAbilityManager.Instance;
        pP = PlayerPassivesManager.Instance;
        oR = ObjectRespawnManager.Instance;
        pU = PlayerUIManager.Instance;
        teleporter = PlayerTeleporter.Instance; 
    }

    [Button]
    public void ResetPlayer()
    {
        StartCoroutine(ResetPlayerCoroutine());
    }

    public IEnumerator ResetPlayerCoroutine()
    {
        yield return null; //Waits 1 frame.
        cM.playerRB.interpolation = RigidbodyInterpolation.None;
        //cM.disableSpecificScripts.DisableScriptsForDuration(disableScriptsOnRespawnList, playerRespawnLockoutTime);

        pA.ResetAllAbilities();
        pP.ResetAllPassives();
        oR.ResetAllResettableObjects();
        pU.ResetAllUI(); 

        if (!cM.playerRB.isKinematic) { cM.playerRB.linearVelocity = Vector3.zero; }
        teleporter.ResetPlayerPosition();

        yield return null; //Waits 1 frame.
        cM.playerRecenter.RecenterPlayer(true); 
        cM.playerRB.interpolation = RigidbodyInterpolation.Interpolate;
    }
}
