using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PhoneUIHomeMenuAnimation))]
public class PhoneUIHomeMenuData : MonoBehaviour, IPhonePanel
{
    public enum MenuActionType { None, OpenChallengeList, OpenMusic, OpenSettings, OpenDiscord, OpenPictureMode, OpenAlbum }

    [System.Serializable]
    public class MenuEntry
    {
        public string menuEntryName;
        public Sprite menuSprite;
        public Color iconColor;
        public MenuActionType actionType;
        [HideInInspector] public RectTransform rectTransform;
    }

    [Header("Icons")]
    public List<MenuEntry> menuEntries;

    [Header("References")]
    public GameObject iconPrefab;
    public GridLayoutGroup gridLayoutGroup;
    public RectTransform rect_GridLayout;

    [Header("Data")]
    [SerializeField] [ReadOnly] private int menuOptionAmount;
    [SerializeField] [ReadOnly] public List<GameObject> instancedIcons = new();
    [SerializeField] [ReadOnly] private int currentIndex;

    private int columnCount => gridLayoutGroup.constraintCount;
    private int rowCount => Mathf.CeilToInt(menuOptionAmount / (float)columnCount);

    private PhoneUIHomeMenuAnimation homeMenuAnimation;
    private PlayerPhone playerPhone; 

    private void Awake()
    {
        homeMenuAnimation = GetComponent<PhoneUIHomeMenuAnimation>();
        playerPhone = PlayerPhone.Instance; 
    }

    public void OnPanelShown()
    {
        SpawnIcons();
        ChangeCurrentIndex(0); // Always reset to index 0 on open
    }

    public void OnPanelHidden()
    {
        // Optional cleanup logic
    }

    public void NavigateLeft()
    {
        int col = currentIndex % columnCount;
        int row = currentIndex / columnCount;

        if (col == 0)
        {
            // Move to the previous row's last column
            if (row > 0)
            {
                int newIndex = (row - 1) * columnCount + (columnCount - 1);
                if (newIndex >= menuOptionAmount)
                    newIndex = ((row - 1) * columnCount) + (menuOptionAmount % columnCount) - 1;

                ChangeCurrentIndex(newIndex);
            }
        }
        else
        {
            ChangeCurrentIndex(currentIndex - 1);
        }
    }

    public void NavigateRight()
    {
        int col = currentIndex % columnCount;
        int row = currentIndex / columnCount;

        if (col == columnCount - 1 || currentIndex + 1 >= menuOptionAmount)
        {
            // Move to the next row's first column
            int newIndex = (row + 1) * columnCount;
            if (newIndex < menuOptionAmount)
                ChangeCurrentIndex(newIndex);
        }
        else
        {
            ChangeCurrentIndex(currentIndex + 1);
        }
    }

    public void NavigateUp()
    {
        int newIndex = currentIndex - columnCount;

        if (newIndex < 0)
        {
            // Wrap to the bottom-most row in same column
            int col = currentIndex % columnCount;
            int lastRow = rowCount - 1;
            int wrappedIndex = lastRow * columnCount + col;

            if (wrappedIndex >= menuOptionAmount)
                wrappedIndex = (lastRow - 1) * columnCount + col;

            ChangeCurrentIndex(wrappedIndex);
        }
        else
        {
            ChangeCurrentIndex(newIndex);
        }
    }

    public void NavigateDown()
    {
        int newIndex = currentIndex + columnCount;

        if (newIndex >= menuOptionAmount)
        {
            // Wrap to top row, same column
            int col = currentIndex % columnCount;
            ChangeCurrentIndex(col);
        }
        else
        {
            ChangeCurrentIndex(newIndex);
        }
    }

    private void ChangeCurrentIndex(int newIndexValue)
    {
        if (instancedIcons.Count == 0) return;

        newIndexValue = Mathf.Clamp(newIndexValue, 0, instancedIcons.Count - 1);
        currentIndex = newIndexValue;

        UpdateSelectionIndicator();
    }

    public void UpdateSelectionIndicator()
    {
        homeMenuAnimation.UpdateSelectionIndicator(GetCurrentLocalPositionForIndicator());
        homeMenuAnimation.AnimateSelectionRotation();
        homeMenuAnimation.AnimateSelectionScale();
    }

    public Vector2 GetCurrentLocalPositionForIndicator()
    {
        if (instancedIcons.Count == 0 || currentIndex < 0 || currentIndex >= instancedIcons.Count)
            return Vector2.zero;

        RectTransform targetRect = instancedIcons[currentIndex].GetComponent<RectTransform>();
        RectTransform indicatorParentRect = homeMenuAnimation.selectionIndicatorParent.parent as RectTransform;

        return indicatorParentRect.InverseTransformPoint(targetRect.position);
    }

    public RectTransform GetCurrentIconRect()
    {
        if (instancedIcons.Count == 0 || currentIndex < 0 || currentIndex >= instancedIcons.Count)
            return null;

        return instancedIcons[currentIndex].GetComponent<RectTransform>();
    }

    public float GetScrollProgress()
    {
        return rowCount <= 1 ? 0f : (float)(currentIndex / columnCount) / (rowCount - 1);
    }

    [Button]
    public void SpawnIcons()
    {
        ClearIcons();
        menuOptionAmount = menuEntries.Count;

        for (int i = 0; i < menuOptionAmount; i++)
        {
            GameObject iconGO = Instantiate(iconPrefab, rect_GridLayout);
            instancedIcons.Add(iconGO);
            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            menuEntries[i].rectTransform = iconRect;

            Image iconImage = iconGO.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = menuEntries[i].menuSprite;
                iconImage.color = menuEntries[i].iconColor;
            }

            TextMeshProUGUI menuEntryText = iconGO.GetComponentInChildren<TextMeshProUGUI>();
            if (menuEntryText != null)
            {
                menuEntryText.SetText(menuEntries[i].menuEntryName);
            }
        }

        StartCoroutine(DelayedUpdateSelectionIndicator());
    }

    private IEnumerator DelayedUpdateSelectionIndicator()
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect_GridLayout);
        UpdateSelectionIndicator();
    }

    [Button]
    private void ClearIcons()
    {
        for (int i = rect_GridLayout.childCount - 1; i >= 0; i--)
        {
            var child = rect_GridLayout.GetChild(i);
            if (child.GetComponent<PhoneMenuIconObject>())
            {
                DestroyImmediate(child.gameObject);
            }
        }

        instancedIcons.Clear();
    }

    public void OnMenuSelect()
    {
        if (currentIndex < 0 || currentIndex >= menuEntries.Count)
            return;

        var selectedEntry = menuEntries[currentIndex];

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
}