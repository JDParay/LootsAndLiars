using UnityEngine;
using UnityEngine.Audio;

public class BGMMngr : MonoBehaviour
{
    public static BGMMngr Instance;
    public AudioSource audioSource;
    public AudioMixer mainMixer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        ApplySavedVolumes();
    }

    void ApplySavedVolumes()
    {
        float savedBGM = PlayerPrefs.GetFloat("Param_BGM", 0.75f);
        float savedSFX = PlayerPrefs.GetFloat("Param_SFX", 0.75f);

        mainMixer.SetFloat("Param_BGM", savedBGM > 0.0001f ? Mathf.Log10(savedBGM) * 20 : -80f);
        mainMixer.SetFloat("Param_SFX", savedSFX > 0.0001f ? Mathf.Log10(savedSFX) * 20 : -80f);
    }

    public void PlayTrack(AudioClip clip)
    {
        if (audioSource.clip == clip && audioSource.isPlaying) return;
        audioSource.clip = clip;
        audioSource.Play();
    }
}