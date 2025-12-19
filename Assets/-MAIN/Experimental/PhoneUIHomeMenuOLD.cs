using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class PhoneUIHomeMenuOLD : MonoBehaviour
{
    public enum MenuActionType { None, OpenChallengeList, OpenMusic, OpenSettings, OpenDiscord, OpenPictureMode, OpenAlbum}
    [System.Serializable]
    public class MenuEntry
    {
        public string menuEntryName;
        public Sprite menuSprite;
        public Color iconColor;
        public MenuActionType actionType;
        [HideInInspector] public RectTransform rectTransform;
    }

    [Header("Highlight")]
    public Vector3 highlightScale;
    public float highlightScaleTime;
    public Ease highlightEaseType;
    public float highlightRotation = 15; 

    [Header("Icons")]
    public List<MenuEntry> menuEntries;
    public GameObject iconPrefab;
    public float menuRadius;
    public RectTransform menuParent;

    [Header("Pointer")]
    public RectTransform pointerTransform;
    public float pointerScaleTime;
    public Ease pointerEaseType;

    [Header("Deadzone")]
    [Range(0, 1)] public float deadZone = 0.7f;

    [Header("Data")]
    [ReadOnly] public int menuOptionAmount;
    [ReadOnly] public float stickAngle;
    [ReadOnly] public int lastSelectedMenuIndex;
    [ReadOnly] public int selectedMenuIndex;

    [Header("References")]
    private PlayerInputManager input;
    private PlayerPhone playerPhone;

    private void Awake()
    {
        input = PlayerInputManager.Instance;
        playerPhone = PlayerPhone.Instance;
    }

    void Update()
    {
        if (playerPhone.currentPanel != PlayerPhone.PhonePanelType.Home) { return; }

        Vector2 stickInput = new Vector2(input.stickAxis_X_L, input.stickAxis_Y_L);

        if (stickInput.magnitude > deadZone)
        {
            float angle = Mathf.Atan2(stickInput.x, stickInput.y) * Mathf.Rad2Deg;
            angle = (360f - angle + 360f) % 360f;
            stickAngle = angle;

            float sliceAngle = 360f / menuOptionAmount;

            // This offset makes selection based on the center of each slice instead of edge boundaries
            float adjustedAngle = (stickAngle + sliceAngle / 2f) % 360f;

            selectedMenuIndex = Mathf.FloorToInt(adjustedAngle / sliceAngle);

            float snappedAngle = selectedMenuIndex * sliceAngle;
            pointerTransform.localRotation = Quaternion.Euler(0, 0, -snappedAngle);

            ShowPointer();
        }
        else
        {
            if (selectedMenuIndex != -1)
            {
                //Debug.Log(selectedMenuIndex);
                OnMenuSelect(selectedMenuIndex);
            }

            selectedMenuIndex = -1;
            UnHighlightAllIcons();
            HidePointer();
        }
        //New Select
        if (lastSelectedMenuIndex != selectedMenuIndex && selectedMenuIndex != -1)
        {
            HighlightIcon();
            //Debug.Log("NewSelect");
        }

        lastSelectedMenuIndex = selectedMenuIndex;
    }

    private void OnEnable() //Refreshes when the menu becomes active to ensure proper starting state.
    {
        selectedMenuIndex = -1;
    }

    [Button]
    public void OnMenuSelect(float selectedMenuIndex)
    {
        if (selectedMenuIndex < 0 || selectedMenuIndex >= menuEntries.Count)
            return;

        var selectedEntry = menuEntries[(int)selectedMenuIndex];

        switch (selectedEntry.actionType)
        {
            case MenuActionType.OpenChallengeList:
                playerPhone.ShowPanel(PlayerPhone.PhonePanelType.ChallengeList);
                break;

            case MenuActionType.OpenMusic:
                playerPhone.ShowPanel(PlayerPhone.PhonePanelType.MusicLibrary);
                break;

            case MenuActionType.OpenSettings:
                playerPhone.ShowPanel(PlayerPhone.PhonePanelType.Settings);
                break;

            case MenuActionType.OpenDiscord:
                playerPhone.ShowPanel(PlayerPhone.PhonePanelType.Discord);
                break;

            case MenuActionType.OpenPictureMode:
                playerPhone.ShowPanel(PlayerPhone.PhonePanelType.PictureMode);
                break;

            case MenuActionType.OpenAlbum:
                playerPhone.ShowPanel(PlayerPhone.PhonePanelType.Album);
                break;

            case MenuActionType.None:
            default:
                Debug.LogWarning("No action assigned to this menu entry!");
                break;
        }
    }

    [Button]
    public void SpawnIcons() //USED IN EDITOR, NOT IN GAME.
    {
        ClearIcons();
        menuOptionAmount = menuEntries.Count;

        for (int i = 0; i < menuOptionAmount; i++)
        {
            float angle = (360f / menuOptionAmount) * i;
            float angleRad = angle * Mathf.Deg2Rad;

            float x = menuRadius * Mathf.Sin(angleRad);
            float y = menuRadius * Mathf.Cos(angleRad);

            Vector2 iconPos = new Vector2(x, y);

            // Instantiate prefab
            GameObject iconGO = Instantiate(iconPrefab, menuParent);
            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchoredPosition = iconPos;
            menuEntries[i].rectTransform = iconRect;

            // Assign the sprite
            Image iconImage = iconGO.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = menuEntries[i].menuSprite;
                iconImage.color = menuEntries[i].iconColor;
            }

            // Assign the menu name
            TextMeshProUGUI menuEntryText = iconGO.GetComponentInChildren<TextMeshProUGUI>();
            if (menuEntryText != null)
            {
                menuEntryText.SetText(menuEntries[i].menuEntryName);
            }
        }
    }

    [Button]
    private void ClearIcons()
    {
        for (int i = menuParent.childCount - 1; i >= 0; i--)
        {
            if (menuParent.GetChild(i).gameObject.GetComponent<PhoneMenuIconObject>())
            {
                DestroyImmediate(menuParent.GetChild(i).gameObject);
            }
        }
    }

    private Tween tween_Highlight;
    private Sequence sequence_HighlightRotation; 
    private void HighlightIcon()
    {
        UnHighlightAllIcons();
        tween_Highlight = menuEntries[selectedMenuIndex].rectTransform.DOScale(highlightScale, highlightScaleTime).SetEase(highlightEaseType);

        sequence_HighlightRotation = DOTween.Sequence();

        RectTransform target = menuEntries[selectedMenuIndex].rectTransform;

        sequence_HighlightRotation.Append(target.DOLocalRotate(new Vector3(0, 0, highlightRotation), 0.1f))
            .AppendInterval(0.2f)
            .Append(target.DOLocalRotate(new Vector3(0, 0, -highlightRotation), 0.1f))
            .AppendInterval(0.2f)
            .SetLoops(-1, LoopType.Restart); 
    }

    private Tween tween_UnHighlight;
    private void UnHighlightAllIcons()
    {
        tween_Highlight?.Kill();
        tween_UnHighlight?.Kill();
        sequence_HighlightRotation?.Kill(); 
        foreach (var item in menuEntries)
        {
            tween_UnHighlight = item.rectTransform.DOScale(Vector3.one, highlightScaleTime).SetEase(highlightEaseType);
            item.rectTransform.localRotation = Quaternion.identity;
        }
    }

    private Tween tween_ShowPointer;
    private void ShowPointer()
    {
        tween_hidePointer?.Kill();
        tween_ShowPointer?.Kill();
        tween_ShowPointer = pointerTransform.DOScale(Vector3.one, pointerScaleTime).SetEase(pointerEaseType);
    }

    private Tween tween_hidePointer;
    private void HidePointer()
    {
        tween_hidePointer?.Kill();
        tween_ShowPointer?.Kill();
        tween_hidePointer = pointerTransform.DOScale(Vector3.zero, pointerScaleTime).SetEase(pointerEaseType);
    }
}
