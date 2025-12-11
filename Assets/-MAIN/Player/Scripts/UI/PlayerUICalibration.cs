using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public struct UIElement
{
    public RectTransform rectTransform;
    public string headerText;
}

public class PlayerUICalibration : MonoBehaviour
{
    [Header("Settings")]
    public bool skipCalibration;

    [Header("Panels")]
    public List<UIElement> panels;

    [Header("TitleScreen")]
    public TitleScreen titleScreen;

    [Header("Pointer")]
    public GameObject nearFarInteractor;

    [Header("BasePanel")]
    public RectTransform basePanelTransform;
    public TextMeshProUGUI headerText;
    public RectTransform headerTransform;

    [Header("NavigationButton")]
    public RectTransform navigationButton_L;
    public TextMeshProUGUI navigationButtonText_L;
    public RectTransform navigationButton_R;
    public TextMeshProUGUI navigationButtonText_R;

    [Header("PlayerIKCalibration")]
    public Image calibrationProgressBarParent;
    public Tweener tween_ProgressBarScale;
    public Image calibrationProgressBar;
    public ParticleSystem particle_Confetti;
    public AudioClip sfx_Confetti;
    public float sfx_Delay_Confetti;
    [Range(0, 1)] public float volume_Confetti;
    [Space]
    public AudioClip sfx_IKSpawnIn;
    public float sfx_Delay_IKSpawnIn;
    [Range(0, 1)] public float volume_IKSpawnIn;
    [Space]
    public RectTransform calibrationCompleteCheckmark;

    [Header("PlayerHeight")] //Disabled for now
    public float playerHeightIncrement = 0.01f;
    public TextMeshProUGUI playerHeightText;

    [Header("SnapTurn")]
    public int snapTurnAngleIncrement = 15;
    public RectTransform turnTypeSelectHighlight_0;
    public RectTransform angleIncrementUI;
    public TextMeshProUGUI currentSnapTurnAngleText;

    [Header("SmoothTurn")]
    public float smoothTurnSpeedIncrement = 0.1f;
    public RectTransform turnTypeSelectHighlight_1;
    public RectTransform smoothTurnIncrementUI;
    public TextMeshProUGUI currentSmoothTurnSpeedText;

    [Header("Data")]
    [ReadOnly] public int currentPanel;

    [Header("Animations")]
    public float scaleInTime;
    public float scaleOutTime;

    [Header("Refs")]
    private PlayerIKCalibration playerIKCalibration;
    private PlayerSaveData playerSaveData;
    private PlayerPassivesManager pP;
    private PlayerAbilityManager pA;
    private PlayerSFX pSFX;
    private PlayerMusic pMusic;

    private void Awake()
    {
        playerSaveData = PlayerSaveData.Instance;
        pP = PlayerPassivesManager.Instance;
        pA = PlayerAbilityManager.Instance;
        pSFX = PlayerSFX.Instance;
        pMusic = PlayerMusic.Instance;
        playerIKCalibration = PlayerIKCalibration.Instance;
    }

    public void Start()
    {
        if (skipCalibration)
        {
            StartGame();
        }
        else
        {
            LoadPlayerPrefs();
            HideAllElements();
        }
    }

    public void HideAllElements()
    {
        UpdateNavigationButtonState(0, false);
        UpdateNavigationButtonState(1, false);
        HideCalibrationCompleteCheckmark();
        HideBackgroundPanel();
        DisableAllPanels();
        HidePointer();
    }

    public void LoadPlayerPrefs()
    {
        if (PlayerPrefs.GetInt("SnapTurnAngle") == 0)
        {
            playerSaveData.SetDefaultSnapTurnAngle(45);
        }

        if (PlayerPrefs.GetFloat("SmoothTurnSpeed") == 0)
        {
            playerSaveData.SetDefaultSmoothTurnSpeed(1);
        }

        LoadCurrentTurnType();
    }

    public void LoadFirstPanel()
    {
        if (!skipCalibration) //Ensure when skip is active no other scripts can active the panel. 
        {
            ShowBackgroundPanel();
            currentPanel = 0;
            EnablePanel(0);
            ShowPanel(currentPanel);
        }
    }

    public void LoadSecondPanel() //For when we want to skip the title screen. 
    {
        if (!skipCalibration)
        {
            ShowBackgroundPanel();
            currentPanel = 1;
            EnablePanel(1);
            ShowPanel(currentPanel);
        }
    }

    [Button]
    public void PreviousPanel()
    {
        if (currentPanel >= 1)
        {
            HidePanel(currentPanel);
            currentPanel--;
            ShowPanel(currentPanel);
        }
    }

