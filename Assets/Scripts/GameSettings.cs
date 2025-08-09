using UnityEngine;

/// <summary>
/// Legacy game settings - mostly replaced by GridLayoutSettings
/// </summary>
[CreateAssetMenu(fileName = "GameSettings", menuName = "Match Card/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Card Settings")]
    public float cardSize = 100f;
    public float minCardSize = 50f;
    public float maxCardSize = 200f;
    public float cardFlipDuration = 0.3f;
    public float flipBackDelay = 1f;

    private void OnValidate()
    {
        cardSize = Mathf.Clamp(cardSize, minCardSize, maxCardSize);
        cardFlipDuration = Mathf.Max(0.1f, cardFlipDuration);
        flipBackDelay = Mathf.Max(0.1f, flipBackDelay);
    }
}
