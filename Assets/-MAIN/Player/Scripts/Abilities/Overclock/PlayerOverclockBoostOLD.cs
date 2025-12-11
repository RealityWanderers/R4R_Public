//using Sirenix.OdinInspector;
//using System.Collections;
//using UnityEngine;
//using DG.Tweening;

//public class PlayerOverclockBoostOLD : MonoBehaviour
//{
//    [Header("Data")]
//    [ReadOnly] public bool canOverclock;
//    [ReadOnly] public bool isOverclocked;
//    [ReadOnly] public float savedPlayerSpeed;

//    [Header("Duration")]
//    public float overclockDuration;

//    //[Header("ForceRegular")]
//    //public float overclockStartForce = 3;
//    //public float overclockContinuousForce;

//    //[Header("ForceRailGrind")]
//    //public float overclockStartForceRailGrind = 9;
//    //public float overclockContinuousForceRailGrind = 0.1f;

//    [Header("Force")]
//    [ReadOnly] public float currentOverclockForce;
//    public float overclockForce;
//    public Ease easeType;
//    [Range(0, 1)] public float endSpeedSlowMulti = 0.8f;

//    [Header("Cooldown")]
//    public float cooldownDuration = 1;
//    public float speedCapMulti;
//    public float freezeFrameDuration = 0.1f;

//    [Header("Refs")]
//    private ComponentManager cM;
//    private Tweener tween_CurrentOverclockSpeed;

//    void Awake()
//    {
//        cM = ComponentManager.Instance;
//    }

//    void Update()
//    {
//        if (canOverclock)
//        {
//            if (cM.playerInputManager.input_Secondary_R.action.WasPressedThisFrame())
//            {
//                TryStartOverclock();
//            }

//            if (cM.playerInputManager.input_Primary_R.action.WasPressedThisFrame())
//            {
//                FillBar();
//            }
//        }
//    }

//    void FillBar()
//    {
//        cM.playerOverclockData.UpdateOverclockPercentage(1);
//    }

//    void FixedUpdate()
//    {
//        if (isOverclocked) // When overclocked add continuous speed + particle loop. 
//        {
//            if (cM.playerRailGrind.isRailGrinding)
//            {
//                cM.playerRailGrind.currentMoveSpeed += currentOverclockForce * Time.fixedDeltaTime;
//            }
//            else
//            {
//                Vector3 direction = cM.playerRB.linearVelocity.normalized;
//                cM.playerRB.AddForce(direction * currentOverclockForce, ForceMode.VelocityChange);
//            }
//        }
//    }

//    [Button]
//    void TryStartOverclock()
//    {
//        if (cM.playerOverclockData.currentReadySegments >= 1)
//        {
//            canOverclock = false;
//            cM.playerOverclockData.UseOverclockSegment();
//            StartCoroutine(StartOverclockRoutine());
//        }
//        else
//        {
//            //SFX for not enough segments.
//        }
//    }

//    IEnumerator StartOverclockRoutine()
//    {
//        //Freeze Frame


//        //Effects
//        cM.playerSpeedLineController.PlayBigBurst();
//        //cm.playerSpeedLineController.PlayLoop(); 
//        //Source.Play(SFX); 
//        //cm.PlayerMusic.DistortMusic(overclockDuration, distortionAmount) >>> this should be in a global system with it's own reset.

//        savedPlayerSpeed = cM.playerRB.linearVelocity.magnitude;

//        //Increase Speed Cap
//        cM.playerSoftSpeedCap.IncreaseSpeedCapWithDuration(speedCapMulti, overclockDuration);
//        cM.playerRailGrind.IncreaseSpeedCapWithDuration(speedCapMulti, overclockDuration);

//        //Set to velocity Speed Cap + initial boost.

//        if (cM.playerRailGrind.isRailGrinding)
//        {
//            StartCoroutine(cM.playerRailGrindCornerBehaviour.StartRailGrindFreezeFrame(freezeFrameDuration));
//            yield return new WaitForSeconds(freezeFrameDuration);

//            cM.playerRailGrind.currentMoveSpeed = cM.playerRailGrind.currentSoftSpeedCap;
//        }
//        else
//        {
//            cM.playerFreezeFrames.StartFreezeFrame(freezeFrameDuration);
//            yield return new WaitForSeconds(freezeFrameDuration);

//            if (cM.playerWallGrind.isWallGrinding)
//            {
//                Vector3 direction = cM.playerRB.linearVelocity.normalized; //Use velocity as that is aligned with the wall. 
//                cM.playerRB.linearVelocity = direction * cM.playerSoftSpeedCap.currentSoftSpeedCap;
//                //cM.playerRB.AddForce(direction * overclockStartForce, ForceMode.VelocityChange);
//            }
//            else
//            {
//                Vector3 direction = cM.playerCameraSteer.GetHorizontalCameraDirection();
//                cM.playerRB.linearVelocity = direction * cM.playerSoftSpeedCap.currentSoftSpeedCap;
//                //cM.playerRB.AddForce(direction * overclockStartForce, ForceMode.VelocityChange);
//            }
//        }

//        StartOverclock();
//        yield return new WaitForSeconds(overclockDuration);
//        StopOverclock();
//        StartCoroutine(StartCoolDown());
//    }

//    void StartOverclock()
//    {
//        isOverclocked = true;
//        currentOverclockForce = 0;
//        if (tween_CurrentOverclockSpeed != null) { tween_CurrentOverclockSpeed.Kill(); }
//        tween_CurrentOverclockSpeed = DOTween.To(() => currentOverclockForce, x => currentOverclockForce = x, overclockForce, overclockDuration * 0.2f);

//    }

//    void StopOverclock()
//    {
//        isOverclocked = false;
//        currentOverclockForce = 0;
//        if (tween_CurrentOverclockSpeed != null) { tween_CurrentOverclockSpeed.Kill(); }
//        float speedToSet = Mathf.Lerp(cM.playerSoftSpeedCap.currentPlayerSpeed, savedPlayerSpeed, endSpeedSlowMulti);
//        cM.playerRB.linearVelocity = cM.playerCameraSteer.GetHorizontalCameraDirection() * speedToSet;
//    }

//    IEnumerator StartCoolDown()
//    {
//        canOverclock = false;
//        yield return new WaitForSeconds(cooldownDuration);
//        canOverclock = true;
//    }

//    public void ResetScript()
//    {
//        StopAllCoroutines();
//        canOverclock = true;
//        isOverclocked = false;
//    }
//}
