using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

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
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI winText;
    
    
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
    private float timer;
    private bool isTimerRunning;
    
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
        timer = 0f;
        isTimerRunning = true;
        
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
            drawButton.onClick.AddListener(ReshuffleWaste);
        if (undoButton != null)
            undoButton.onClick.AddListener(UndoLastAction);
        
        UpdateUI();
    }

    private Dictionary<Card, CardGameObject> cardGameObjectMap = new Dictionary<Card, CardGameObject>();

    void UpdateUI()
    {
        // Clear all card GameObjects that are not animating
        foreach (var entry in cardGameObjectMap)
        {
            if (entry.Value != null && !entry.Value.isAnimating)
            {
                Destroy(entry.Value.gameObject);
            }
        }
        cardGameObjectMap.Clear();

        CreatePyramidUI();
        CreateStockUI();
        CreateWasteUI();
        
        if (scoreText != null) scoreText.text = "Score: " + score;
        if (movesText != null) movesText.text = "Moves: " + moves;

        if (drawButton != null)
        {
            drawButton.gameObject.SetActive(stock.Count == 0 && waste.Count > 0);
        }

        if (winText != null)
        {
            winText.gameObject.SetActive(pyramid.Count == 0);
            if (pyramid.Count == 0) isTimerRunning = false;
        }
    }

    void Update()
    {
        if (isTimerRunning)
        {
            timer += Time.deltaTime;
            UpdateTimerText();
        }
    }

    void UpdateTimerText()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timer / 60F);
            int seconds = Mathf.FloorToInt(timer % 60F);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void UpdateFoundationUI()
    {
        foreach (Transform child in foundationContainer) Destroy(child.gameObject);
        CreateFoundationUI();
    }

    void CreatePyramidUI()
    {
        foreach (Card card in pyramid)
        {
            CreateCardGameObject(card, GetPyramidPosition(card.row, card.col), pyramidContainer);
        }
    }
    
    void CreateStockUI()
    {
        if (stock.Count > 0)
        {
            Card topCard = stock.Last();
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

    void CreateFoundationUI()
    {
        if (foundation.Count > 0)
        {
            // Create a single card back to represent the foundation pile
            GameObject cardObj = Instantiate(cardGameObjectPrefab, foundationContainer.position, Quaternion.identity, foundationContainer);
            CardGameObject cardGameObject = cardObj.GetComponent<CardGameObject>();
            cardGameObject.Initialize(null, GetCardBackSprite()); // No card data needed, just the sprite
            cardGameObject.GetComponent<BoxCollider2D>().enabled = false; // Not interactable
        }
    }
    
    GameObject CreateCardGameObject(Card card, Vector3 position, Transform parent)
    {
        if (cardGameObjectPrefab == null) return null;
        
        GameObject cardObj = Instantiate(cardGameObjectPrefab, position, Quaternion.identity, parent);
        CardGameObject cardGameObject = cardObj.GetComponent<CardGameObject>();
        Sprite cardSprite = GetCardSprite(card);
        cardGameObject.Initialize(card, cardSprite);
        cardGameObjectMap[card] = cardGameObject;
        
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
            return GetCardBackSprite();
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

    public Sprite GetCardBackSprite()
    {
        if (cardBackSprites != null && cardBackSprites.Length > 0)
        {
            return cardBackSprites[0];
        }
        return cardSprites[cardSprites.Length - 1]; // Fallback
    }
    
    Vector3 GetPyramidPosition(int row, int col)
    {
        float xOffset = (col - row * 0.5f) * pyramidCardSpacing;
        float yOffset = (pyramidRows - 1 - row) * pyramidRowSpacing;
        return new Vector3(xOffset, yOffset - 1, 0);
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

    private Card currentlyDraggedCard;

    public void StartDrag(Card card)
    {
        currentlyDraggedCard = card;
        if (card.isInPyramid)
        {
            pyramid.Remove(card);
        }
        else if (card.isInWaste)
        {
            ShowPreviousWasteCard();
        }
    }

    public void EndDrag(bool success)
    {
        if (currentlyDraggedCard == null) return;

        if (!success)
        {
            if (currentlyDraggedCard.isInPyramid)
            {
                pyramid.Add(currentlyDraggedCard);
            }
        }

        if (currentlyDraggedCard.isInWaste)
        {
            HidePreviousWasteCard();
        }

        currentlyDraggedCard = null;
    }

    public bool ProcessDrop(Card targetCard)
    {
        if (currentlyDraggedCard == null) return false;

        bool isMatch = false;
        Card card1 = currentlyDraggedCard;
        Card card2 = targetCard;

        // Case 1: Dropped a King on an empty space
        if (card1.value == 13 && card2 == null)
        {
            isMatch = true;
        }
        // Case 2: Dropped a card on another card
        else if (card2 != null && IsCardRemovable(card2))
        {
            if (card1.value + card2.value == 13)
            {
                isMatch = true;
            }
        }

        if (isMatch)
        {
            StartCoroutine(HandleSuccessfulMatch(card1, card2));
        }

        return isMatch;
    }

    private IEnumerator HandleSuccessfulMatch(Card card1, Card card2)
    {
        // Find the game objects before they are destroyed
        CardGameObject card1GO = cardGameObjectMap.ContainsKey(card1) ? cardGameObjectMap[card1] : null;
        CardGameObject card2GO = (card2 != null && cardGameObjectMap.ContainsKey(card2)) ? cardGameObjectMap[card2] : null;

        // Mark them as animating so they aren't destroyed by UpdateUI
        if (card1GO != null) card1GO.isAnimating = true;
        if (card2GO != null) card2GO.isAnimating = true;

        // Update the game state data
        if (card1.isInPyramid) score += 5;
        if (card2 != null && card2.isInPyramid) score += 5;
        if (scoreText != null) scoreText.text = "Score: " + score;

        if (card2 == null) // King
        {
            RemoveMatchedCard(card1);
        }
        else
        {
            RemoveMatchedCard(card1);
            RemoveMatchedCard(card2);
        }
        moves++;

        // Redraw the UI, which will leave the animating cards alone
        UpdateUI();

        // Now, animate the cards to the foundation
        if (card1GO != null) StartCoroutine(card1GO.AnimateToFoundation(foundationContainer.position, 0.3f));
        if (card2GO != null) StartCoroutine(card2GO.AnimateToFoundation(foundationContainer.position, 0.3f));

        // Wait for animations to finish before updating the foundation
        yield return new WaitForSeconds(0.4f); 
        UpdateFoundationUI();
    }

    public void RemoveMatchedCard(Card card)
    {
        if (card.isInPyramid) RemoveCardFromPyramid(card);
        else if (card.isInWaste) RemoveCardFromWaste(card);
    }

    private GameObject tempWasteCardGO;

    public void ShowPreviousWasteCard()
    {
        if (waste.Count > 1)
        {
            if (tempWasteCardGO != null) Destroy(tempWasteCardGO);
            Card secondCard = waste[waste.Count - 2];
            tempWasteCardGO = CreateCardGameObject(secondCard, wasteContainer.position, wasteContainer);
        }
    }

    public void HidePreviousWasteCard()
    {
        if (tempWasteCardGO != null)
        {
            Destroy(tempWasteCardGO);
            tempWasteCardGO = null;
        }
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