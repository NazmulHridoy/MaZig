using UnityEngine;

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

    [Header("Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float effectsVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;

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

        effectsSource.volume = effectsVolume;
        musicSource.volume = musicVolume;
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

    public void SetEffectsVolume(float volume)
    {
        effectsVolume = Mathf.Clamp01(volume);
        if (effectsSource != null)
            effectsSource.volume = effectsVolume;

        PlayerPrefs.SetFloat("EffectsVolume", effectsVolume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;

        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }

    private void Start()
    {
        effectsVolume = PlayerPrefs.GetFloat("EffectsVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);

        SetEffectsVolume(effectsVolume);
        SetMusicVolume(musicVolume);
        
        // Start background music automatically
        PlayBackgroundMusic();
    }
}
