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
        rectTransform.position = Mouse.current.position.ReadValue(); // move the card with the mouse
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // first check the card is dropped in the middle area

        if (RectTransformUtility.RectangleContainsScreenPoint(GameManager.Instance.middleArea, Input.mousePosition))
        {
            rectTransform.position = GameManager.Instance.middleArea.position; // move to the middle
        }
        else
        {
            rectTransform.position = originalPosition; // return to the original place
        }

        canvasGroup.blocksRaycasts = true; // restore raycast
    }



}
