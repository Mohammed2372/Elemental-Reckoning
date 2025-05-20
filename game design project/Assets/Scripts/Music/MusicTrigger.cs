using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    public AudioClip newMusic;

    void Start()
    {
        musicManager music = FindObjectOfType<musicManager>();
        if (music != null && newMusic != null)
        {
            // Only play if the clip is different
            if (music.GetCurrentClip() != newMusic)
            {
                music.PlayMusic(newMusic);
            }
        }
    }
}

