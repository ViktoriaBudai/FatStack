using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour 
{
    public Image cardImage;
    public string suit;
    public string value;

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
