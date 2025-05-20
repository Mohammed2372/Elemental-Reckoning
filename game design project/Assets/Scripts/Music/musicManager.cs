using System.Collections;
using UnityEngine;

public class musicManager : MonoBehaviour
{
    private static musicManager instance;
    private AudioSource audioSource;
    public AudioClip backgroundMusic;
    public float fadeDuration = 1.5f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true;
            DontDestroyOnLoad(gameObject);
            PlayMusic(backgroundMusic);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(AudioClip newClip)
    {
        if (audioSource == null || newClip == null)
            return;

        if (audioSource.clip == newClip)
            return; // Already playing this music

        StartCoroutine(FadeMusic(newClip));
    }

    private IEnumerator FadeMusic(AudioClip newClip)
    {
        // Fade out
        float startVolume = audioSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();

        // Change and play new clip
        audioSource.clip = newClip;
        audioSource.Play();

        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = startVolume;
    }

    public AudioClip GetCurrentClip()
    {
        if (audioSource != null)
        {
            return audioSource.clip;
        }
        return null;
    }
}
