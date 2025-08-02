using UnityEngine;

public class CardPrefabCreator : MonoBehaviour
{
    void Start()
    {
        CreateCardPrefab();
    }
    
    void CreateCardPrefab()
    {
        // Create card GameObject
        GameObject cardPrefab = new GameObject("CardPrefab");
        cardPrefab.tag = "Card";
        
        // Add SpriteRenderer
        SpriteRenderer spriteRenderer = cardPrefab.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = "Default";
        
        // // Add BoxCollider2D
        // BoxCollider2D boxCollider = cardPrefab.AddComponent<BoxCollider2D>();
        // boxCollider.size = new Vector2(1f, 1.4f); // Card aspect ratio
        // boxCollider.enabled = true;
        // boxCollider.isTrigger = true;
        
        // // Add Rigidbody2D
        // Rigidbody2D rigidbody2D = cardPrefab.AddComponent<Rigidbody2D>();
        // rigidbody2D.gravityScale = 0f;
        // rigidbody2D.linearDamping = 5f;
        // rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Add CardGameObject component
        cardPrefab.AddComponent<CardGameObject>();
        
        // Set scale
        cardPrefab.transform.localScale = new Vector3(1f, 1f, 1f);
        
        Debug.Log("Card prefab created successfully!");
    }
} 