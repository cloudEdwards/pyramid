using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RaycastDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse button clicked!");
            DebugRaycast();
        }
    }
    
    void Start()
    {
        Debug.Log($"RaycastDebugger Start - EventSystem: {EventSystem.current}");
    }
    
    void DebugRaycast()
    {
        Vector2 mousePosition = Input.mousePosition;
        Debug.Log($"Mouse clicked at: {mousePosition}");
        
        if (EventSystem.current == null)
        {
            Debug.LogError("No EventSystem found!");
            return;
        }
        
        // Check what UI elements are under the mouse
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = mousePosition;
        
        System.Collections.Generic.List<RaycastResult> results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        Debug.Log($"Raycast hit {results.Count} UI elements:");
        for (int i = 0; i < results.Count; i++)
        {
            var result = results[i];
            Debug.Log($"  {i}: {result.gameObject.name} - {result.gameObject.GetComponent<Image>()?.raycastTarget} - SortingOrder: {result.sortingOrder}");
        }
        
        if (results.Count == 0)
        {
            Debug.Log("No UI elements hit by raycast!");
        }
    }
} 