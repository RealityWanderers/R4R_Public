using UnityEngine;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;

public class PlayerUIActionChain : PlayerUI
{
    [Header("Settings Appear")]
    public Vector3 appearScale; 
    public float appearDuration;
    public float textRandomRotationAmount = 20; 

    [Header("Settings Chain")]
    public Vector3 textPunchScale;
    public float textPunchScaleDuration = 0.4f;
    public int textPunchScaleVibrato = 2;
    public float chainToScaleFactor;
    public float maxChainScale = 2; 

    [Header("Refs")]
    public TextMeshProUGUI text_CurrentChain;
    public RectTransform actionChainUIRoot;
    private Tweener tween_PunchScale; 
    private PlayerComponentManager cM;

    void Awake()
    {
        cM = PlayerComponentManager.Instance;
    }

    [Button]
    public void UpdateCurrentChainText(int currentChain)
    {
        text_CurrentChain.rectTransform.DOLocalRotate(new Vector3(0, 0, 0), 0);
        text_CurrentChain.rectTransform.DOLocalRotate(new Vector3(0, 0, Random.Range(-textRandomRotationAmount, textRandomRotationAmount)), 0.05f); 
        if (tween_PunchScale != null) tween_PunchScale.Kill(); //Ensures punch scale is killed as this overwrites scale as long as it's active. 
        float scaleToSet = 1 + (chainToScaleFactor * currentChain); //Set scale to one + a small increase per chain.
        scaleToSet = Mathf.Clamp(scaleToSet, 1, maxChainScale); //Clamp to a max to ensure it does not get too crazy.
        text_CurrentChain.rectTransform.localScale = new Vector3(scaleToSet, scaleToSet, scaleToSet); //Apply scale
        tween_PunchScale = text_CurrentChain.rectTransform.DOPunchScale(textPunchScale, textPunchScaleDuration, textPunchScaleVibrato); //Do a punch scale.
        if (currentChain < 10) 
        {
            text_CurrentChain.SetText("0" + currentChain.ToString()); //If we are below 10 append a 0 for a stylized look, 01, 02, etc. 
        }
        else
        {
            text_CurrentChain.SetText(currentChain.ToString()); //If we are above 10 just use the regular number.
        }
    }

    [Button]
    public void ActionChainAppear()
    {
        actionChainUIRoot.DOScale(appearScale, appearDuration);   
    }

    [Button]
    public void ActionChainHide()
    {
        actionChainUIRoot.DOScale(Vector3.zero, appearDuration);
        cM.playerUIOverclock.DoPunchScale(); 
    }

    public override void ResetUI()
    {
        base.ResetUI();
        ActionChainHide();
    }
}
