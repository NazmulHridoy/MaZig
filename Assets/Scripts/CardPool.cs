using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object pooling system for Card objects to reduce instantiation/destruction overhead
/// Manages a pool of reusable card GameObjects for optimal performance
/// </summary>
public class CardPool : MonoBehaviour
{
    [Header("Pool Configuration")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private int initialPoolSize = 36;
    [SerializeField] private Transform poolContainer;
    
    private Queue<Card> availableCards = new Queue<Card>();
    private List<Card> activeCards = new List<Card>();
    private bool isInitialized = false;
    
    private void Awake()
    {
        if (cardPrefab != null)
        {
            InitializePool();
        }
    }
    
    private void InitializePool()
    {
        if (isInitialized || cardPrefab == null)
            return;
            
        if (poolContainer == null)
        {
            poolContainer = transform;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewCard();
        }
        
        isInitialized = true;
    }
    
    private Card CreateNewCard()
    {
        if (cardPrefab == null)
            return null;
        
        GameObject cardObject = Instantiate(cardPrefab, poolContainer);
        Card card = cardObject.GetComponent<Card>();
        
        if (card == null)
        {
            Destroy(cardObject);
            return null;
        }
        
        cardObject.SetActive(false);
        availableCards.Enqueue(card);
        
        return card;
    }
    
    public Card GetCard()
    {
        Card card = null;

        if (!isInitialized && cardPrefab != null)
        {
            InitializePool();
        }
        
        if (availableCards.Count > 0)
        {
            card = availableCards.Dequeue();
        }
        else if (cardPrefab != null)
        {
            GameObject cardObject = Instantiate(cardPrefab, poolContainer);
            card = cardObject.GetComponent<Card>();
            
            if (card == null)
            {
                Destroy(cardObject);
                return null;
            }
        }
        
        if (card != null)
        {
            card.gameObject.SetActive(true);
            activeCards.Add(card);
        }
        
        return card;
    }
    
    public void ReturnCard(Card card)
    {
        if (card == null) return;
        
        card.gameObject.SetActive(false);
        card.transform.SetParent(poolContainer);
        
        // Reset card state
        card.ResetForPool();
        
        activeCards.Remove(card);
        availableCards.Enqueue(card);
        
    }
    
    public void ReturnAllCards()
    {
        for (int i = activeCards.Count - 1; i >= 0; i--)
        {
            ReturnCard(activeCards[i]);
        }
    }
    
    public void SetCardPrefab(GameObject prefab)
    {
        cardPrefab = prefab;
        InitializePool();
    }
}