    [Button]
    public void NextPanel()
    {
        if (currentPanel == panels.Count - 1)
        {
            //Debug.Log("StartGame");
            StartGame();
        }
        if (currentPanel < panels.Count - 1)
        {
            HidePanel(currentPanel);
            currentPanel++;
            ShowPanel(currentPanel);
        }
    }

    public void DisableAllPanels()
    {
        for (int i = 0; i < panels.Count; i++)
        {
            DisablePanel(i);
        }
    }

    public void DisablePanel(int currentPanel)
    {
        panels[currentPanel].rectTransform.gameObject.SetActive(false);
    }

    public void EnablePanel(int currentPanel)
    {
        panels[currentPanel].rectTransform.gameObject.SetActive(true);
    }

    public void HidePanel(int currentPanel)
    {
        panels[currentPanel].rectTransform.DOScale(new Vector3(0, 0, 0), scaleOutTime).OnComplete(() => DisablePanel(currentPanel));
    }

    public void ShowPanel(int currentPanel)
    {
        headerText.SetText(panels[currentPanel].headerText);

        panels[currentPanel].rectTransform.localScale = Vector3.zero;
        EnablePanel(currentPanel);
        panels[currentPanel].rectTransform.DOScale(new Vector3(1, 1, 1), scaleInTime);

        if (currentPanel == 0 || currentPanel == 1) //When at panel 0 / 1 (0 = title, 1 = is cali, 3 = more settings.)
        {
            UpdateNavigationButtonState(0, false);
        }

        if (currentPanel >= 2 && !navigationButton_L.gameObject.activeInHierarchy) //When at panel 2 (0 = title, 1 = is cali, 3 = more settings.)
        {
            UpdateNavigationButtonState(0, true);
        }

        if (currentPanel == panels.Count - 1) //When at the final panel set right navigator to GO!. 
        {
            navigationButtonText_R.SetText("GO!");
            StartUIPulse(navigationButton_R);
        }
        else //Otherwise reset text back to default Navigator Symbol
        {
            StopUIPulse(navigationButton_R);
            navigationButtonText_R.SetText(">");
        }
    }

    private Tween pulseTween;

