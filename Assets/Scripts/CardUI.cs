using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public Image cardImage;
    // new 08-12
    public Card card { get; private set; }
    public string suit;
    public string value;
    // new 08-12
    public int Points => card?.points ?? 0;
    public bool IsTrump => card?.IsTrump() ?? false;
    // new 08-08
    public Sprite CurrentSprite => cardImage != null ? cardImage.sprite : null;
    public string Id => $"{suit}_{value}"; // optional, handy for debugging/history


    public void SetCard(Sprite sprite, string suit, string value)
    {
        if (cardImage == null) 
        {
            Debug.LogError("CardUI: cardImage reference is missing!");
            return;
        }
        cardImage.sprite = sprite;
        this.suit = suit;
        this.value = value;
    }
    
}
