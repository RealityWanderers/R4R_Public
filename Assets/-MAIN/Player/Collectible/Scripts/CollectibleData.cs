using UnityEngine;

[CreateAssetMenu(fileName = "New Collectible Data", menuName = "Collectibles/Collectible Data")]
public class CollectibleData : ScriptableObject
{
    [Header("Name")]
    public string collectibleName;

    [Header("Value")]
    public int value = 1;

    [Header("SFX")]
    public AudioClip collectSound;
    [Range(0, 1)] public float volume;

    [Header("Pitch Increase Settings")]
    public bool enablePitchIncrease = false;
    public float defaultPitch = 1; 
    public float maxPitch = 1.4f;
    public float pitchIncreaseAmount = 0.1f;
    public float pitchIncreaseWindow = 1.5f;
}