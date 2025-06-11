using UnityEngine;
using UnityEngine.EventSystems;

public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        originalScale = transform.localScale; // store original scale
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // scale up the cards a little and change the border color
        transform.localScale = originalScale * 1.5f;
        spriteRenderer.color = Color.yellow;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // reset scale and color
        transform.localScale = originalScale;
        spriteRenderer.color = Color.white;
    }
}