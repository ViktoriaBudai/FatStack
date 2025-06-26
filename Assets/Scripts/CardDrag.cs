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
        //old version, try someting new 06.24.
        // first check the card is dropped in the middle area
        /*Vector2 mousePos = Mouse.current.position.ReadValue();

        if (RectTransformUtility.RectangleContainsScreenPoint(GameManager.Instance.middleArea, mousePos))
        {
            rectTransform.position = GameManager.Instance.middleArea.position; // move to the middle
        }
        else
        {
            rectTransform.position = originalPosition; // return to the original place
        }

        canvasGroup.blocksRaycasts = true; // restore raycast*/

        //trying out new version 06.24.

        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (RectTransformUtility.RectangleContainsScreenPoint(GameManager.Instance.middleArea, mousePos))
        {
            transform.SetParent(GameManager.Instance.middleArea, false);
            transform.SetAsLastSibling();

            RectTransform rt = GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            // this is works well, but new test 06.24
            rt.localScale = Vector3.one; // keeps the card from shrinking
            rt.sizeDelta = new Vector2(240f, 560f); // lock to same size as in hand, for some reson I need bigger scale for the played cards, so I made this manually

   

            GameManager.Instance.PlayCard(gameObject);
        }
        else
        {
            rectTransform.position = originalPosition;
        }

        canvasGroup.blocksRaycasts = true;
    }

}
