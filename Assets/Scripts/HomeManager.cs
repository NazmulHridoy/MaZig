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
    
    private SaveSystem saveSystem;
    
    private void Start()
    {
        saveSystem = GetComponent<SaveSystem>();
        if (saveSystem == null)
        {
            GameObject saveSystemObj = new GameObject("SaveSystem");
            saveSystem = saveSystemObj.AddComponent<SaveSystem>();
        }
        
        SetupUI();
        SetupDropdowns();
        CheckForSaveData();
    }
    
    private void SetupUI()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
            
        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGameClicked);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
        
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGameClicked);
            
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        if (newGamePanel != null)
            newGamePanel.SetActive(false);
    }
    
    private void SetupDropdowns()
    {
        if (gridSizeDropdown != null)
        {
            gridSizeDropdown.ClearOptions();
            List<string> gridOptions = new List<string>
            {
                "2x2", "2x3", "3x4", "4x4", "4x5", "5x5", "5x6", "6x6"
            };
            gridSizeDropdown.AddOptions(gridOptions);
            
            int defaultIndex = gridOptions.IndexOf("2x2");
            if (defaultIndex >= 0)
                gridSizeDropdown.value = defaultIndex;
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
        }
    }
    
    private void CheckForSaveData()
    {
        bool hasSave = saveSystem != null && saveSystem.HasSaveData();
        
        if (continueButton != null)
            continueButton.gameObject.SetActive(hasSave);
    }
    
    private void OnContinueClicked()
    {
        PlayerPrefs.SetInt("LoadSaveOnStart", 1);
        SceneManager.LoadScene("MainScene");
    }
    
    private void OnNewGameClicked()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
            
        if (newGamePanel != null)
            newGamePanel.SetActive(true);
    }
    
    private void OnStartGameClicked()
    {
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
            
            if (saveSystem != null)
                saveSystem.DeleteSave();
            
            SceneManager.LoadScene("MainScene");
        }
    }
    
    private void OnBackClicked()
    {
        if (newGamePanel != null)
            newGamePanel.SetActive(false);
            
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }
    
    private void OnQuitClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}