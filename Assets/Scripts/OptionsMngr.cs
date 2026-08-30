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
    bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 0.75f);
    sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
    fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

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