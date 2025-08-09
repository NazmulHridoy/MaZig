using UnityEngine;

/// <summary>
/// ScriptableObject containing sprite collections for different visual themes
/// </summary>
[CreateAssetMenu(fileName = "SpriteTheme", menuName = "Match Card/Sprite Theme")]
public class SpriteTheme : ScriptableObject
{
    [Header("Theme Information")]
    public string themeName = "Default Theme";
    public string description = "A collection of sprites for the match card game";

    [Header("Card Sprites")]
    public Sprite[] cardFaces;
    public Sprite cardBack;

    [Header("Theme Settings")]
    public Color backgroundColor = Color.white;

    public bool IsValid()
    {
        return cardFaces != null && cardFaces.Length > 0 && cardBack != null;
    }

    public int GetMaxPairs()
    {
        return cardFaces != null ? cardFaces.Length : 0;
    }
}
