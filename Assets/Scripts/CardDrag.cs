using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler 
{
    private Vector3 originalPosition;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.position;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false; // it is allows to drag over UI elements
    }

    public void OnDrag(PointerEventData eventData)
    {
        //rectTransform.position = Mouse.current.position.ReadValue(); // move the card with the mouse

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos;

        //Camera camera = eventData.pressEventCamera != null ? eventData.pressEventCamera : Camera.main;
        // converts a screen position, mouse position into a word position relative to a UI element ( a RectTransform), so objects follow the cursor naturally
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, mousePos, eventData.pressEventCamera, out worldPos);

        rectTransform.position = worldPos; // move UI element to world position 
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //old version
        // first check the card is dropped in the middle area
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (RectTransformUtility.RectangleContainsScreenPoint(GameManager.Instance.middleArea, mousePos))
        {
            rectTransform.position = GameManager.Instance.middleArea.position; // move to the middle
        }
        else
        {
            rectTransform.position = originalPosition; // return to the original place
        }

        canvasGroup.blocksRaycasts = true; // restore raycast

        //new version
        // Prevent dragging if it's not the correct player's turn
        /*if (GameManager.Instance.GetCurrentPlayerArea() != transform.parent)
        {
            Debug.LogWarning("You cannot place a card right now. Wait for your turn.");
            //rectTransform.position = originalPosition; // Reset position
            return;
        }

        // Ensure card is dropped in the correct area
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (RectTransformUtility.RectangleContainsScreenPoint(GameManager.Instance.middleArea, mousePos))
        {
            rectTransform.SetParent(GameManager.Instance.middleArea, false);
            // new
            //rectTransform.anchoredPosition = GameManager.Instance.middleArea.anchoredPosition;
            // Make sure new cards always appear on top
            rectTransform.SetSiblingIndex(GameManager.Instance.middleArea.childCount - 1);
        
            GameManager.Instance.PlayCard(gameObject); // Register the card in GameManager
        }
        else
        {
            rectTransform.position = originalPosition; // Return to the original place
        }

        canvasGroup.blocksRaycasts = true; // Restore raycast*/
    }

}
