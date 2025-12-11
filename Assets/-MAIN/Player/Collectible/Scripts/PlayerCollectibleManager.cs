using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class PlayerCollectibleManager : MonoBehaviour
{
    private List<ICollectible> collectibles = new List<ICollectible>();
    public Transform collectiblesParent;

    [Header("Refs")]
    public TextMeshProUGUI textCoinCount;
    private int coinCount;
    private int maxCoinsCount;
    public TextMeshProUGUI textGearCount;
    private int gearCount;
    private int maxGearsCount;

    // Pitch tracking variables
    private float currentPitch = 1f;
    private float lastCollectionTime = 0f;
    private CollectibleData lastCollectedData; // Track the last collected item data

    private void Start()
    {
        RegisterCollectibles(collectiblesParent);

        maxCoinsCount = 0;
        maxGearsCount = 0;

        foreach (var collectible in collectibles)
        {
            if (collectible is PrismCollectible)
                maxCoinsCount++;
            else if (collectible is GearCollectible)
                maxGearsCount++;
        }

        textCoinCount.SetText($"Prisms: {coinCount} / {maxCoinsCount}");
        textGearCount.SetText($"Gears: {gearCount} / {maxGearsCount}");
    }

    private void RegisterCollectibles(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.TryGetComponent(out ICollectible collectible))
            {
                collectible.OnCollected += HandleCollectibleCollected;
                collectibles.Add(collectible);
            }

            if (child.childCount > 0)
            {
                RegisterCollectibles(child);
            }
        }
    }

    private void HandleCollectibleCollected(ICollectible collectible)
    {
        UpdateCollectibleCounterText(collectible);
        HandlePlaySFX(collectible.GetData());
    }

    private void UpdateCollectibleCounterText(ICollectible collectible)
    {
        if (collectible is PrismCollectible)
        {
            coinCount++;
            textCoinCount.SetText($"Prisms: {coinCount} / {maxCoinsCount}");
        }
        else if (collectible is GearCollectible)
        {
            gearCount++;
            textGearCount.SetText($"Gears: {gearCount} / {maxGearsCount}");
        }
    }

    private void HandlePlaySFX(CollectibleData data)
    {
        if (!data.enablePitchIncrease)
        {
            PlayerSFX.Instance.PlaySFX(data.collectSound, data.volume, 0, data.defaultPitch);
        }
        else
        {
            float currentTime = Time.time;

            // If outside the window or collecting a different type, reset pitch **without increasing**
            if (currentTime - lastCollectionTime > data.pitchIncreaseWindow || lastCollectedData != data)
            {
                currentPitch = data.defaultPitch;
            }
            else
            {
                // Only increase pitch if still inside the window
                currentPitch = Mathf.Min(currentPitch + data.pitchIncreaseAmount, data.maxPitch);
            }

            lastCollectionTime = currentTime;
            lastCollectedData = data;

            // Play sound with adjusted pitch
            PlayerSFX.Instance.PlaySFX(data.collectSound, data.volume, 0, currentPitch);
            //Debug.Log(currentPitch);
        }
    }
}