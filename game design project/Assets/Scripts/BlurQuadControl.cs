using UnityEngine;
using UnityEngine.UI;

public class BlurQuadControl : MonoBehaviour
{
    private Image image;
    private PauseMenu pauseMenu;

    private void Start()
    {
        image = GetComponent<Image>();
        pauseMenu = FindObjectOfType<PauseMenu>();

        // Hide the overlay initially
        image.enabled = false;
    }

    private void Update()
    {
        // Update visibility based on pause state
        if (pauseMenu != null)
        {
            image.enabled = Time.timeScale == 0f;
        }
    }
}