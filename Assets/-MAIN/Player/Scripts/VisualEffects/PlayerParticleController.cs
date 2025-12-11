using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParticleController : PlayerPassive
{
    [Header("Threshold")]
    public float tier1SpeedThreshold;
    public float tier2SpeedThreshold;

    [Header("SpeedLines")]
    public ParticleSystem tier_1;
    public ParticleSystem tier_2;

    [Header("SidedSpeedBurst")]
    public ParticleSystem smallParticleBurst_L;
    public ParticleSystem smallParticleBurst_R;

    [Header("BigSpeedBurst")]
    public ParticleSystem bigParticleBurstLooping;

    [Header("BoostRing")]
    public ParticleSystem boostRing;

    [Header("RailGrind")]
    public ParticleSystem particle_RailGrindSpeedLines;
    public float setting_RailGrindMinSize = 0.005f;
    public float setting_RailGrindMaxSize = 0.0125f;

    [Header("Data")]
    [ReadOnly] public int currentTier;
    [ReadOnly] public int previousTier;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerPassivesManager pP; 
    [Header("Refs")]
    private PlayerSoftSpeedCap softSpeedCap;

    public void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance;
    }

    private void Start()
    {
        softSpeedCap = pP.GetPassive<PlayerSoftSpeedCap>();  
    }

    void Update()
    {
        previousTier = currentTier;

        float currentSpeedPercentage = softSpeedCap.GetSpeedPercentage();
        if (currentSpeedPercentage < tier1SpeedThreshold)
        {
            currentTier = 0;
        }

        if (currentSpeedPercentage > tier1SpeedThreshold)
        {
            currentTier = 1;
        }

        if (currentSpeedPercentage > tier2SpeedThreshold)
        {
            currentTier = 2;
        }

        if (currentTier == 0 && currentTier != previousTier)
        {
            tier_1.Stop();
            tier_2.Stop();
        }

        if (currentTier == 1 && currentTier != previousTier)
        {
            tier_2.Stop();
            tier_1.Play();
        }

        if (currentTier == 2 && currentTier != previousTier)
        {
            tier_1.Stop();
            tier_2.Play();
        }

        if (currentSpeedPercentage > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(cM.playerRB.linearVelocity);
            transform.position = new Vector3(transform.position.x, cM.transform_MainCamera.position.y, transform.position.z);
        }

        //if (cM.playerRailGrind.isRailGrinding)
        //{
        //    transform.forward = -cM.transform_MainCamera.forward;
        //    particle_RailGrindSpeedLines.startSize = Mathf.Lerp(setting_RailGrindMinSize, setting_RailGrindMaxSize, cM.playerRailGrind.speedPercentage);     
        //}
    }

    public void PlaySmallBurst_L()
    {
        smallParticleBurst_L.Play();
    }

    public void PlaySmallBurst_R()
    {
        smallParticleBurst_R.Play();
    }

    public void PlayBigBurstLooping()
    {
        bigParticleBurstLooping.Play();
    }

    public void StopBigBurstLooping()
    {
        bigParticleBurstLooping.Stop();
    }

    public void PlayBoostRing()
    {
        boostRing.Play();
    }

    public void StartRailGrindSpeedLines()
    {
        particle_RailGrindSpeedLines.Play(); 
    }

    public void StopRailGrindSpeedLines()
    {
        particle_RailGrindSpeedLines.Stop();
    }

    public override void ResetPassive()
    {
        base.ResetPassive();

        StopRailGrindSpeedLines();
    }
}
