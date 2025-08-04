using UnityEngine;
using System.Collections;

public class CardGameObject : MonoBehaviour
{
    [Header("Components")]
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D boxCollider;

    [Header("Card Data")]
    public Card card;
    public GameManager gameManager;

    [HideInInspector]
    public bool isAnimating = false;

    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 originalPosition;
    private string originalSortingLayerName;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        gameManager = FindObjectOfType<GameManager>();
    }

    public void Initialize(Card card, Sprite cardSprite)
    {
        this.card = card;
        spriteRenderer.sprite = cardSprite;
        if (spriteRenderer.sprite != null)
        {
            boxCollider.size = spriteRenderer.bounds.size;
        }
    }

    void OnMouseDown()
    {
        if (isAnimating || card == null || gameManager == null) return;

        if (card.isInStock)
        {
            gameManager.DrawCard();
            return;
        }

        if (gameManager.IsCardRemovable(card))
        {
            isDragging = true;
            originalPosition = transform.position;
            originalSortingLayerName = spriteRenderer.sortingLayerName;
            spriteRenderer.sortingLayerName = "Dragged";
            offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);

            gameManager.StartDrag(card);
        }
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset;
            transform.position = new Vector3(newPosition.x, newPosition.y, originalPosition.z);
        }
    }

    void OnMouseUp()
    {
        if (!isDragging) return;

        isDragging = false;

        Card targetCard = null;
        
        boxCollider.enabled = false;
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        boxCollider.enabled = true;

        if (hit.collider != null)
        {
            CardGameObject otherCardGO = hit.collider.GetComponent<CardGameObject>();
            if (otherCardGO != null)
            {
                targetCard = otherCardGO.card;
            }
        }

        bool success = gameManager.ProcessDrop(targetCard);

        if (!success)
        {
            StartCoroutine(AnimateMove(originalPosition, 0.2f, true));
        }
        else
        {
            spriteRenderer.sortingLayerName = originalSortingLayerName;
        }

        gameManager.EndDrag(success);
    }

    public IEnumerator AnimateToFoundation(Vector3 targetPosition, float duration)
    {
        isAnimating = true;
        boxCollider.enabled = false;
        yield return StartCoroutine(AnimateMove(targetPosition, duration, false));
        spriteRenderer.sprite = gameManager.GetCardBackSprite();
        yield return new WaitForSeconds(0.5f); // Pause briefly on the foundation
        Destroy(gameObject);
    }

    private IEnumerator AnimateMove(Vector3 targetPosition, float duration, bool restoreLayer)
    {
        float elapsedTime = 0;
        Vector3 startingPosition = transform.position;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startingPosition, targetPosition, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        if (restoreLayer)
        {
            spriteRenderer.sortingLayerName = originalSortingLayerName;
        }
    }
}


 