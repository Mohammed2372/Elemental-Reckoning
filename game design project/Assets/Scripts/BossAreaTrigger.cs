using UnityEngine;
using Cinemachine;

public class BossAreaTrigger : MonoBehaviour
{
    public GameObject collidersGroup; // Assign your "colliders" GameObject here
    public CinemachineVirtualCamera bossCam; // Assign the Cinemachine Virtual Camera for the boss
    public AudioClip newMusic; // Music to play for the boss area
    public GameObject boss;
    public GameObject boss_health;


    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger entered by: " + other.name + " with tag: " + other.tag);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered the boss area!");

            // Enable boss area colliders
            if (collidersGroup != null)
                collidersGroup.SetActive(true);

            // Switch to boss camera
            if (bossCam != null)
                bossCam.Priority = 100;

            // Change music
            if (newMusic != null)
            {
                musicManager music = FindObjectOfType<musicManager>();
                if (music != null && music.GetCurrentClip() != newMusic)
                {
                    music.PlayMusic(newMusic);
                }
            }

            // Prevent re-trigger
            GetComponent<Collider2D>().enabled = false;

            boss.GetComponent<BossMove>().enabled = true;
            boss_health.SetActive(true);

        }
    }

}
