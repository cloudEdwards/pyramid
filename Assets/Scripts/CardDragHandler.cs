using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Transform originalParent;
    private Card card;
    private GameManager gameManager;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        gameManager = FindFirstObjectByType<GameManager>();
        
        Debug.Log($"CardDragHandler Awake - Card: {card?.rank} of {card?.suit}, Canvas: {canvas}, GameManager: {gameManager}");
    }
    
    void Start()
    {
        Debug.Log($"CardDragHandler Start - Card: {card?.rank} of {card?.suit}");
    }
    
    public void SetCard(Card card)
    {
        this.card = card;
        Debug.Log($"CardDragHandler SetCard - Card: {card?.rank} of {card?.suit}");
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"OnBeginDrag called for card: {card?.rank} of {card?.suit}");
        
        // Only allow dragging if card is removable
        if (card != null && gameManager != null && gameManager.IsCardRemovable(card))
        {
            Debug.Log($"Starting drag for removable card: {card.rank} of {card.suit}");
            originalPosition = rectTransform.anchoredPosition;
            originalParent = transform.parent;
            
            // Set the card to be on top while dragging
            transform.SetParent(canvas.transform);
            
            // Make the card semi-transparent while dragging
            canvasGroup.alpha = 0.8f;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            Debug.Log($"Card not removable: {card?.rank} of {card?.suit}, isRemovable: {gameManager?.IsCardRemovable(card)}");
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (card != null && gameManager != null && gameManager.IsCardRemovable(card))
        {
            // Move the card with the mouse/touch
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (card != null && gameManager != null && gameManager.IsCardRemovable(card))
        {
            // Reset visual properties
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            
            // Check if dropped on a valid target
            bool validDrop = CheckValidDrop(eventData);
            
            if (validDrop)
            {
                // Handle the card removal
                gameManager.RemoveCardFromPyramid(card);
            }
            else
            {
                // Return to original position
                transform.SetParent(originalParent);
                rectTransform.anchoredPosition = originalPosition;
            }
        }
    }
    
    bool CheckValidDrop(PointerEventData eventData)
    {
        // Check if we dropped on a valid target (foundation area, waste area, etc.)
        // For now, we'll consider any drop valid if it's not on the pyramid
        // You can expand this logic based on your game rules
        
        // Check if dropped on foundation area
        if (gameManager.foundationContainer != null)
        {
            RectTransform foundationRect = gameManager.foundationContainer.GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(foundationRect, eventData.position, eventData.pressEventCamera))
            {
                return true;
            }
        }
        
        // Check if dropped on waste area
        if (gameManager.wasteContainer != null)
        {
            RectTransform wasteRect = gameManager.wasteContainer.GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(wasteRect, eventData.position, eventData.pressEventCamera))
            {
                return true;
            }
        }
        
        // Check if dropped outside the pyramid area
        if (gameManager.pyramidContainer != null)
        {
            RectTransform pyramidRect = gameManager.pyramidContainer.GetComponent<RectTransform>();
            if (!RectTransformUtility.RectangleContainsScreenPoint(pyramidRect, eventData.position, eventData.pressEventCamera))
            {
                return true;
            }
        }
        
        return false;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"CardDragHandler OnPointerClick - Card: {card?.rank} of {card?.suit}");
    }
} 