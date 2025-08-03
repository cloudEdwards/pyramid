using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int pyramidRows = 7;
    public int stockCards = 24;
    public float pyramidCardSpacing = 1.2f;
    public float pyramidRowSpacing = 1.5f;
    
    [Header("UI References")]
    public Transform pyramidContainer;
    public Transform stockContainer;
    public Transform wasteContainer;
    public Transform foundationContainer;
    public Button resetButton;
    public Button drawButton;
    public Button undoButton;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI movesText;
    
    
    [Header("Card Prefab")]
    public GameObject cardGameObjectPrefab;
    
    // Game state
    private List<Card> deck = new List<Card>();
    private List<Card> pyramid = new List<Card>();
    private List<Card> stock = new List<Card>();
    private List<Card> waste = new List<Card>();
    private List<Card> foundation = new List<Card>();
    
    private int score = 0;
    private int moves = 0;
    private Stack<GameAction> actionHistory = new Stack<GameAction>();
    
    // Card sprites
    private Sprite[] cardSprites;
    private Sprite[] cardBackSprites;
    
    // Selection logic
    private Card selectedCard;
    
    void Start()
    {
        LoadCardSprites();
        InitializeGame();
        SetupUI();
    }
    
    void LoadCardSprites()
    {
        cardSprites = Resources.LoadAll<Sprite>("Cards trimmed");
        cardBackSprites = Resources.LoadAll<Sprite>("Cards backs");
        if (cardSprites == null || cardSprites.Length == 0)
        {
            Debug.LogError("Failed to load card sprites!");
            CreateFallbackSprites();
        }
    }
    
    void CreateFallbackSprites()
    {
        cardSprites = new Sprite[52];
        string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };
        string[] ranks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        for (int suit = 0; suit < 4; suit++)
        {
            for (int rank = 0; rank < 13; rank++)
            {
                Color cardColor = suit < 2 ? Color.red : Color.black;
                cardSprites[suit * 13 + rank] = CreateSimpleSprite(cardColor, ranks[rank] + "_of_" + suits[suit]);
            }
        }
    }
    
    Sprite CreateSimpleSprite(Color color, string name)
    {
        Texture2D texture = new Texture2D(64, 96);
        Color[] pixels = new Color[64 * 96];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 64, 96), new Vector2(1f, 1f));
    }
    
    void InitializeGame()
    {
        score = 0;
        moves = 0;
        actionHistory.Clear();
        selectedCard = null;
        
        CreateDeck();
        ShuffleDeck();
        DealPyramid();
        DealStock();
        UpdateUI();
    }
    
    void CreateDeck()
    {
        deck.Clear();
        string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };
        string[] ranks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        for (int suit = 0; suit < 4; suit++)
        {
            for (int rank = 0; rank < 13; rank++)
            {
                deck.Add(new Card(ranks[rank], suits[suit], rank + 1));
            }
        }
    }
    
    void ShuffleDeck()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Card temp = deck[i];
            deck[i] = deck[j];
            deck[j] = temp;
        }
    }
    
    void DealPyramid()
    {
        pyramid.Clear();
        int cardIndex = 0;
        for (int row = 0; row < pyramidRows; row++)
        {
            for (int col = 0; col <= row; col++)
            {
                if (cardIndex < deck.Count)
                {
                    Card card = deck[cardIndex++];
                    card.row = row;
                    card.col = col;
                    card.isInPyramid = true;
                    card.isFaceUp = true; 
                    pyramid.Add(card);
                }
            }
        }
    }
    
    void DealStock()
    {
        stock.Clear();
        waste.Clear();
        foundation.Clear();
        
        for (int i = pyramid.Count; i < deck.Count; i++)
        {
            Card card = deck[i];
            card.isInStock = true;
            card.isFaceUp = false;
            stock.Add(card);
        }
    }
    
    void SetupUI()
    {
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetGame);
        if (drawButton != null)
            drawButton.onClick.AddListener(DrawCard);
        if (undoButton != null)
            undoButton.onClick.AddListener(UndoLastAction);
        
        UpdateUI();
    }

    void UpdateUI()
    {
        // Clear all card GameObjects
        foreach (Transform child in pyramidContainer) Destroy(child.gameObject);
        foreach (Transform child in stockContainer) Destroy(child.gameObject);
        foreach (Transform child in wasteContainer) Destroy(child.gameObject);
        foreach (Transform child in foundationContainer) Destroy(child.gameObject);

        CreatePyramidUI();
        CreateStockUI();
        CreateWasteUI();
        
        if (scoreText != null) scoreText.text = "Score: " + score;
        if (movesText != null) movesText.text = "Moves: " + moves;
    }
    
    void CreatePyramidUI()
    {
        foreach (Card card in pyramid)
        {
            CreateCardGameObject(card, GetPyramidPosition(card.row, card.col), pyramidContainer);
        }
    }
    
    Vector3 GetPyramidPosition(int row, int col)
    {
        float xOffset = (col - row * 0.5f) * pyramidCardSpacing;
        float yOffset = (pyramidRows - 1 - row) * pyramidRowSpacing;
        return new Vector3(xOffset, yOffset - 1, 0);
    }
    
    void CreateStockUI()
    {
        if (stock.Count > 0)
        {
            Card topCard = stock.Last();
            // Stock cards are face down until drawn
            CreateCardGameObject(topCard, stockContainer.position, stockContainer);
        }
    }
    
    void CreateWasteUI()
    {
        if (waste.Count > 0)
        {
            Card topCard = waste.Last();
            topCard.isFaceUp = true;
            CreateCardGameObject(topCard, wasteContainer.position, wasteContainer);
        }
    }
    
    GameObject CreateCardGameObject(Card card, Vector3 position, Transform parent)
    {
        if (cardGameObjectPrefab == null) return null;
        
        GameObject cardObj = Instantiate(cardGameObjectPrefab, position, Quaternion.identity, parent);
        cardObj.tag = "Card";
        
        CardGameObject cardGameObject = cardObj.GetComponent<CardGameObject>();
        Sprite cardSprite = GetCardSprite(card);
        cardGameObject.Initialize(card, cardSprite);
        
        if (card.isInPyramid)
        {
            SetCardSortingLayer(cardObj, position.y);
        }
        
        return cardObj;
    }
    
    Sprite GetCardSprite(Card card)
    {
        if (!card.isFaceUp)
        {
            if (cardBackSprites != null && cardBackSprites.Length > 0)
            {
                return cardBackSprites[0];
            }
            // Return card back sprite, assuming it's the last one
            return cardSprites[cardSprites.Length - 1];
        }
        int suitIndex = GetSuitIndex(card.suit);
        int rankIndex = card.value - 1;
        int spriteIndex = (suitIndex * 13) + rankIndex;
        if (spriteIndex >= 0 && spriteIndex < cardSprites.Length)
        {
            return cardSprites[spriteIndex];
        }
        return cardSprites[0]; // Fallback
    }
    
    int GetSuitIndex(string suit)
    {
        switch (suit.ToLower())
        {
            case "clubs": return 0;
            case "diamonds": return 1;
            case "hearts": return 2;
            case "spades": return 3;
            default: return 0;
        }
    }
    
    void SetCardSortingLayer(GameObject cardObj, float yPosition)
    {
        SpriteRenderer spriteRenderer = cardObj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = Mathf.RoundToInt(-yPosition * 10);
        }
    }

    public bool IsCardRemovable(Card card)
    {
        if (card == null) return false;

        if (card.isInPyramid)
        {
            if (!card.isFaceUp) return false;
            bool isCovered = pyramid.Any(c => c.row == card.row + 1 && (c.col == card.col || c.col == card.col + 1));
            return !isCovered;
        }

        // Only top of waste is selectable. Stock is not directly selectable.
        return card.isInWaste && waste.Count > 0 && waste.Last() == card;
    }

    public void SelectCard(Card card)
    {
        if (!IsCardRemovable(card))
        {
            Debug.Log($"Card not selectable: {card?.rank} of {card?.suit}");
            return;
        }

        if (card.value == 13) // King
        {
            RemoveMatchedCard(card);
            score += 5;
            moves++;
            UpdateUI();
        }
        else if (selectedCard == null)
        {
            selectedCard = card;
            Debug.Log($"Selected card: {card.rank} of {card.suit}");
        }
        else
        {
            if (selectedCard != card)
            {
                TryRemovePair(selectedCard, card);
            }
            selectedCard = null;
        }
    }

    void TryRemovePair(Card card1, Card card2)
    {
        if (card1.value + card2.value == 13)
        {
            RemoveMatchedCard(card1);
            RemoveMatchedCard(card2);
            score += 10;
            moves++;
            UpdateUI();
        }
        else
        {
            Debug.Log("Cards do not sum to 13");
        }
    }
    
    void RemoveMatchedCard(Card card)
    {
        if (card.isInPyramid) RemoveCardFromPyramid(card);
        else if (card.isInWaste) RemoveCardFromWaste(card);
    }

    public void RemoveCardFromPyramid(Card card)
    {
        actionHistory.Push(new GameAction { actionType = GameActionType.RemoveFromPyramid, card = card });
        pyramid.Remove(card);
        card.isInPyramid = false;
        card.isInFoundation = true;
        foundation.Add(card);
        RevealCardsAbove(card.row, card.col);
    }

    void RemoveCardFromWaste(Card card)
    {
        actionHistory.Push(new GameAction { actionType = GameActionType.RemoveFromWaste, card = card });
        waste.Remove(card);
        card.isInWaste = false;
        card.isInFoundation = true;
        foundation.Add(card);
    }
    
    void RevealCardsAbove(int row, int col)
    {
        // This logic can be improved, but is out of scope of the current issue
    }
    
    public void ResetGame()
    {
        InitializeGame();
    }

    public void DrawCard()
    {
        if (stock.Count > 0)
        {
            Card card = stock.Last();
            stock.Remove(card);
            card.isInStock = false;
            card.isInWaste = true;
            card.isFaceUp = true;
            waste.Add(card);
            actionHistory.Push(new GameAction { actionType = GameActionType.DrawCard, card = card });
            moves++;
            UpdateUI();
        }
        else
        {
            ReshuffleWaste();
        }
    }
    
    void ReshuffleWaste()
    {
        if (waste.Count == 0) return;
        
        stock.AddRange(waste);
        waste.Clear();
        
        foreach(var card in stock)
        {
            card.isInStock = true;
            card.isFaceUp = false;
            card.isInWaste = false;
        }

        ShuffleDeck(); // Re-shuffle the entire deck for simplicity
        UpdateUI();
    }
    
    void UndoLastAction()
    {
        if (actionHistory.Count == 0) return;
        
        GameAction action = actionHistory.Pop();
        
        switch (action.actionType)
        {
            case GameActionType.RemoveFromPyramid:
                foundation.Remove(action.card);
                action.card.isInFoundation = false;
                action.card.isInPyramid = true;
                pyramid.Add(action.card);
                // This doesn't re-cover cards, but is a simple undo
                break;
                
            case GameActionType.RemoveFromWaste:
                foundation.Remove(action.card);
                action.card.isInFoundation = false;
                action.card.isInWaste = true;
                waste.Add(action.card);
                break;

            case GameActionType.DrawCard:
                waste.Remove(action.card);
                action.card.isInWaste = false;
                action.card.isInStock = true;
                action.card.isFaceUp = false;
                stock.Add(action.card);
                break;
        }
        
        moves--;
        UpdateUI();
    }
}

[System.Serializable]
public class Card
{
    public string rank;
    public string suit;
    public int value;
    public int row;
    public int col;
    public bool isFaceUp;
    public bool isInPyramid;
    public bool isInStock;
    public bool isInWaste;
    public bool isInFoundation;
    
    public Card(string rank, string suit, int value)
    {
        this.rank = rank;
        this.suit = suit;
        this.value = value;
    }
}

public enum GameActionType
{
    RemoveFromPyramid,
    RemoveFromWaste,
    DrawCard
}

[System.Serializable]
public class GameAction
{
    public GameActionType actionType;
    public Card card;
}