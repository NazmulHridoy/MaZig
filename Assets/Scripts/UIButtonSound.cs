using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Add this component to any UI Button to automatically play button click sound
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    private Button button;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        
        // Add click sound to button
        button.onClick.AddListener(PlayButtonClickSound);
    }
    
    private void PlayButtonClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
    
    private void OnDestroy()
    {
        // Clean up listener when destroyed
        if (button != null)
        {
            button.onClick.RemoveListener(PlayButtonClickSound);
        }
    }
}