using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerActionChainData : PlayerAbility
{
    [Header("Settings")]
    public float actionChainDecayTime;
    public int maxActionChainComboMulti = 4;
    public float percentagePerChain;

    [Header("Data")]
    [ReadOnly] public bool actionChainActive;
    [ReadOnly] public int currentActionChainAmount;
    [ReadOnly] public float lastActionChainTimeStamp;
    [ReadOnly] public float currentActionChainMulti;

    [Header("Audio Chain")]
    public AudioSource sfx_SourceActionChain;
    public AudioClip sfx_Clip;
    [Range(0, 1)] public float sfx_Volume = 0.05f;
    private float sfx_CurrentPitch;
    public float sfx_MaxPitch;
    public float sfx_PitchIncrease;

    [Header("Audio CashIn")]
    public AudioClip sfx_ClipCashIn;
    [Range(0, 1)] public float sfx_VolumeCashIn = 0.15f;

    [Header("Managers")]
    private PlayerComponentManager cM;
    private PlayerAbilityManager pA;
    private PlayerPassivesManager pP; 
    private PlayerSFX sfx; 
    [Header("Refs")]
    private PlayerOverclockData overclockData;

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
        pA = PlayerAbilityManager.Instance;
        pP = PlayerPassivesManager.Instance; 
        sfx = PlayerSFX.Instance; 
    }

    private void Start()
    {
        overclockData = pP.GetPassive<PlayerOverclockData>();
    }

    public void Update()
    {
        if (actionChainActive)
        {
            if (Time.time > lastActionChainTimeStamp + actionChainDecayTime)
            {
                actionChainActive = false;
                EndActionChain();
            }
        }
    }

    public void RestActionChainTimeStamp()
    {
        lastActionChainTimeStamp = Time.time; //Can be called from other functions update that want to "pause" the action chain decay.
    }

    public void AddToChain()
    {
        if (currentActionChainAmount == 0)
        {
            cM.playerUIActionChain.ActionChainAppear();
            actionChainActive = true;
        }

        lastActionChainTimeStamp = Time.time;
        currentActionChainAmount++;

        if (currentActionChainAmount == 1)
        {
            currentActionChainMulti = 1;
        }
        if (currentActionChainAmount == 2)
        {
            currentActionChainMulti = 2;
        }
        if (currentActionChainAmount == 3)
        {
            currentActionChainMulti = 4;
        }
        if (currentActionChainAmount == 4)
        {
            currentActionChainMulti = 8;
        }

        cM.playerUIActionChain.UpdateCurrentChainText(currentActionChainAmount);

        PlaySFX(); 
    }

    public void EndActionChain()
    {
        float percentageToAdd = currentActionChainAmount * percentagePerChain;
        overclockData.UpdateOverclockPercentage(percentageToAdd);
        cM.playerUIActionChain.ActionChainHide();
        currentActionChainAmount = 0;
        ResetPitch();
        sfx.PlaySFX(sfx_ClipCashIn, sfx_VolumeCashIn);
    }

    void PlaySFX()
    {
        sfx_SourceActionChain.clip = sfx_Clip;
        sfx_SourceActionChain.pitch = sfx_CurrentPitch;
        sfx_SourceActionChain.volume = sfx_Volume;
        sfx_SourceActionChain.Play();
        IncrementPitch(); 
    }

    void IncrementPitch()
    {
        if (sfx_CurrentPitch != sfx_MaxPitch)
        {
            sfx_CurrentPitch += sfx_PitchIncrease;
        }
    }

    void ResetPitch()
    {
        sfx_CurrentPitch = 1;
    }

    public override void ResetAbility()
    {
        base.ResetAbility();

        currentActionChainAmount = 0;
        actionChainActive = false;
        ResetPitch(); 
    }
}
