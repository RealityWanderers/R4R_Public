using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerRamenDeliveryPoint : MonoBehaviour
{
    [Header("Particles")]
    public List<ParticleSystem> onCatchParticles;

    [Header("Audio")]
    public AudioSource source_CatchSFX;
    public AudioSource source_NPCVoice;

    [Header("References")]
    private ChallengeObject challengeObject;

    public void Awake()
    {
        challengeObject = GetComponentInParent<ChallengeObject>();
    }

    public void OnCatch()
    {
        //Play Particle
        //Play SFX
        //Play Animation

        source_CatchSFX.Play();
        source_NPCVoice.Play(); 

        foreach (var particle in onCatchParticles)
        {
            particle.Play(); //Might need to use emit instead.
        }

        challengeObject.CompleteChallengeObjective();

        gameObject.SetActive(false); 
    }
}
