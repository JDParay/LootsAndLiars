using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class OptionsMngr : MonoBehaviour
{
    public AudioMixer mainMixer;
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Toggle fullscreenToggle;

    void Start()
{
    float savedBGM = PlayerPrefs.GetFloat("Param_BGM", 0.75f);
    float savedSFX = PlayerPrefs.GetFloat("Param_SFX", 0.75f);
    bool savedFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

    bgmSlider.value = savedBGM;
    sfxSlider.value = savedSFX;
    fullscreenToggle.SetIsOnWithoutNotify(savedFullscreen);

    ApplyBGMVolume(savedBGM);
    ApplySFXVolume(savedSFX);

    bgmSlider.onValueChanged.AddListener(ApplyBGMVolume);
    sfxSlider.onValueChanged.AddListener(ApplySFXVolume);
    fullscreenToggle.onValueChanged.AddListener(ApplyFullscreen);
}

    public void ApplyBGMVolume(float sliderValue)
    {
        float db = sliderValue > 0.0001f ? Mathf.Log10(sliderValue) * 20 : -80f;
        mainMixer.SetFloat("Param_BGM", db);
        PlayerPrefs.SetFloat("Param_BGM", sliderValue);
    }

    public void ApplySFXVolume(float sliderValue)
    {
        float db = sliderValue > 0.0001f ? Mathf.Log10(sliderValue) * 20 : -80f;
        mainMixer.SetFloat("Param_SFX", db);
        PlayerPrefs.SetFloat("Param_SFX", sliderValue);
    }

    public void ApplyFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }
}