    public void StartUIPulse(Transform target)
    {
        if (pulseTween == null) // Prevent duplicate loops
        {
            pulseTween = target.DOScale(Vector3.one * 1.1f, 0.3f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void StopUIPulse(Transform target)
    {
        if (pulseTween != null)
        {
            pulseTween.Kill(); // Stop the animation
            target.localScale = Vector3.one; // Reset to normal scale
            pulseTween = null;
        }
    }


    public void UpdateNavigationButtonState(int side, bool enable)
    {
        if (side == 0)
        {
            navigationButton_L.gameObject.SetActive(enable);
        }

        if (side == 1)
        {
            navigationButton_R.gameObject.SetActive(enable);
        }
    }




    public void UpdateCalibrationProgress(float progress)
    {
        calibrationProgressBar.fillAmount = progress;
    }

    public void HideCalibrationCompleteCheckmark()
    {
        if (tween_Checkmark != null) { tween_Checkmark.Kill(); }
        calibrationCompleteCheckmark.localScale = Vector3.zero;
    }

    private Tween tween_Checkmark;
    public void OnCalibrationComplete()
    {
        //SFX / VFX
        particle_Confetti.Emit(100);
        pSFX.PlaySFX(sfx_Confetti, volume_Confetti, sfx_Delay_Confetti);
        pSFX.PlaySFX(sfx_IKSpawnIn, volume_IKSpawnIn, sfx_Delay_IKSpawnIn);

        //Checkmark
        if (tween_Checkmark != null) { tween_Checkmark.Kill(); }
        Sequence sequence = DOTween.Sequence();
        tween_Checkmark = sequence;
        sequence.Append(calibrationCompleteCheckmark.DOScale(new Vector3(1f, 1f, 1f), 0.2f).SetEase(Ease.Linear));
        sequence.Append(calibrationCompleteCheckmark.DOPunchScale(new Vector3(0.45f, 0.45f, 0.45f), 0.4f, 1).SetEase(Ease.InOutExpo));

        //ProgressBar
        tween_ProgressBarScale.Kill();
        calibrationProgressBarParent.rectTransform.localScale = Vector3.one;
        tween_ProgressBarScale = calibrationProgressBarParent.rectTransform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f, 1);

        UpdateNavigationButtonState(1, true);
        ShowPointer();
    }

    public void EnableCalibration() //Called by UI panel to ensure you can only calibrate when needed. 
    {
        playerIKCalibration.canCalibrate = true;
    }

    public void DisableCalibration()
    {
        playerIKCalibration.canCalibrate = false;
    }




    public void DecreasePlayerHeight()
    {
        playerSaveData.EditHeight(-playerHeightIncrement);
        UpdatePlayerHeightValue();
    }

    public void IncreasePlayerHeight()
    {

        playerSaveData.EditHeight(playerHeightIncrement);
        UpdatePlayerHeightValue();
    }

    public void UpdatePlayerHeightValue()
    {
        playerHeightText.SetText(PlayerPrefs.GetFloat("PlayerHeight").ToString());
    }


    public void EnableTurning()
    {
        pP.EnablePassiveByType<PlayerSnapTurn>();
    }

    public void DisableTurning()
    {
        pP.DisablePassiveByType<PlayerSnapTurn>();
    }

    public void DecreaseSnapTurnAngle()
    {
        playerSaveData.EditSnapTurnAngle(-snapTurnAngleIncrement);
        currentSnapTurnAngleText.SetText(PlayerPrefs.GetInt("SnapTurnAngle").ToString());
    }

    public void IncreaseSnapTurnAngle()
    {
        playerSaveData.EditSnapTurnAngle(snapTurnAngleIncrement);
        currentSnapTurnAngleText.SetText(PlayerPrefs.GetInt("SnapTurnAngle").ToString());
    }

    public void DecreaseSmoothTurnSpeed()
    {
        playerSaveData.EditSmoothTurnSpeed(-smoothTurnSpeedIncrement);
        currentSmoothTurnSpeedText.SetText(PlayerPrefs.GetFloat("SmoothTurnSpeed").ToString());
    }

    public void IncreaseSmoothTurnSpeed()
    {
        playerSaveData.EditSmoothTurnSpeed(smoothTurnSpeedIncrement);
        currentSmoothTurnSpeedText.SetText(PlayerPrefs.GetFloat("SmoothTurnSpeed").ToString());
    }

    public void SetToSnapTurn()
    {
        playerSaveData.TurningTypeSnapTurn();
        ShowSnapTurnUI();
    }

    public void ShowSnapTurnUI()
    {
        turnTypeSelectHighlight_0.gameObject.SetActive(true);
        turnTypeSelectHighlight_1.gameObject.SetActive(false);
        angleIncrementUI.gameObject.SetActive(true);
        smoothTurnIncrementUI.gameObject.SetActive(false);
        currentSnapTurnAngleText.SetText(PlayerPrefs.GetInt("SnapTurnAngle").ToString());
    }

    public void SetToSmoothTurn()
    {
        playerSaveData.TurningTypeSmoothTurn();
        ShowSmoothTurnUI();
    }

    public void ShowSmoothTurnUI()
    {
        turnTypeSelectHighlight_1.gameObject.SetActive(true);
        turnTypeSelectHighlight_0.gameObject.SetActive(false);
        smoothTurnIncrementUI.gameObject.SetActive(true);
        angleIncrementUI.gameObject.SetActive(false);
        currentSmoothTurnSpeedText.SetText(PlayerPrefs.GetFloat("SmoothTurnSpeed").ToString());
    }

    public void LoadCurrentTurnType()
    {
        int currentTurnType = PlayerPrefs.GetInt("TurnType");
        if (currentTurnType == 0)
        {
            ShowSnapTurnUI();
        }
        if (currentTurnType == 1)
        {
            ShowSmoothTurnUI();
        }
    }

    public void TurnOffMusic()
    {
        pMusic.StopPlaying(0.3f); 
    }

    public void TurnOnMusic()
    {
        if (pMusic._MusicPlayerState == PlayerMusic.MusicPlayerState.stopped)
        {
            pMusic.PlayTrack(0);
        }
    }


    private Tween tween_ScaleBasePanel;
    public void HideBackgroundPanel()
    {
        if (tween_ScaleBasePanel != null) { tween_ScaleBasePanel.Kill(); }
        basePanelTransform.localScale = Vector3.one;
        tween_ScaleBasePanel = basePanelTransform.DOScale(Vector3.zero, 0.2f).OnComplete(() => basePanelTransform.gameObject.SetActive(false));
    }

    public void ShowBackgroundPanel()
    {
        if (tween_ScaleBasePanel != null) { tween_ScaleBasePanel.Kill(); }
        basePanelTransform.gameObject.SetActive(true); 
        basePanelTransform.localScale = Vector3.zero;
        tween_ScaleBasePanel = basePanelTransform.DOScale(Vector3.one, 0.2f);
    }



    public void HidePointer()
    {
        nearFarInteractor.SetActive(false);
    }

    public void ShowPointer()
    {
        nearFarInteractor.SetActive(true);
    }



    public void StartGame()
    {
        pP.EnableAllPassives();
        pA.EnableAllAbilities();
        HideAllElements();
        //titleScreen.ResetTitleScreen(); 
    }
}
