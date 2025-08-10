using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource effectsSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip cardFlipSound;
    [SerializeField] private AudioClip matchSound;
    [SerializeField] private AudioClip mismatchSound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip buttonClickSound;
    
    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float effectsVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;
    
    // Properties for external access
    public float MasterVolume => masterVolume;
    public float EffectsVolume => effectsVolume;
    public float MusicVolume => musicVolume;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSources()
    {
        if (effectsSource == null)
        {
            GameObject effectsObj = new GameObject("EffectsSource");
            effectsObj.transform.SetParent(transform);
            effectsSource = effectsObj.AddComponent<AudioSource>();
            effectsSource.playOnAwake = false;
        }

        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }

        UpdateVolumes();
    }

    public void PlayCardFlip() => PlaySound(cardFlipSound);
    public void PlayMatch() => PlaySound(matchSound);
    public void PlayMismatch() => PlaySound(mismatchSound);
    public void PlayGameOver() => PlaySound(gameOverSound);
    public void PlayButtonClick() => PlaySound(buttonClickSound);

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && effectsSource != null)
        {
            effectsSource.PlayOneShot(clip, effectsVolume);
        }
    }
    
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
    
    public void StopBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
    
    public void PauseBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }
    
    public void ResumeBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.Save();
    }

    public void SetEffectsVolume(float volume)
    {
        effectsVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        PlayerPrefs.SetFloat("EffectsVolume", effectsVolume);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
    }
    
    private void UpdateVolumes()
    {
        if (effectsSource != null)
            effectsSource.volume = effectsVolume * masterVolume;
        
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
    }

    private void Start()
    {
        // Load saved volume settings
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        effectsVolume = PlayerPrefs.GetFloat("EffectsVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);

        UpdateVolumes();
        
        // Start background music automatically
        PlayBackgroundMusic();
    }
    
    // Methods for UI sliders
    public void OnMasterVolumeChanged(Slider slider)
    {
        if (slider != null)
            SetMasterVolume(slider.value);
    }
    
    public void OnEffectsVolumeChanged(Slider slider)
    {
        if (slider != null)
            SetEffectsVolume(slider.value);
    }
    
    public void OnMusicVolumeChanged(Slider slider)
    {
        if (slider != null)
            SetMusicVolume(slider.value);
    }
}
