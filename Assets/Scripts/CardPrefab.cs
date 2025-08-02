using UnityEngine;
using UnityEngine.UI;

// This script helps set up the card prefab with the necessary components
public class CardPrefab : MonoBehaviour
{
    void Awake()
    {
        // Ensure we have an Image component
        if (GetComponent<Image>() == null)
        {
            gameObject.AddComponent<Image>();
        }
        
        // Ensure we have a Button component
        if (GetComponent<Button>() == null)
        {
            gameObject.AddComponent<Button>();
        }
        
        // Set up the button to work with the CardUI
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.transition = Selectable.Transition.None;
        }
        
        // Ensure we have a CanvasGroup for dragging
        if (GetComponent<CanvasGroup>() == null)
        {
            gameObject.AddComponent<CanvasGroup>();
        }
        
        // Ensure the Image has raycast target enabled
        Image image = GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = true;
        }
        
        // Set default size for cards
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(.8f, 1);
        }
    }
} 