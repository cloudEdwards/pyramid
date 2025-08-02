using UnityEngine;
using UnityEngine.UI;

public class CardGameObject : MonoBehaviour
{
    [Header("Components")]
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D boxCollider;
    
    
    [Header("Card Data")]
    public Card card;
    public GameManager gameManager;
    
    [Header("Drag Settings")]
    public float dragSpeed = 10f;
    public float returnSpeed = 15f;
    
    private Vector3 originalPosition;
    
    private Camera mainCamera;
    
    void Awake()
    {
        // Get or add required components
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // boxCollider = GetComponent<BoxCollider2D>();
        // if (boxCollider == null)
        // {
        //     boxCollider = gameObject.AddComponent<BoxCollider2D>();
        // }
        
        // rigidbody2D = GetComponent<Rigidbody2D>();
        // if (rigidbody2D == null)
        // {
        //     rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
        //     rigidbody2D.gravityScale = 0f;
        //     rigidbody2D.linearDamping = 5f;
        // }
        
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        gameManager = FindFirstObjectByType<GameManager>();
    }
    
    public void Initialize(Card card, Sprite cardSprite)
    {
        this.card = card;
        if (spriteRenderer != null && cardSprite != null)
        {
            spriteRenderer.sprite = cardSprite;
        }
        
        // Set collider size based on sprite
        if (boxCollider != null && cardSprite != null)
        {
            boxCollider.size = cardSprite.bounds.size;
        }
        
        Debug.Log($"CardGameObject initialized: {card.rank} of {card.suit}");
    }
    
        void OnMouseDown()
    {
        if (card != null && gameManager != null && gameManager.IsCardRemovable(card))
        {
            gameManager.SelectCard(card);
        }
        else
        {
            Debug.Log($"Card not selectable: {card?.rank} of {card?.suit}");
        }
    }
} 