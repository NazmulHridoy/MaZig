using UnityEngine;

/// <summary>
/// ScriptableObject that stores grid layout configuration for different board sizes
/// Provides both automatic sizing (responsive) and manual sizing options
/// Contains presets for common grid sizes (2x2 through 6x6) with optimized settings
/// </summary>
[CreateAssetMenu(fileName = "GridLayoutSettings", menuName = "Match Card/Grid Layout Settings")]
public class GridLayoutSettings : ScriptableObject
{
    [System.Serializable]
    public class GridSizePreset
    {
        [Tooltip("Display name for this grid preset (e.g., '2x2', '4x4')")]
        public string name = "2x2";
        [Tooltip("Number of rows in the grid")]
        public int rows = 2;
        [Tooltip("Number of columns in the grid")]
        public int columns = 2;

        [Header("Card Settings")]
        [Range(30f, 600f)]
        [Tooltip("Base size of each card in pixels (before multiplier)")]
        public float cardSize = 100f;
        [Range(0.5f, 2f)]
        [Tooltip("Multiplier applied to base card size (1.0 = normal, 1.5 = 50% larger)")]
        public float cardSizeMultiplier = 1f;

        [Header("Spacing")]
        [Range(0f, 100f)]
        [Tooltip("Space between adjacent cards in pixels")]
        public float cardSpacing = 20f;
        [Range(0f, 200f)]
        [Tooltip("Padding from left/right edges of play area in pixels")]
        public float horizontalPadding = 50f;
        [Range(0f, 200f)]
        [Tooltip("Padding from top/bottom edges of play area in pixels")]
        public float verticalPadding = 50f;

        [Header("Advanced")]
        [Tooltip("Calculate card size automatically to fit available space")]
        public bool useAutomaticSizing = true;
        [Tooltip("Limit automatic sizing to maintain original card proportions")]
        public bool maintainAspectRatio = true;

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public GridSizePreset() { }

        /// <summary>
        /// Creates a preset with specified grid dimensions and default settings
        /// Used for creating fallback presets when none are found
        /// </summary>
        public GridSizePreset(string gridName, int r, int c)
        {
            name = gridName;
            rows = r;
            columns = c;
        }
    }

    [Header("Global Settings")]
    [Range(30f, 600f)]
    [Tooltip("Minimum allowed card size in pixels (prevents cards from becoming too small)")]
    public float minCardSize = 30f;
    [Range(50f, 600f)]
    [Tooltip("Maximum allowed card size in pixels (prevents cards from becoming too large)")]
    public float maxCardSize = 600f;

    [Header("Grid Presets")]
    [Tooltip("Predefined configurations for common grid sizes - optimized for good visual balance")]
    public GridSizePreset[] gridPresets = new GridSizePreset[]
    {
        new GridSizePreset("2x2", 2, 2) { cardSize = 500f, cardSpacing = 100f, horizontalPadding = 50f, verticalPadding = 50f },
        new GridSizePreset("2x3", 2, 3) { cardSize = 450f, cardSpacing = 80f, horizontalPadding = 40f, verticalPadding = 40f },
        new GridSizePreset("3x4", 3, 4) { cardSize = 400f, cardSpacing = 60f, horizontalPadding = 30f, verticalPadding = 30f },
        new GridSizePreset("4x4", 4, 4) { cardSize = 350f, cardSpacing = 50f, horizontalPadding = 25f, verticalPadding = 25f },
        new GridSizePreset("4x5", 4, 5) { cardSize = 300f, cardSpacing = 40f, horizontalPadding = 20f, verticalPadding = 20f },
        new GridSizePreset("5x6", 5, 6) { cardSize = 200f, cardSpacing = 30f, horizontalPadding = 15f, verticalPadding = 15f },
        new GridSizePreset("6x6", 6, 6) { cardSize = 150f, cardSpacing = 20f, horizontalPadding = 10f, verticalPadding = 10f }
    };

    [Header("Animation")]
    [Range(0.1f, 2f)]
    [Tooltip("Duration of card flip animation in seconds")]
    public float defaultFlipDuration = 0.3f;
    [Range(0.1f, 5f)]
    [Tooltip("Time to wait before flipping mismatched cards back (gives player time to see)")]
    public float defaultFlipBackDelay = 1f;
    [Range(1f, 10f)]
    [Tooltip("How long to show all cards at game start for memorization")]
    public float defaultPreviewDuration = 3f;

    public GridSizePreset GetPresetForGrid(int rows, int columns)
    {
        foreach (var preset in gridPresets)
        {
            if (preset.rows == rows && preset.columns == columns)
            {
                return preset;
            }
        }
        return new GridSizePreset($"{rows}x{columns}", rows, columns);
    }

    public void SavePreset(int rows, int columns, float cardSize, float spacing, float hPadding, float vPadding)
    {
        var preset = GetPresetForGrid(rows, columns);
        if (preset != null)
        {
            preset.cardSize = cardSize;
            preset.cardSpacing = spacing;
            preset.horizontalPadding = hPadding;
            preset.verticalPadding = vPadding;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
