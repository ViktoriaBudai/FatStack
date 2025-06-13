using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class GameManager : MonoBehaviour 
{
    public List<Card> deck = new List<Card>(); // deck of cards
    public Transform player1Area; // UI area for player 1
    public Transform player2Area; // UI area for player 2
    public GameObject cardPrefab; // card UI prefab
    public Sprite[] cardSprites; // array of card images

    public Transform deckTransform; // deck object reference

    public static GameManager Instance;
    public RectTransform middleArea;
    public Transform player1WinPile; // Player 1's winning cards
    public Transform player2WinPile; // Player 2's winning cards

    private bool isPlayer1Turn = true;
    private GameObject lastPlacedCard;
    private Dictionary<Transform, int> playerScores = new Dictionary<Transform, int>();

    private void Start()
    {
        Instance = this;
        playerScores[player1Area] = 0;
        playerScores[player2Area] = 0;

        InitializeDeck();
        ShuffleDeck();
        StartCoroutine(DealStartingCards());
    }

    void Awake()
    {
        Instance = this;
    }

    void InitializeDeck()
    {
        string[] suits = { "acorn", "heart", "leaf", "bell" };
        string[] values = { "Ace", "10", "queen", "upper", "under", "9", "8", "7" };

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
        card.transform.SetParent(middleArea, false);
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.anchoredPosition = middleArea.anchoredPosition;

        lastPlacedCard = card;

        if(!isPlayer1Turn)
        {
            Debug.Log("AI Player2 place a card! Drawing automayically.");
            DrawCard(player2Area);
        }
        else
        {
            StartCoroutine(ForcePlayer1ToDraw());
        }
        StartCoroutine(NextTurn());
    }
    IEnumerator AI_PlayTurn()
    {
        yield return new WaitForSeconds(1f);

        GameObject aiCard = player2Area.GetChild(Random.Range(0, player2Area.childCount)).gameObject;
        PlayCard(aiCard);
    }



    IEnumerator AI_DrawNextCard()
    {
        yield return new WaitForSeconds(0.5f);
        if (player2Area.childCount < 4)
        {
            DrawCard(player2Area);
        }
    }

    IEnumerator ForcePlayer1ToDraw()
    {
        yield return new WaitForSeconds(0.5f);
        DrawNextCard();
    }

    public void DrawNextCard()
    {
        if (lastPlacedCard != null)
        {
            Debug.LogWarning("Player must place a card before drawing a new one!");
            return;
        }
        Debug.Log("Player is allowed to draw a card!");
        DrawCard(GetCurrentPlayerArea());
        lastPlacedCard = null;
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
        // new
        LeanTween.move(cardObj, playerArea.position, 1f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => cardObj.transform.SetParent(playerArea, false));

        // old version
        /*RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        cardRect.anchoredPosition = playerArea.GetComponent<RectTransform>().anchoredPosition;
        Debug.Log($"Card '{card.value} of {card.suit}' drawn!");*/
        //LeanTween.move(cardObj, playerArea.position, 2f).setEase(LeanTweenType.easeOutQuad);
    }

    IEnumerator DetermineWinner()
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
    }

    IEnumerator NextTurn()
    {
        yield return new WaitForSeconds(0.5f);
        isPlayer1Turn = !isPlayer1Turn; // alternate turns

        if (!isPlayer1Turn)
        {
            StartCoroutine(AI_PlayTurn());
        }
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
        newCard.GetComponent<RectTransform>().anchoredPosition = playerArea.GetComponent<RectTransform>().anchoredPosition;
        newCard.SetActive(true);

        CardUI cardUI = newCard.GetComponent<CardUI>();
        cardUI.SetCard(GetCardSprite(card), card.suit, card.value);

        return newCard;
    }

    Transform GetCurrentPlayerArea()
    {
        return isPlayer1Turn ? player1Area : player2Area;
    }

    Sprite GetCardSprite(Card card)
    {

        foreach (Sprite sprite in cardSprites)
        {
            if (sprite.name.Contains(card.suit) && sprite.name.Contains(card.value))
            {
                return sprite; // found the matching sprite
            }
        }

        //Debug.LogError($"No matching sprite found for {card.suit} {card.value}");
        return cardSprites[Random.Range(0, cardSprites.Length)];
    }
}
