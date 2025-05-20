using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Menu Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button backButton;

    [Header("Blur Effect")]
    [SerializeField] private Material blurMaterial;
    [SerializeField] private float blurAmount = 5f;

    private bool isPaused = false;
    private float originalTimeScale = 1f;
    private float originalBlurAmount = 0f;
    private Button[] currentPanelButtons;
    private int currentButtonIndex = 0;
    private float lastInputTime = 0f;
    private float inputDelay = 0.2f;

    private void Start()
    {
        pauseMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);

        originalTimeScale = Time.timeScale;
        originalBlurAmount = blurMaterial != null ? blurMaterial.GetFloat("_BlurAmount") : 0f;

        currentPanelButtons = new Button[] { resumeButton, optionsButton, mainMenuButton };
    }

    private void Update()
    {
        // Check for Escape (keyboard) or Start button (gamepad)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame ||
            Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
        {
            TogglePause();
        }

        if (isPaused)
        {
            HandleMenuNavigation();
        }
    }

    private void HandleMenuNavigation()
    {
        if (Time.unscaledTime - lastInputTime < inputDelay)
            return;

        // Gamepad/Keyboard Navigation
        if (Gamepad.current != null)
        {
            if (Gamepad.current.dpad.up.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                NavigateUp();
                lastInputTime = Time.unscaledTime;
            }
            else if (Gamepad.current.dpad.down.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                NavigateDown();
                lastInputTime = Time.unscaledTime;
            }

            if (Gamepad.current.buttonSouth.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
            {
                SelectCurrentButton();
                lastInputTime = Time.unscaledTime;
            }
        }
    }

    private void NavigateUp()
    {
        currentButtonIndex--;
        if (currentButtonIndex < 0)
            currentButtonIndex = currentPanelButtons.Length - 1;
        UpdateButtonSelection();
    }

    private void NavigateDown()
    {
        currentButtonIndex++;
        if (currentButtonIndex >= currentPanelButtons.Length)
            currentButtonIndex = 0;
        UpdateButtonSelection();
    }

    private void UpdateButtonSelection()
    {
        foreach (Button button in currentPanelButtons)
        {
            if (button != null)
                button.OnDeselect(null);
        }

        if (currentPanelButtons[currentButtonIndex] != null)
            currentPanelButtons[currentButtonIndex].Select();
    }

    private void SelectCurrentButton()
    {
        if (currentPanelButtons[currentButtonIndex] != null)
            currentPanelButtons[currentButtonIndex].onClick.Invoke();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            pauseMenuPanel.SetActive(true);
            optionsPanel.SetActive(false);

            currentPanelButtons = new Button[] { resumeButton, optionsButton, mainMenuButton };
            currentButtonIndex = 0;
            UpdateButtonSelection();

            if (blurMaterial != null)
                blurMaterial.SetFloat("_BlurAmount", blurAmount);
        }
        else
        {
            Time.timeScale = originalTimeScale;
            pauseMenuPanel.SetActive(false);
            optionsPanel.SetActive(false);

            if (blurMaterial != null)
                blurMaterial.SetFloat("_BlurAmount", originalBlurAmount);
        }
    }

    public void ResumeGame() => TogglePause();

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);

        currentPanelButtons = new Button[] { backButton };
        currentButtonIndex = 0;
        UpdateButtonSelection();
    }

    public void OpenMainMenu()
    {
        SceneManager.LoadScene("main menu");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        Time.timeScale = originalTimeScale;
        if (blurMaterial != null)
            blurMaterial.SetFloat("_BlurAmount", originalBlurAmount);
    }
}