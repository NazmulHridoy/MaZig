using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Static class for managing scene transitions
/// </summary>
public static class SceneController
{
    public const string HOME_SCENE = "HomeScene";
    public const string MAIN_SCENE = "MainScene";
    
    public static void LoadHomeScene()
    {
        SceneManager.LoadScene(HOME_SCENE);
    }
    
    public static void LoadGameScene(bool loadSave = false)
    {
        PlayerPrefs.SetInt("LoadSaveOnStart", loadSave ? 1 : 0);
        SceneManager.LoadScene(MAIN_SCENE);
    }
    
    public static string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
}