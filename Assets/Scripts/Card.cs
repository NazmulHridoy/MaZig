using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Individual card behavior handling flip animations, state management, and player interaction
/// Implements IPointerClickHandler for direct click detection
/// Each card manages its own visual state and communicates with GameManager when clicked
/// </summary>
public class Card : MonoBehaviour, IPointerClickHandler
{
    [Header("Card Visual Components")]
    [SerializeField] private Image cardImage; 
    [SerializeField] private Image cardBackImage;
    [SerializeField] private CanvasGroup canvasGroup;
    
    private int cardValue;
    private Sprite cardFaceSprite;
    private Sprite cardBackSprite;
    
    private bool isFlipped = false;
    private bool isMatched = false;
    private bool isAnimating = false;
    private float flipDuration = 0.3f;
    
    private GameManager gameManager;

    public int CardValue => cardValue; 
    public bool IsFlipped => isFlipped;
    public bool IsMatched => isMatched;

    private void Awake()
    {
        if (cardImage == null)
            cardImage = GetComponent<Image>();
        
        if (cardBackImage == null)
        {
            GameObject backObj = new GameObject("CardBack");
            backObj.transform.SetParent(transform, false);
            cardBackImage = backObj.AddComponent<Image>();
            
            RectTransform rt = backObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Initialize(int value, Sprite faceSprite, Sprite backSprite, float animDuration = 0.3f)
    {
        cardValue = value;
        cardFaceSprite = faceSprite;
        cardBackSprite = backSprite;
        flipDuration = animDuration;

        cardImage.sprite = cardFaceSprite;
        cardBackImage.sprite = cardBackSprite;

        gameManager = GameManager.Instance;
        
        ShowFace();
        ResetCard();
    }

    public void ShowPreview()
    {
        StopAllCoroutines();
        isAnimating = false;
        isFlipped = false;
        ShowFace();
        transform.localEulerAngles = Vector3.zero;
    }

    public void HidePreview()
    {
        StartCoroutine(FlipToBackAnimation());
    }


    private IEnumerator FlipToBackAnimation()
    {
        isAnimating = true;
        
        yield return StartCoroutine(RotateCard(0f, 90f));
        
        ShowBack();
        
        yield return StartCoroutine(RotateCard(90f, 0f));

        isFlipped = false;
        isAnimating = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isAnimating && !isMatched && gameManager != null)
        {
            gameManager.CardClicked(this);
        }
    }

    public void Flip()
    {
        if (!isAnimating && !isMatched)
        {
            StartCoroutine(FlipAnimation());
        }
    }

    public void FlipToBack()
    {
        if (!isAnimating && !isMatched && isFlipped)
        {
            StartCoroutine(FlipAnimation());
        }
    }

    public void ForceFlip()
    {
        isFlipped = true;
        ShowFace();
    }

    private IEnumerator FlipAnimation()
    {
        isAnimating = true;
        
        if (isFlipped)
        {
            yield return StartCoroutine(RotateCard(0f, 90f));
            ShowBack();
            yield return StartCoroutine(RotateCard(90f, 0f));
            isFlipped = false;
        }
        else
        {
            yield return StartCoroutine(RotateCard(0f, 90f));
            ShowFace();
            yield return StartCoroutine(RotateCard(90f, 0f));
            isFlipped = true;
        }
        
        isAnimating = false;
    }

    private IEnumerator RotateCard(float fromAngle, float toAngle)
    {
        float elapsedTime = 0f;
        Vector3 startRotation = transform.localEulerAngles;

        while (elapsedTime < flipDuration / 2f)
        {
            float t = elapsedTime / (flipDuration / 2f);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            float currentAngle = Mathf.Lerp(fromAngle, toAngle, smoothT);
            transform.localEulerAngles = new Vector3(0f, currentAngle, 0f);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.localEulerAngles = new Vector3(0f, toAngle, 0f);
    }

    private void ShowFace()
    {
        cardImage.enabled = true;
        cardBackImage.enabled = false;
    }

    private void ShowBack()
    {
        cardImage.enabled = false;
        cardBackImage.enabled = true;
    }

    public void SetMatched()
    {
        isMatched = true;
        isFlipped = true;
        ShowFace();
        StartCoroutine(MatchAnimation());
    }

    private IEnumerator MatchAnimation()
    {
        float duration = 0.3f;
        float elapsedTime = 0f;
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.1f;
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale = Vector3.Lerp(originalScale, targetScale, smoothT);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale = Vector3.Lerp(targetScale, originalScale, smoothT);
            canvasGroup.alpha = Mathf.Lerp(1f, 0.5f, smoothT);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.localScale = originalScale;
        canvasGroup.alpha = 0.5f;
    }


    private void ResetCard()
    {
        isFlipped = false;
        isMatched = false;
        isAnimating = false;
        
        canvasGroup.alpha = 1f;
        transform.localScale = Vector3.one;
        transform.localEulerAngles = Vector3.zero;
        
        ShowBack();
    }
    
    public void ResetForPool()
    {
        StopAllCoroutines();
        ResetCard();
        gameManager = null;
        cardValue = -1;
        cardFaceSprite = null;
        cardBackSprite = null;
    }
}
