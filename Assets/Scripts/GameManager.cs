using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour 
{
    // Singleton
    public static GameManager Instance;

    public List<Card> deck = new List<Card>(); // deck of cards
    public Transform player1Area; // UI area for player 1
    public Transform player2Area; // UI area for player 2
    public GameObject cardPrefab; // card UI prefab
    public Sprite[] cardSprites; // array of card images

    public Transform deckTransform; // deck object reference

    
    public RectTransform middleArea;
    public Transform player1WinPile; // Player 1's winning cards
    public Transform player2WinPile; // Player 2's winning cards

    private GameObject lastPlacedCard;
    private int currentPlayer = 0; // player1[0], player2[1]
    private List<Card> playedCards = new List<Card>(); // stores played cards
    public List<GameObject> player1Cards = new List<GameObject>(); // Cards in Player 1 area
    public List<GameObject> player2Cards = new List<GameObject>(); // Cards in Player 2 area
    private Dictionary<Transform, int> playerScores = new Dictionary<Transform, int>();
    private bool canPlay = true; // new flag to enforce turns

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        InitializeDeck();
        ShuffleDeck();
        StartCoroutine(DealStartingCards());
    }

   
    void InitializeDeck()
    {
        string[] suits = { "acorn", "heart", "leaf", "bell" };
        string[] values = { "Ace", "10", "queen", "over", "under", "9", "8", "7" };

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

    IEnumerator DealStartingCards()
    {

        for (int i = 0; i < 4; i++)
        {
            yield return new WaitForSeconds(0.3f); // delay between each cards
            DrawCard(player1Area);
            yield return new WaitForSeconds(0.3f);
            DrawCard(player2Area);
        }
        
    }


    public void PlayCard(GameObject card)
    {
        // new version
        if (!canPlay)
        {
            Debug.LogWarning($"It's not Player {currentPlayer + 1}'s turn! You cannot play right now.");
            return;
        }
        // ensure that only the current player can place a card
        if (card.transform.parent != GetCurrentPlayerArea())
        {
            Debug.LogWarning($"It's Player {currentPlayer + 1}'s turn! You cannot play right now.");
            return;
        }

        // this is a test, if it is not working remove this
        // **Remove the card from the player's list**
        if (currentPlayer == 0)
        {
            player1Cards.Remove(card);
        }
        else
        {
            player2Cards.Remove(card);
        }

        // disable actions until the next turn starts
        canPlay = false;

        card.transform.SetParent(middleArea, false);

        card.transform.SetSiblingIndex(middleArea.childCount - 1); // ensure that the card appears on the top
        card.transform.position = new Vector3(card.transform.position.x, card.transform.position.y, -middleArea.childCount);
        //old
        //card.transform.SetAsLastSibling();

        lastPlacedCard = card; // Track last played card

        // old version
        /*card.transform.SetParent(middleArea, false);
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.anchoredPosition = middleArea.anchoredPosition;*/


        CardUI cardUI = card.GetComponent<CardUI>();
        playedCards.Add(new Card(cardUI.suit, cardUI.value, false));
        

        //Debug.Log($"Player {currentPlayer + 1} played {card.name}");
        StartCoroutine(NextTurn());
    }
    

    public void DrawNextCard()
    {
        //new
        if (!canPlay)
        {
            Debug.LogWarning("It's not your turn! Wait for the next move.");
            return;
        }

        if (lastPlacedCard != null)
        {
            Debug.LogWarning("Player must place a card before drawing a new one!");
            return;
        }
        Debug.Log("Player is allowed to draw a card!");
        // new
        Transform playerArea = GetCurrentPlayerArea();
        DrawCard(playerArea);

    }

    void DrawCard(Transform playerArea)
    {
        Card card = GetNextCard();
        if (card == null)
        {
            Debug.LogWarning("No more cards left in the deck!");
            return;
        }

        GameObject cardObj = InstantiateCard(card, deckTransform);
        deck.Remove(card);

        if (cardObj == null)
        {
            Debug.LogError("Card object failed to instantiate!");
            return;
        }

        // new part if it is not working, strat from here to debug, something is not okay with that still
        // Assign the card to the correct player's list
        if (playerArea == player1Area)
        {
            player1Cards.Add(cardObj);
        }
            
        else
        {
            player2Cards.Add(cardObj);
        }
            

        LeanTween.move(cardObj, playerArea.position, 1f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => cardObj.transform.SetParent(playerArea, false));
    }


    // this is also new, I added with the above one, so if it is not working just remove
    void UpdatePlayerAreas()
    {
        // Update Player 1 cards visually
        foreach (GameObject card in player1Cards)
        {
            card.transform.SetParent(player1Area, false);
        }

        // Update Player 2 cards visually
        foreach (GameObject card in player2Cards)
        {
            card.transform.SetParent(player2Area, false);
        }
    }


    /*IEnumerator DetermineWinner()
    {
        yield return new WaitForSeconds(1f);

        Transform winner = GetWinningPlayer();
        MoveCardsToWinner(winner);
    }
    void MoveCardsToWinner(Transform winner)
    {
        int pointsGained = 0;

        foreach (Transform card in middleArea)
        {
            if (card.name.Contains("Ace") || card.name.Contains("10"))
            {
                pointsGained += 10;
            }

            card.SetParent(winner == player1Area ? player1WinPile : player2WinPile);
            card.gameObject.SetActive(false);
        }

        playerScores[winner] += pointsGained;
        Debug.Log($"{winner.name} gained {pointsGained} points! Total score: {playerScores[winner]}");

        StartCoroutine(NextTurn());
    }

    Transform GetWinningPlayer()
    {
        GameObject bestCard = middleArea.GetChild(0).gameObject;
        Transform winner = bestCard.transform.parent;

        Debug.Log($"{winner.name} wins this round!");

        return winner;
    }*/

    IEnumerator NextTurn()
    {
        yield return new WaitForSeconds(0.5f);
        currentPlayer = (currentPlayer == 0) ? 1 : 0;
        canPlay = true; // Enable the next player's turn
        Debug.Log($"Player {currentPlayer + 1}'s turn!");
        
    }


    Card GetNextCard()
    {
        if (deck.Count > 0)
        {
            Card card = deck[0];
            deck.RemoveAt(0);
            return card;
        }
        return null;
    }

    public GameObject InstantiateCard(Card card, Transform playerArea)
    {
        GameObject newCard = Instantiate(cardPrefab);
        newCard.transform.SetParent(playerArea, false);
        //change this back if it is not working
        //newCard.GetComponent<RectTransform>().anchoredPosition = playerArea.GetComponent<RectTransform>().anchoredPosition;
        newCard.SetActive(true);

        CardUI cardUI = newCard.GetComponent<CardUI>();
        cardUI.SetCard(GetCardSprite(card), card.suit, card.value);

        return newCard;
    }

    public Transform GetCurrentPlayerArea()
    {
        return (currentPlayer == 0) ? player1Area : player2Area;
    }



    Sprite GetCardSprite(Card card)
    {
        //new
        foreach (Sprite sprite in cardSprites)
        {
            if (sprite.name.Equals($"{card.suit}_{card.value}", System.StringComparison.OrdinalIgnoreCase))
            {
                return sprite; // Found the exact matching sprite
            }
        }

        Debug.LogError($"No matching sprite found for {card.suit} {card.value}");
        return null; // Return null or a default placeholder sprite
        //old
        /*foreach (Sprite sprite in cardSprites)
        {
            if (sprite.name.Contains(card.suit) && sprite.name.Contains(card.value))
            {
                return sprite; // found the matching sprite
            }
        }

        //Debug.LogError($"No matching sprite found for {card.suit} {card.value}");
        return cardSprites[Random.Range(0, cardSprites.Length)];*/
    }
}
