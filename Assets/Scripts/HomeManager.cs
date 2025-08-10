using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manages the home scene UI and navigation
/// </summary>
public class HomeManager : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject mainMenuPanel;
    
    [Header("New Game Panel")]
    [SerializeField] private GameObject newGamePanel;
    [SerializeField] private TMP_Dropdown gridSizeDropdown;
    [SerializeField] private TMP_Dropdown themeDropdown;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button backButton;
    
    [Header("Theme Settings")]
    [SerializeField] private SpriteTheme[] availableThemes;
    
    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    
    private SaveSystem saveSystem;
    private AudioManager audioManager;
    
    private void Start()
    {
        saveSystem = SaveSystem.Instance;
        audioManager = AudioManager.Instance;
        
        SetupUI();
        SetupDropdowns();
        SetupSettingsPanel();
        CheckForSaveData();
    }
    
    private void SetupUI()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
            
        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGameClicked);
            
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
        
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGameClicked);
            
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
            
        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(OnSettingsBackClicked);

        if (newGamePanel != null)
            newGamePanel.SetActive(false);
            
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
    
    private void SetupDropdowns()
    {
        if (gridSizeDropdown != null)
        {
            gridSizeDropdown.ClearOptions();
            List<string> gridOptions = new List<string>
            {
                "2x2", "2x3", "3x4", "4x4", "4x5", "5x6", "6x6"
            };
            gridSizeDropdown.AddOptions(gridOptions);
            
            int defaultIndex = gridOptions.IndexOf("2x2");
            if (defaultIndex >= 0)
                gridSizeDropdown.value = defaultIndex;
                
            gridSizeDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
        
        if (themeDropdown != null && availableThemes != null && availableThemes.Length > 0)
        {
            themeDropdown.ClearOptions();
            List<string> themeOptions = new List<string>();
            
            foreach (SpriteTheme theme in availableThemes)
            {
                if (theme != null)
                {
                    themeOptions.Add(theme.themeName);
                }
            }
            
            themeDropdown.AddOptions(themeOptions);
            themeDropdown.value = 0;
            
            themeDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
    }
    
    private void OnDropdownValueChanged(int value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
    
    private void CheckForSaveData()
    {
        bool hasSave = saveSystem != null && saveSystem.HasSaveData();
        
        if (continueButton != null)
            continueButton.gameObject.SetActive(hasSave);
    }
    
    private void OnContinueClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
            
        PlayerPrefs.SetInt("LoadSaveOnStart", 1);
        SceneManager.LoadScene("MainScene");
    }
    
    private void OnNewGameClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
            
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
            
        if (newGamePanel != null)
            newGamePanel.SetActive(true);
    }
    
    private void OnStartGameClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
            
        string gridSize = gridSizeDropdown.options[gridSizeDropdown.value].text;
        string[] dimensions = gridSize.Split('x');
        
        if (dimensions.Length == 2)
        {
            int.TryParse(dimensions[0], out int rows);
            int.TryParse(dimensions[1], out int cols);
            
            PlayerPrefs.SetInt("SelectedRows", rows);
            PlayerPrefs.SetInt("SelectedColumns", cols);
            PlayerPrefs.SetInt("SelectedTheme", themeDropdown.value);
            PlayerPrefs.SetInt("LoadSaveOnStart", 0);
            PlayerPrefs.Save();
            
            Debug.Log($"HomeManager: Starting game with rows={rows}, cols={cols}, theme={themeDropdown.value}");
            
            if (saveSystem != null)
                saveSystem.DeleteSave();

            StartCoroutine(LoadMainScene());
        }
    }
    
    private IEnumerator LoadMainScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainScene");
        
        asyncLoad.allowSceneActivation = true;
        
        while (!asyncLoad.isDone)
        {
            Debug.Log($"Loading progress: {asyncLoad.progress * 100}%");
            yield return null;
        }
        
        Debug.Log("MainScene loaded successfully!");
    }
    
    private void OnBackClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
            
        if (newGamePanel != null)
            newGamePanel.SetActive(false);
            
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }
    
    private void OnQuitClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
            
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    private void OnSettingsClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
            
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
            
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }
    
    private void OnSettingsBackClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
            
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }
    
    private void SetupSettingsPanel()
    {
        if (audioManager != null)
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = audioManager.MasterVolume;
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
                UpdateVolumeText(masterVolumeText, audioManager.MasterVolume);
            }
            
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = audioManager.EffectsVolume;
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
                UpdateVolumeText(sfxVolumeText, audioManager.EffectsVolume);
            }
            
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = audioManager.MusicVolume;
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
                UpdateVolumeText(musicVolumeText, audioManager.MusicVolume);
            }
        }
    }
    
    private void OnMasterVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetMasterVolume(value);
            UpdateVolumeText(masterVolumeText, value);           
            audioManager.PlayButtonClick();
        }
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetEffectsVolume(value);
            UpdateVolumeText(sfxVolumeText, value);
            audioManager.PlayButtonClick();
        }
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetMusicVolume(value);
            UpdateVolumeText(musicVolumeText, value);
        }
    }
    
    private void UpdateVolumeText(TextMeshProUGUI text, float value)
    {
        if (text != null)
        {
            text.text = Mathf.RoundToInt(value * 100) + "%";
        }
    }
    
    private void OnDestroy()
    {
        if (gridSizeDropdown != null)
            gridSizeDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
            
        if (themeDropdown != null)
            themeDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
            
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
            
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
    }
}