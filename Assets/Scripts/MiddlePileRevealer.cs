using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MiddlePileRevealer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private RectTransform middleArea;     // the container where cards sit
    [SerializeField] private Image highlightImage;         // optional background image to tint on hover
    [SerializeField] private Image topCardImage;           // shows the top card face
    [SerializeField] private TextMeshProUGUI countText;    // shows count

    [Header("Layout")]
    [SerializeField] private float spacing = 120f;
    [SerializeField] private float baseY = 0f;
    [SerializeField] private float revealDuration = 0.35f;
    [SerializeField] private float collapseDuration = 0.25f;

    private readonly List<GameObject> middleCardGOs = new();
    private bool isPointerOver;

    // GameManager will call this whenever the middle pile changes
    public void SetMiddleCards(IReadOnlyList<GameObject> cards)
    {
        middleCardGOs.Clear();
        if (cards != null) middleCardGOs.AddRange(cards);

        // Update UI badges
        if (countText) countText.text = middleCardGOs.Count.ToString();

        if (topCardImage)
        {
            if (middleCardGOs.Count > 0)
            {
                var top = middleCardGOs[middleCardGOs.Count - 1];
                var ui = top ? top.GetComponent<CardUI>() : null;
                topCardImage.sprite = ui ? ui.CurrentSprite : null;
                topCardImage.enabled = topCardImage.sprite != null;
            }
            else
            {
                topCardImage.sprite = null;
                topCardImage.enabled = false;
            }
        }

        // If hover is active, keep layout revealed; otherwise keep collapsed
        if (isPointerOver) Reveal();
        else Collapse();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        if (highlightImage) highlightImage.color = new Color(1f, 1f, 0.7f, 1f);
        Reveal();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        if (highlightImage) highlightImage.color = Color.white;
        Collapse();
    }

    private void Reveal()
    {
        if (middleCardGOs.Count == 0) return;

        for (int i = 0; i < middleCardGOs.Count; i++)
        {
            var go = middleCardGOs[i];
            if (!go) continue;

            var rt = go.GetComponent<RectTransform>();
            // Centered spread: first played left, last right
            Vector2 target = new Vector2((i - middleCardGOs.Count / 2f) * spacing, baseY);

            // Ensure they’re under the middle area and on top visually
            if (rt.parent != middleArea) rt.SetParent(middleArea, worldPositionStays: false);
            rt.SetAsLastSibling();

            // Cancel any ongoing tween to avoid fights
            LeanTween.cancel(rt);
            LeanTween.move(rt, target, revealDuration).setEase(LeanTweenType.easeOutExpo);
        }
    }

    private void Collapse()
    {
        if (middleCardGOs.Count == 0) return;

        for (int i = 0; i < middleCardGOs.Count; i++)
        {
            var go = middleCardGOs[i];
            if (!go) continue;

            var rt = go.GetComponent<RectTransform>();
            LeanTween.cancel(rt);
            LeanTween.move(rt, Vector2.zero, collapseDuration).setEase(LeanTweenType.easeInQuad);
        }
    }
}

