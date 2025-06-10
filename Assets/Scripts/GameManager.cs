using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour 
{
    public List<Card> deck = new List<Card>(); // deck of cards
    public Transform player1Area; // UI area for player 1
    public Transform player2Area; // UI area for player 2
    public GameObject cardPrefab; // card UI prefab
    public Sprite[] cardSprites; // array of card images

    private void Start()
    {
        InitializeDeck();
        ShuffleDeck();
        DealCards();
    }

    void InitializeDeck()
    {
        string[] suits = { "acorn", "heart", "leaf", "bell" };
        string[] values = { "Ace", "10", "Queen", "upper", "under", "9", "8", "7" };

        foreach (string suit in suits)
        {
            foreach (string value in values)
            {
                deck.Add (new Card(suit, value, false));
            }
        }
    }

    void ShuffleDeck()
    {
        deck = deck.OrderBy(card => Random.value).ToList();
    }

    void DealCards()
    {
        AssignCardsToPlayer(player1Area);
        AssignCardsToPlayer(player2Area);
    }

    void AssignCardsToPlayer(Transform playerArea)
    {
        for (int i = 0; i < 4; i++)
        {
            if (deck.Count > 0)
            {
                Card card = deck[0]; // pick the top card
                deck.RemoveAt(0); // remove from the deck

                GameObject newCard = Instantiate(cardPrefab, playerArea);
                newCard.transform.SetParent(playerArea, false);

                CardUI cardUI = newCard.GetComponent<CardUI>();
                cardUI.SetCard(GetCardSprite(card), card.suit, card.value);
            }
        }
    }

    Sprite GetCardSprite(Card card)
    {
        if (cardSprites.Length == 0)
        {
            Debug.LogError("CardSprites array is empty!");
            return null;
        }

        foreach (Sprite sprite in cardSprites)
        {
            if (sprite.name.Contains(card.suit) && sprite.name.Contains(card.value))
            {
                return sprite; // found the matching sprite
            }
        }

        Debug.LogError($"No matching sprite found for {card.suit} {card.value}");
        return cardSprites[Random.Range(0, cardSprites.Length)];
    }
}
