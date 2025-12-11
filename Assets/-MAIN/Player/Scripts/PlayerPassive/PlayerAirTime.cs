using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirTime : PlayerPassive
{
    [Header("Data")]
    [ReadOnly] public bool timerOn;
    [ReadOnly] public float totalAirTime;
    [ReadOnly] public bool landedFrame;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerPassivesManager pP; 
    [Header("Refs")]
    private ModularGroundedDetector groundedDetector;

    private void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance;
    }

    private void Start()
    {
        groundedDetector = pP.GetPassive<ModularGroundedDetector>(); 
    }

    void Update()
    {
        if (!groundedDetector.isGrounded && !timerOn)
        {
            StartTimer();
        }
        if (groundedDetector.isGrounded && timerOn)
        {
            StartCoroutine(SetLandingFrame());
        }
        //if (cM.playerWallGrind.wallRunning)
        //{
        //    ResetTimer();
        //}

        if (timerOn)
        {
            totalAirTime += Time.deltaTime;
        }
    }

    public void StartTimer()
    {
        timerOn = true;
    }

    public void StopTimer()
    {
        timerOn = false;
    }

    public void ResetTimer()
    {
        timerOn = false;
        totalAirTime = 0f;
    }

    public IEnumerator SetLandingFrame()
    {
        landedFrame = true;
        yield return null;
        landedFrame = false;
        ResetTimer();
    }

    public override void ResetPassive()
    {
        base.ResetPassive();
    }
}
