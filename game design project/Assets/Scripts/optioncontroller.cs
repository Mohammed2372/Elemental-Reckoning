using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Audio;

public class optioncontroller : MonoBehaviour
{

    [SerializeField]
    private AudioMixer mixer;

    public loadManager loadManager;

    public void setVolume(float volume)
    {
        Debug.Log(volume);
        mixer.SetFloat("volume", volume);
    }

    public void setQuality(int n)
    {
        QualitySettings.SetQualityLevel(n + 3);
    }

    public void fullScreenToggle(bool isFullScreen)
    {

#if UNITY_EDITOR
        Debug.Log(isFullScreen);
#endif
        Screen.fullScreen = isFullScreen;
    }

    public void Play_Button()
    {
        loadManager.LoadNextLevel();
        //SceneManager.LoadScene("level 1");
    }


    public void Quit_Button()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();

    }
}
