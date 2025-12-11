using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSFX : MonoBehaviour
{
    private AudioSource source;

    public static PlayerSFX Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }

    private void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlaySFX(AudioClip clip, float volume, float delay = 0f, float pitch = 1)
    {
        if (clip == null) { /*Debug.Log("NoClip");*/ return; }
        if (delay > 0)
            StartCoroutine(PlaySFXWithDelay(clip, volume, delay, pitch));
        else
        {
            source.pitch = pitch;
            source.PlayOneShot(clip, volume);
        }
    }

    private IEnumerator PlaySFXWithDelay(AudioClip clip, float volume, float delay, float pitch)
    {
        yield return new WaitForSeconds(delay);
        source.pitch = pitch;
        source.PlayOneShot(clip, volume);
    }
}
