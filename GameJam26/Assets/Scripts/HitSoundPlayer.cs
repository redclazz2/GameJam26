using System.Collections.Generic;
using UnityEngine;

public class HitSoundPlayer : MonoBehaviour
{
    [Header("Audio")]
    public List<AudioClip> hitSounds;
    public float volume = 1f;

    [Header("Lifetime")]
    public float destroyDelay = 1f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D
    }

    void Start()
    {
        if (hitSounds == null || hitSounds.Count == 0)
        {
            Debug.LogWarning("HitSoundPlayer: No hit sounds assigned.");
            Destroy(gameObject);
            return;
        }

        AudioClip clip = hitSounds[Random.Range(0, hitSounds.Count)];
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();

        Destroy(gameObject, Mathf.Max(clip.length, destroyDelay));
    }
}
