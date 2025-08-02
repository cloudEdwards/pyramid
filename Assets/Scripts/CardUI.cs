using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class CardUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Components")]
    public Image cardImage;
    public Button cardButton;
    
    private Card card;
    private Sprite[] cardSprites;
    
    public event Action<Card> OnCardClicked;
    
    void Awake()
    {
        if (cardImage == null)
        {
            cardImage = GetComponent<Image>();
        }
        
        if (cardButton == null)
        {
            cardButton = GetComponent<Button>();
        }
    }
    
    public void Initialize(Card card, Sprite[] cardSprites)
    {
        this.card = card;
        this.cardSprites = cardSprites;
        
        UpdateCardVisual();
    }
    
    void UpdateCardVisual()
    {
        if (cardImage == null) return;
        
        if (card.isFaceUp)
        {
            // Show the actual card sprite
            Sprite cardSprite = GetCardSprite();
            if (cardSprite != null)
            {
                cardImage.sprite = cardSprite;
            }
        }
        else
        {
            // Show card back (use a simple colored rectangle as card back)
            cardImage.sprite = CreateCardBackSprite();
        }
        
    }
    
    Sprite CreateCardBackSprite()
    {
        // Create a simple blue card back
        Texture2D texture = new Texture2D(64, 96);
        Color[] pixels = new Color[64 * 96];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.blue;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 64, 96), new Vector2(0.5f, 0.5f));
    }
    
    Sprite GetCardSprite()
    {
        if (cardSprites == null || cardSprites.Length == 0) return null;
        
        // Calculate the correct sprite index based on card value and suit
        int spriteIndex = GetSpriteIndex();
        
        // Ensure the index is within bounds
        if (spriteIndex >= 0 && spriteIndex < cardSprites.Length)
        {
            return cardSprites[spriteIndex];
        }
        
        // Fallback: return first sprite if index is out of bounds
        Debug.LogWarning($"Sprite index {spriteIndex} out of bounds for card {card.rank} of {card.suit}. Using fallback sprite.");
        return cardSprites[0];
    }
    
    int GetSpriteIndex()
    {
        // Standard playing card order: A, 2, 3, 4, 5, 6, 7, 8, 9, 10, J, Q, K
        // Suits order: Clubs, Diamonds, Hearts, Spades (alphabetical)
        
        int rankIndex = card.value - 1; // Convert 1-13 to 0-12
        int suitIndex = GetSuitIndex(card.suit);
        
        // Calculate sprite index: (suitIndex * 13) + rankIndex
        int spriteIndex = (suitIndex * 13) + rankIndex;
        
        return spriteIndex;
    }
    
    int GetSuitIndex(string suit)
    {
        switch (suit.ToLower())
        {
            case "clubs": return 0;
            case "diamonds": return 1;
            case "hearts": return 2;
            case "spades": return 3;
            default: return 0; // Default to clubs
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"CardUI OnPointerClick - Card: {card?.rank} of {card?.suit}");
        OnCardClicked?.Invoke(card);
    }
    
    public void SetInteractable(bool interactable)
    {
        if (cardButton != null)
        {
            cardButton.interactable = interactable;
        }
    }
    
    public void UpdateCard()
    {
        UpdateCardVisual();
    }
} 