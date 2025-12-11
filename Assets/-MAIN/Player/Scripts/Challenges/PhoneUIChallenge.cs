using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using DG.Tweening; 

public class PhoneUIChallenge : MonoBehaviour
{
    [Header("Scroll")]
    public Vector2 scrollMinMax = new Vector2(-7, 7);
    [ReadOnly] public int currentScrollIndex;
    private float scrollCooldown = 0.2f;
    private float scrollTimer = 0f;
    public RectTransform scrollIndicatorTransform;

    [Header("Arrows")]
    public Vector3 punchScaleScrollArrow = new Vector3(1.02f, 1.02f, 1.02f);
    public float punchScaleDurationArrow = 0.2f;
    public Ease punchScaleEase; 
    public RectTransform scrollArrowLeft;
    public RectTransform scrollArrowRight;

    [Header("References")]
    private PlayerPhone playerPhone;
    private PlayerChallengeSystem playerChallengeSystem;
    private PlayerInputManager input; 

    [Header("BestTime")]
    public TextMeshProUGUI bestTime;

    [Header("ChallengeNumber")]
    public TextMeshProUGUI text_challengeNumber;

    [Header("ChallengeInfoPanel")]
    public RectTransform rect_ChallengeInfo;

    [Header("Go Button")]
    public RectTransform rect_GoButton;
    public Vector3 scale_GoButton;
    public float duration_GoButton;

    [Header("S+ Colours")]
    public Color[] sPlusColors = new Color[]
    {
    Color.red, Color.magenta, Color.blue, Color.cyan, Color.green, Color.yellow, Color.red
    };
    public float colorTweenDuration = 0.5f;

    [Header("ChallengeType")]
    public List<ChallengeTypeImage> challengeTypeImages;
    [System.Serializable]
    public class ChallengeTypeImage
    {
        public ChallengeSaveEntry.ChallengeType challengeType;
        public GameObject challengeImage;
    }

    [Header("RankImages")]
    public List<ChallengeRankImage> rankImages;
    [System.Serializable]
    public class ChallengeRankImage
    {
        public ChallengeSaveEntry.Rank rank;
        public GameObject rankImage;
    }

    public void Awake()
    {
        playerChallengeSystem = PlayerChallengeSystem.Instance;
        input = PlayerInputManager.Instance;
        playerPhone = PlayerPhone.Instance; 
    }

    private void Update()
    {
        if (playerPhone.currentPanel == PlayerPhone.PhonePanelType.ChallengeList)
        {
            scrollTimer -= Time.deltaTime;

            if (scrollTimer <= 0f)
            {
                if (input.stickAxis_X_L <= -0.5f && currentScrollIndex != playerChallengeSystem.challenges.Count - 1)
                {
                    ChangeScrollIndex(1);
                    AnimateScrollArrow(scrollArrowRight);
                    AnimateNextChallengeInfo(false);
                    scrollTimer = scrollCooldown;
                }
                else if (input.stickAxis_X_L >= 0.5f && currentScrollIndex != 0)
                {
                    ChangeScrollIndex(-1);
                    AnimateScrollArrow(scrollArrowLeft);
                    AnimateNextChallengeInfo(true);     
                    scrollTimer = scrollCooldown;
                }
            }

            if (currentChallengeManager != null && input.playerInput.Left.Secondary.WasPerformedThisFrame())
            {
                StartChallenge(); 
            }
        }
    }

    public void OnEnable() //Refresh when activating the panel.
    {
        LoadChallengeData(currentScrollIndex);
    }

    private Tween tween_punchScaleArrow; 
    public void AnimateScrollArrow(RectTransform arrow)
    {
        tween_punchScaleArrow?.Kill(); 
        arrow.localScale = Vector3.one;
        tween_punchScaleArrow = arrow.DOPunchScale(punchScaleScrollArrow, punchScaleDurationArrow).SetEase(punchScaleEase); 
    }

    [Button]
    public void ChangeScrollIndex(int value)
    {
        currentScrollIndex += value;
        currentScrollIndex = Mathf.Clamp(currentScrollIndex, 0, playerChallengeSystem.challenges.Count - 1);
        LoadChallengeData(currentScrollIndex);

        float scrollProgress = playerChallengeSystem.challenges.Count > 1
            ? (float)currentScrollIndex / (playerChallengeSystem.challenges.Count - 1)
            : 0f; // Fix for divide by zero if there is only one challenge.
        float xPos = Mathf.Lerp(scrollMinMax.x, scrollMinMax.y, scrollProgress);
        scrollIndicatorTransform.DOAnchorPos(new Vector2(xPos, scrollIndicatorTransform.anchoredPosition.y), 0.1f, true);
    }

