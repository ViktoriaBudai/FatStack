using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // new
    [SerializeField] private Vector2 handSize = new Vector2(120f, 180f);
    [SerializeField] private Vector2 playedSizeMiddleArea = new Vector2(120f, 180f);

    private Vector3 originalPosition;
    private Transform originalParent;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform dragLayer;

    // new 08.05
    // captured each time you start dragging
    private Vector2 originalAnchoredPos;
    private Vector2 originalSizeDelta;
    private Vector3 originalLocalScale;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        //old
        /*originalPosition = rectTransform.position;
        originalParent = transform.parent;*/


        // find the "DragLayer" under the root Canvas
        var rootCanvas = GetComponentInParent<Canvas>().transform;
        dragLayer = rootCanvas.Find("DragLayer");
        if (dragLayer == null)
            Debug.LogError("DragLayer not found under Canvas!");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //old
        /*originalParent = transform.parent;
        originalPosition = rectTransform.position;*/
        // new 08.05.
        // Capture everything we’ll need to restore on a “peek”
        originalParent = transform.parent;
        originalAnchoredPos = rectTransform.anchoredPosition;
        originalSizeDelta = rectTransform.sizeDelta;
        originalLocalScale = rectTransform.localScale;

        transform.SetParent(dragLayer, worldPositionStays: false); // reparent to the DragLayer so it visually floats above all piles
        rectTransform.SetAsLastSibling(); // bring this card to the very front of its Canvas

        canvasGroup.blocksRaycasts = false; // it is allows to drag over UI elements
        // new 
        if (originalParent == GameManager.Instance.middleArea)
        {
            rectTransform.sizeDelta = this.handSize;
            rectTransform.localScale = Vector3.one;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, mousePos, eventData.pressEventCamera, out worldPos);

        rectTransform.position = worldPos; // move UI element to world position 
    }


    // new 08.05.
    public void OnEndDrag(PointerEventData eventData)
    {
        bool droppedInMiddle = RectTransformUtility.RectangleContainsScreenPoint(
            GameManager.Instance.middleArea,
            Mouse.current.position.ReadValue(),
            eventData.pressEventCamera);

        bool startedInMiddle = originalParent == GameManager.Instance.middleArea;

        if (droppedInMiddle && !startedInMiddle)
        {
            // actually playing from hand into middle
            transform.SetParent(GameManager.Instance.middleArea, false);
            CenterInParent(rectTransform);
            GameManager.Instance.PlayCard(gameObject);
        }
        else
        {
            //old
            // snap back exactly where you came from
            /*transform.SetParent(originalParent, false);
            rectTransform.position = originalPosition;*/

            // new 08.05.
            // peek-drag or missed drop: restore **exactly** what we captured
            transform.SetParent(originalParent, worldPositionStays: false);
            rectTransform.anchoredPosition = originalAnchoredPos;
            rectTransform.sizeDelta = originalSizeDelta;
            rectTransform.localScale = originalLocalScale;
        }

        canvasGroup.blocksRaycasts = true;
    }

    private void CenterInParent(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one; // keeps the card from shrinking
        rt.sizeDelta = playedSizeMiddleArea;
    }

}
