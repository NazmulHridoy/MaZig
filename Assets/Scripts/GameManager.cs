using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// Main game controller that manages the match card game flow, UI, and game state.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private GridLayoutSettings gridLayoutSettings;
    private float cardSpacing = 10f;
    [SerializeField] private float flipBackDelay = 1f;
    [SerializeField] private float cardFlipDuration = 0.3f;
    [SerializeField] private float previewDuration = 3f;
    
    
    private GridLayoutSettings.GridSizePreset currentGridPreset;
    private float calculatedCardSize = 100f;

    [Header("Card Setup")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private RectTransform playArea;
    [SerializeField] private Sprite[] cardFaces;
    [SerializeField] private Sprite cardBack;
    
    [Header("Sprite Themes")]
    [SerializeField] private SpriteTheme[] availableThemes;
    [SerializeField] private int currentThemeIndex = 0;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI finalMovesText;
    [SerializeField] private TextMeshProUGUI finalTimeText;
    [SerializeField] private TMP_Dropdown layoutDropdown;
    [SerializeField] private TMP_Dropdown themeDropdown;

    // Game state variables
    private List<Card> flippedCards = new List<Card>();
    private List<Card> allCards = new List<Card>();
    private int score = 0;
    private int moves = 0;
    private float gameTime = 0f;
    private bool gameActive = false;
    private int totalPairs;
    private int matchedPairs = 0;
    private int currentRows = 2;
    private int currentColumns = 2;
    private bool isCheckingMatches = false;

    // System references
    private SaveSystem saveSystem;
    private AudioManager audioManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        saveSystem = GetComponent<SaveSystem>();
        audioManager = AudioManager.Instance;
        
        SetupLayoutDropdown();
        SetupThemeDropdown();

        StartCoroutine(DelayedStart());
    }
    
    private IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();

        if (playArea != null)
        {
            Canvas.ForceUpdateCanvases();
            
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(playArea);
            
            yield return null;
            
            float maxAttempts = 3;
            float attempts = 0;
            
            while ((playArea.rect.width <= 0 || playArea.rect.height <= 0) && attempts < maxAttempts)
            {
                Debug.LogWarning($"Waiting for valid play area dimensions... Attempt {attempts + 1}");
                yield return new WaitForEndOfFrame();
                Canvas.ForceUpdateCanvases();
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(playArea);
                attempts++;
            }
            
            if (playArea.rect.width > 0 && playArea.rect.height > 0)
            {
                Debug.Log($"Play area initialized with dimensions: {playArea.rect.width}x{playArea.rect.height}");
            }
            else
            {
                Debug.LogError("Failed to get valid play area dimensions after multiple attempts");
            }
        }
        
        // Check if we should load save or start new game based on home scene preferences
        bool shouldLoadSave = PlayerPrefs.GetInt("LoadSaveOnStart", 0) == 1;
        PlayerPrefs.DeleteKey("LoadSaveOnStart"); // Clear after reading
        
        if (shouldLoadSave && saveSystem.HasSaveData())
        {
            LoadGame();
        }
        else
        {
            // Get game preferences from home scene
            currentRows = PlayerPrefs.GetInt("SelectedRows", 2);
            currentColumns = PlayerPrefs.GetInt("SelectedColumns", 2);
            currentThemeIndex = PlayerPrefs.GetInt("SelectedTheme", 0);
            
            // Update dropdowns to match selected values
            UpdateDropdownsToSelection();
            
            StartNewGame();
        }
    }

    private void Update()
    {
        if (gameActive)
        {
            gameTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    private void SetupLayoutDropdown()
    {
        if (layoutDropdown != null)
        {
            layoutDropdown.ClearOptions();
            List<string> options = new List<string>
            {
                "2x2", "2x3", "3x4", "4x4", "4x5", "5x5", "5x6", "6x6"
            };
            layoutDropdown.AddOptions(options);
            layoutDropdown.onValueChanged.AddListener(OnLayoutChanged);
            
            // Set dropdown to match current grid size
            string currentLayout = $"{currentRows}x{currentColumns}";
            int index = options.IndexOf(currentLayout);
            if (index >= 0)
            {
                layoutDropdown.SetValueWithoutNotify(index);
            }
        }
    }

    // Handles grid size change from dropdown selection
    private void OnLayoutChanged(int index)
    {
        string layout = layoutDropdown.options[index].text;
        string[] dimensions = layout.Split('x');
        if (dimensions.Length == 2)
        {
            int.TryParse(dimensions[0], out currentRows);
            int.TryParse(dimensions[1], out currentColumns);
            StartNewGame();
        }
    }
    
    private void SetupThemeDropdown()
    {
        if (themeDropdown != null && availableThemes != null && availableThemes.Length > 0)
        {
            themeDropdown.ClearOptions();
            List<string> options = new List<string>();
            
            foreach (SpriteTheme theme in availableThemes)
            {
                if (theme != null)
                {
                    options.Add(theme.themeName);
                }
            }
            
            themeDropdown.AddOptions(options);
            themeDropdown.onValueChanged.AddListener(OnThemeChanged);
            
            if (currentThemeIndex >= 0 && currentThemeIndex < availableThemes.Length)
            {
                themeDropdown.SetValueWithoutNotify(currentThemeIndex);
                SetCurrentTheme(currentThemeIndex);
            }
        }
        else
        {
            Debug.Log("No sprite themes configured, using legacy sprites");
        }
    }

    private void OnThemeChanged(int index)
    {
        if (index >= 0 && index < availableThemes.Length)
        {
            currentThemeIndex = index;
            SetCurrentTheme(index);
            StartNewGame();
        }
    }
    
    private void SetCurrentTheme(int index)
    {
        if (availableThemes != null && index >= 0 && index < availableThemes.Length)
        {
            currentThemeIndex = index;
            var currentTheme = availableThemes[currentThemeIndex];
            if (currentTheme != null && currentTheme.backgroundColor != Color.white)
            {
                Camera.main.backgroundColor = currentTheme.backgroundColor;
            }
        }
    }
    
    private void UpdateDropdownsToSelection()
    {
        // Update layout dropdown
        if (layoutDropdown != null)
        {
            string selectedLayout = $"{currentRows}x{currentColumns}";
            for (int i = 0; i < layoutDropdown.options.Count; i++)
            {
                if (layoutDropdown.options[i].text == selectedLayout)
                {
                    layoutDropdown.SetValueWithoutNotify(i);
                    break;
                }
            }
        }
        
        // Update theme dropdown and apply theme
        if (themeDropdown != null && currentThemeIndex >= 0 && currentThemeIndex < themeDropdown.options.Count)
        {
            themeDropdown.SetValueWithoutNotify(currentThemeIndex);
            SetCurrentTheme(currentThemeIndex);
        }
    }
    
    private Sprite GetCardFaceSprite(int cardValue)
    {
        if (availableThemes != null && currentThemeIndex < availableThemes.Length)
        {
            var currentTheme = availableThemes[currentThemeIndex];
            if (currentTheme != null && currentTheme.IsValid())
            {
                return cardValue < currentTheme.cardFaces.Length ? currentTheme.cardFaces[cardValue] : currentTheme.cardBack; // Have to change this later if time
            }
        }
        
        // Fallback to legacy sprite array
        return cardValue < cardFaces.Length ? cardFaces[cardValue] : cardBack;
    }
    
    private Sprite GetCardBackSprite()
    {
        if (availableThemes != null && currentThemeIndex < availableThemes.Length)
        {
            var currentTheme = availableThemes[currentThemeIndex];
            if (currentTheme != null && currentTheme.IsValid())
            {
                return currentTheme.cardBack;
            }
        }
        
        // Fallback to legacy back sprite
        return cardBack;
    }
    
    private int GetMaxAvailableSprites()
    {
        if (availableThemes != null && currentThemeIndex < availableThemes.Length)
        {
            var currentTheme = availableThemes[currentThemeIndex];
            if (currentTheme != null && currentTheme.IsValid())
            {
                return currentTheme.GetMaxPairs();
            }
        }
        
        return cardFaces != null ? cardFaces.Length : 12;
    }

    public void StartNewGame()
    {
        ClearBoard();
        SetupBoard();
        ResetGameStats();
        if (winPanel != null)
            winPanel.SetActive(false);
        StartCoroutine(ShowCardsPreview());
    }

    private void ClearBoard()
    {
        foreach (Card card in allCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }
        allCards.Clear();
        flippedCards.Clear();
    }

    private void SetupBoard()
    {
        totalPairs = (currentRows * currentColumns) / 2;

        int maxUniqueSprites = GetMaxAvailableSprites();
        
        int uniqueSpritesToUse = Mathf.Min(totalPairs, maxUniqueSprites);
        
        List<int> cardValues = new List<int>();

        for (int i = 0; i < totalPairs; i++)
        {
            int spriteIndex = i % uniqueSpritesToUse;
            cardValues.Add(spriteIndex);
            cardValues.Add(spriteIndex);
        }

        cardValues = ShuffleList(cardValues);

        CalculateCardLayout();

        // Create and position each card in the grid
        int cardIndex = 0;
        for (int row = 0; row < currentRows; row++)
        {
            for (int col = 0; col < currentColumns; col++)
            {
                if (cardIndex < cardValues.Count)
                {
                    CreateCard(row, col, cardValues[cardIndex]);
                    cardIndex++;
                }
            }
        }
    }

    private void CalculateCardLayout()
    {
        if (playArea == null) 
        {
            Debug.LogError("PlayArea is null in CalculateCardLayout!");
            return;
        }

        float areaWidth = playArea.rect.width;
        float areaHeight = playArea.rect.height;

        if (areaWidth <= 0 || areaHeight <= 0)
        {
            Debug.LogError($"Invalid play area dimensions: {areaWidth}x{areaHeight}");
            Canvas.ForceUpdateCanvases();
            areaWidth = playArea.rect.width;
            areaHeight = playArea.rect.height;
            
            if (areaWidth <= 0) areaWidth = 800f;
            if (areaHeight <= 0) areaHeight = 600f;
        }
        
        if (gridLayoutSettings != null)
        {
            currentGridPreset = gridLayoutSettings.GetPresetForGrid(currentRows, currentColumns);
            
            if (currentGridPreset.useAutomaticSizing)
            {
                CalculateAutomaticLayout(areaWidth, areaHeight);
            }
            else
            {
                cardSpacing = currentGridPreset.cardSpacing;
                calculatedCardSize = currentGridPreset.cardSize * currentGridPreset.cardSizeMultiplier;
            }

            flipBackDelay = gridLayoutSettings.defaultFlipBackDelay;
            cardFlipDuration = gridLayoutSettings.defaultFlipDuration;
            previewDuration = gridLayoutSettings.defaultPreviewDuration;
        }

        Debug.Log($"Grid: {currentRows}x{currentColumns}, CardSize: {calculatedCardSize}, Spacing: {cardSpacing}");
        Debug.Log($"Padding: H={currentGridPreset?.horizontalPadding ?? 0}px, V={currentGridPreset?.verticalPadding ?? 0}px");
        Debug.Log($"PlayArea: {areaWidth}x{areaHeight}");
    }
    
    private void CalculateAutomaticLayout(float areaWidth, float areaHeight)
    {
        float hPadding = currentGridPreset.horizontalPadding;
        float vPadding = currentGridPreset.verticalPadding;
        float spacing = currentGridPreset.cardSpacing;
        
        float effectiveWidth = areaWidth - (2 * hPadding);
        float effectiveHeight = areaHeight - (2 * vPadding);

        float cardWidth = (effectiveWidth - (currentColumns - 1) * spacing) / currentColumns;
        float cardHeight = (effectiveHeight - (currentRows - 1) * spacing) / currentRows;
        
        float cardSize = Mathf.Min(cardWidth, cardHeight);
        
        if (currentGridPreset.maintainAspectRatio)
        {
            cardSize = Mathf.Min(cardSize, currentGridPreset.cardSize);
        }
        
        cardSize *= currentGridPreset.cardSizeMultiplier;

        if (gridLayoutSettings != null)
        {
            cardSize = Mathf.Clamp(cardSize, gridLayoutSettings.minCardSize, gridLayoutSettings.maxCardSize);
        }
        else
        {
            cardSize = Mathf.Clamp(cardSize, 30f, 250f); // Fallback limits
        }

        cardSpacing = spacing;
        calculatedCardSize = cardSize;
    }

    private void CreateCard(int row, int col, int cardValue)
    {
        GameObject newCard = Instantiate(cardPrefab, cardContainer);
        Card cardComponent = newCard.GetComponent<Card>();
        
        if (cardComponent != null)
        {
            RectTransform rectTransform = newCard.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            
            rectTransform.sizeDelta = new Vector2(calculatedCardSize, calculatedCardSize);
            
            float spacing = cardSpacing;
            float totalWidth = currentColumns * calculatedCardSize + (currentColumns - 1) * spacing;
            float totalHeight = currentRows * calculatedCardSize + (currentRows - 1) * spacing;
            
            // Calculate starting position (top-left card center)
            float startX = -totalWidth / 2f + calculatedCardSize / 2f;
            float startY = totalHeight / 2f - calculatedCardSize / 2f;
            
            // Calculate this card's position in the grid
            float xPos = startX + col * (calculatedCardSize + spacing);
            float yPos = startY - row * (calculatedCardSize + spacing);
            rectTransform.anchoredPosition = new Vector2(xPos, yPos);

            Sprite cardFace = GetCardFaceSprite(cardValue);
            Sprite cardBackSprite = GetCardBackSprite();
            
            cardComponent.Initialize(cardValue, cardFace, cardBackSprite, cardFlipDuration);
            allCards.Add(cardComponent);
        }
    }

    private List<T> ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
        return list;
    }

    private IEnumerator ShowCardsPreview()
    {
        foreach (Card card in allCards)
        {
            card.ShowPreview();
        }
        
        yield return new WaitForSeconds(previewDuration);
        
        foreach (Card card in allCards)
        {
            card.HidePreview();
        }

        yield return new WaitForSeconds(cardFlipDuration);

        gameActive = true;
    }

    public void CardClicked(Card clickedCard)
    {
        if (!gameActive || clickedCard.IsFlipped || clickedCard.IsMatched)
            return;

        clickedCard.Flip();
        flippedCards.Add(clickedCard);
        
        moves++;
        UpdateMovesDisplay();

        if (audioManager != null)
            audioManager.PlayCardFlip();

        if (flippedCards.Count >= 2 && !isCheckingMatches)
        {
            StartCoroutine(CheckForMatches());
        }
    }

    private IEnumerator CheckForMatches()
    {
        isCheckingMatches = true;
        
        List<Card> cardsToCheck = flippedCards.Take(2).ToList();

        foreach (var card in cardsToCheck)
        {
            flippedCards.Remove(card);
        }
        
        isCheckingMatches = false;

        yield return new WaitForSeconds(flipBackDelay);

        if (cardsToCheck.Count >= 2 && cardsToCheck[0].CardValue == cardsToCheck[1].CardValue)
        {
            cardsToCheck[0].SetMatched();
            cardsToCheck[1].SetMatched();
            score += 10;
            matchedPairs++;
            
            if (audioManager != null)
                audioManager.PlayMatch();
                
            UpdateScoreDisplay();
            
            if (matchedPairs >= totalPairs)
            {
                GameWon();
            }
        }
        else
        {
            if (audioManager != null)
                audioManager.PlayMismatch();
                
            foreach (Card card in cardsToCheck)
            {
                if (!card.IsMatched)
                {
                    card.FlipToBack();
                }
            }
        }
        SaveGame();

        if (flippedCards.Count >= 2)
        {
            StartCoroutine(CheckForMatches());
        }
    }

    private void GameWon()
    {
        gameActive = false;
        
        if (audioManager != null)
            audioManager.PlayGameOver();
        
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            
            if (finalScoreText != null)
                finalScoreText.text = "Score: " + score;
            
            if (finalMovesText != null)
                finalMovesText.text = "Moves: " + moves;
            
            if (finalTimeText != null)
                finalTimeText.text = "Time: " + FormatTime(gameTime);
        }
        ClearSaveData();
    }

    private void ResetGameStats()
    {
        score = 0;
        moves = 0;
        gameTime = 0f;
        matchedPairs = 0;
        flippedCards.Clear();
        isCheckingMatches = false;

        UpdateScoreDisplay();
        UpdateMovesDisplay();
        UpdateTimerDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    private void UpdateMovesDisplay()
    {
        if (movesText != null)
            movesText.text = "Moves: " + moves;
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
            timerText.text = "Time: " + FormatTime(gameTime);
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void SaveGame()
    {
        if (saveSystem != null && gameActive)
        {
            GameData data = new GameData
            {
                score = score,
                moves = moves,
                gameTime = gameTime,
                matchedPairs = matchedPairs,
                totalPairs = totalPairs,
                rows = currentRows,
                columns = currentColumns,
                cardStates = new List<CardState>()
            };

            foreach (Card card in allCards)
            {
                data.cardStates.Add(new CardState
                {
                    cardValue = card.CardValue,
                    isFlipped = card.IsFlipped,
                    isMatched = card.IsMatched,
                    position = card.transform.position
                });
            }
            saveSystem.SaveGame(data);
        }
    }

    public void LoadGame()
    {
        if (saveSystem != null)
        {
            GameData data = saveSystem.LoadGame();
            if (data != null)
            {
                currentRows = data.rows;
                currentColumns = data.columns;
                score = data.score;
                moves = data.moves;
                gameTime = data.gameTime;
                matchedPairs = data.matchedPairs;
                totalPairs = data.totalPairs;

                ClearBoard();
                RestoreBoard(data.cardStates);
                
                UpdateScoreDisplay();
                UpdateMovesDisplay();
                UpdateTimerDisplay();
                
                if (layoutDropdown != null)
                {
                    string loadedLayout = $"{currentRows}x{currentColumns}";
                    for (int i = 0; i < layoutDropdown.options.Count; i++)
                    {
                        if (layoutDropdown.options[i].text == loadedLayout)
                        {
                            layoutDropdown.SetValueWithoutNotify(i);
                            break;
                        }
                    }
                }

                gameActive = true;
            }
            else
            {
                StartNewGame();
            }
        }
    }

    private void RestoreBoard(List<CardState> cardStates)
    {
        CalculateCardLayout();
        int index = 0;
        for (int row = 0; row < currentRows; row++)
        {
            for (int col = 0; col < currentColumns; col++)
            {
                if (index < cardStates.Count)
                {
                    CreateCard(row, col, cardStates[index].cardValue);
                    Card card = allCards[allCards.Count - 1];

                    if (cardStates[index].isMatched)
                    {
                        card.SetMatched();
                    }
                    else if (cardStates[index].isFlipped)
                    {
                        card.ForceFlip();
                        flippedCards.Add(card);
                    }                    
                    index++;
                }
            }
        }
    }

    public void ClearSaveData()
    {
        if (saveSystem != null)
        {
            saveSystem.DeleteSave();
        }
    }

    public void RestartGame()
    {
        ClearSaveData();
        StartNewGame();
    }

    public void QuitGame()
    {
        SaveGame();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void ReturnToHome()
    {
        if (gameActive)
        {
            SaveGame(); // Auto-save current game
        }
        SceneController.LoadHomeScene();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && gameActive)
        {
            SaveGame();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && gameActive)
        {
            SaveGame();
        }
    }
}