using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;

public interface IPhonePanel
{
    void OnPanelShown();
    void OnPanelHidden();
}

public class PlayerPhone : MonoBehaviour
{
    [Header("Model")]
    //Animations such as flipping open the phone.

    [Header("UI")]
    public RectTransform rect_Panels;
    public RectTransform rect_BaseUI;
    private Vector3 defaultUIParentScale;
    public float scaleInTime;
    public Ease scaleInEase;

    [Header("Panels")]
    public List<PanelEntry> panelEntries;
    [System.Serializable]
    public class PanelEntry
    {
        public PhonePanelType panelType;
        public GameObject panelObject;
    }
    public enum PhonePanelType
    { None, Home, ChallengeList, MusicLibrary, Settings, Discord, PictureMode, Album }
    [ReadOnly] public PhonePanelType currentPanel = PhonePanelType.None;

    [Header("State")]
    [ReadOnly] public bool isPhoneActive;

    [Header("Haptic")]
    [PropertyRange(0, 1)]
    public float hapticAmplitude;
    [PropertyRange(0, 1)]
    public float hapticDuration;

    [Header("References")]
    private PlayerInputManager input;
    public static PlayerPhone Instance { get; private set; }

    private void Awake()
    {
        input = PlayerInputManager.Instance;

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        defaultUIParentScale = rect_Panels.localScale;
        DeActivatePhone();
    }

    public void Update()
    {
        if (input.playerInput.Left.StickClick.WasPerformedThisFrame())
        {
            if (!isPhoneActive)
            {
                ChangePhoneActiveState(true);
                //Debug.Log("OpenPhone");
            }
            else
            {
                ChangePhoneActiveState(false);
                //Debug.Log("ClosePhone");
            }
        }
    }

    private void ChangePhoneActiveState(bool desiredState)
    {
        if (desiredState == true && !isPhoneActive)
        {
            ActivatePhone();
        }

        if (desiredState == false && isPhoneActive)
        {
            DeActivatePhone();
        }
    }

    [Button]
    public void ShowPanel(PhonePanelType type)
    {
        HideAllPanels();
        currentPanel = type;

        foreach (var entry in panelEntries)
        {
            bool isTarget = entry.panelType == type;
            entry.panelObject.SetActive(isTarget);

            // Handle all IPhonePanel scripts under the object
            IPhonePanel[] panels = entry.panelObject.GetComponentsInChildren<IPhonePanel>(includeInactive: true);

            foreach (var panel in panels)
            {
                if (isTarget)
                    panel.OnPanelShown();
                else
                    panel.OnPanelHidden();
            }
        }
    }

    public void HideAllPanels()
    {
        foreach (var entry in panelEntries)
        {
            if (entry.panelObject.TryGetComponent<IPhonePanel>(out var panel))
            {
                panel.OnPanelHidden();
            }
            entry.panelObject.SetActive(false);
        }
    }

    private Tween scaleUI;
    [Button]
    public void ActivatePhone()
    {
        isPhoneActive = true;
        //Call Animation to open phone. 
        //Call SFX - Flip open + on Beep
        //Open UI - Monitor Turn on Animation

        ShowPanel(PhonePanelType.Home);

        scaleUI?.Kill();
        rect_Panels.localScale = Vector3.zero;
        scaleUI = rect_Panels.DOScale(defaultUIParentScale, scaleInTime).SetEase(scaleInEase);
        rect_BaseUI.gameObject.SetActive(true);
    }

    public void DeActivatePhone()
    {
        isPhoneActive = false;
        //Call Animation to open phone. 
        //Call SFX - Flip Close + Off Beep
        //Close UI - Monitor Turn Off Animation
        scaleUI?.Kill();
        rect_Panels.localScale = defaultUIParentScale;
        scaleUI = rect_Panels.DOScale(Vector3.zero, scaleInTime).SetEase(scaleInEase).OnComplete(() => HideAllPanels());
        rect_BaseUI.gameObject.SetActive(false);
        HideAllPanels();
        currentPanel = PhonePanelType.None; 
    }

    private void PhoneHaptic()
    {
        input.playerHaptic_L.SendHapticImpulse(hapticAmplitude, hapticDuration);
    }

}