    public Sequence sequence_NextChallengeInfo; 
    [Button]
    public void AnimateNextChallengeInfo(bool leftScroll)
    {
        sequence_NextChallengeInfo?.Kill();
        sequence_NextChallengeInfo = DOTween.Sequence();
        
        float dir = leftScroll ? 1 : -1;

        sequence_NextChallengeInfo
            .Append(rect_ChallengeInfo.DOAnchorPosX(0, 0f, true))
            .Append(rect_ChallengeInfo.DOAnchorPosX(40 * dir, 0.1f, true))
            .Append(rect_ChallengeInfo.DOAnchorPosX(-40 * dir, 0f, true))
            .Append(rect_ChallengeInfo.DOAnchorPosX(0, 0.1f, true));
    }

    private Tween tween_RankColorLoop;
    public void ShowRank(ChallengeSaveEntry.Rank type)
    {
        tween_RankColorLoop?.Kill();

        foreach (var entry in rankImages)
        {
            bool isActive = entry.rank == type;
            entry.rankImage.SetActive(isActive);

            if (isActive && type == ChallengeSaveEntry.Rank.S_Plus)
            {
                var image = entry.rankImage.GetComponent<Image>();
                if (image != null && sPlusColors.Length > 1)
                {
                    tween_RankColorLoop = image.DOGradientColor(CreateRainbowGradient(), colorTweenDuration)
                        .SetLoops(-1, LoopType.Yoyo);
                }
            }
        }
    }

    private Gradient CreateRainbowGradient()
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[sPlusColors.Length];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[sPlusColors.Length];

        for (int i = 0; i < sPlusColors.Length; i++)
        {
            float time = (float)i / (sPlusColors.Length - 1);
            colorKeys[i] = new GradientColorKey(sPlusColors[i], time);
            alphaKeys[i] = new GradientAlphaKey(1f, time);
        }

        gradient.SetKeys(colorKeys, alphaKeys);
        return gradient;
    }

    public void ShowChallengeType(ChallengeSaveEntry.ChallengeType type)
    {
        foreach (var entry in challengeTypeImages)
        {
            entry.challengeImage.SetActive(entry.challengeType == type);
        }
    }

    private ChallengeManager currentChallengeManager; 
    public void LoadChallengeData(int index)
    {
        var challengeList = playerChallengeSystem.challenges;

        if (challengeList == null || challengeList.Count == 0)
        {
            Debug.LogWarning("No challenges found!");
            return;
        }

        if (index < 0 || index >= challengeList.Count)
        {
            Debug.LogWarning("Scroll index out of bounds: " + index);
            return;
        }

        string challengeID = challengeList[index].challengeManager.challengeID;

        var data = ChallengeSaveManager.Instance.savedEntries.Find(entry => entry.challengeID == challengeID);

        if (data == null)
        {
            Debug.LogWarning($"No saved data found for challenge ID: {challengeID}");
            return;
        }

        ShowRank(data.bestRank);
        SetChallengeNumber();
        ShowChallengeType(data.challengeType);
        AnimateGoButtonScale(); 

        bestTime.text = data.bestTime < 0
            ? "----"
            : data.bestTime.ToString("F2") + "s";

        currentChallengeManager = challengeList[index].challengeManager; 
    }

    public void SetChallengeNumber()
    {
        if (currentScrollIndex + 1 < 10)
        {
            text_challengeNumber.SetText("0" + (currentScrollIndex + 1).ToString());
        }
        else
        {
            text_challengeNumber.SetText((currentScrollIndex + 1).ToString());
        }   
    }

    private Sequence sequence_GoButton; 
    public void AnimateGoButtonScale()
    {
        sequence_GoButton?.Kill();
        sequence_GoButton = DOTween.Sequence();
        sequence_GoButton
            .Append(rect_GoButton.DOScale(scale_GoButton, duration_GoButton))
            .Append(rect_GoButton.DOScale(Vector3.one, duration_GoButton))
            .SetLoops(-1); 
    }

    public void StartChallenge()
    {
        if (currentChallengeManager == null) { return; }
        currentChallengeManager.OnChallengeStart(); 
    }
}
