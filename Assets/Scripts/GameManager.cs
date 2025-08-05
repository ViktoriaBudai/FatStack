using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using Unity.VisualScripting;
using TMPro;

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
    private List<GameObject> middleCards = new List<GameObject>(); // what's visible in the middle area
    public List<GameObject> player1Cards = new List<GameObject>(); // Cards in Player 1 area
    public List<GameObject> player2Cards = new List<GameObject>(); // Cards in Player 2 area
    private Dictionary<Transform, int> playerScores = new Dictionary<Transform, int>();
    private bool canPlay = true; // new flag to enforce turns

    public TextMeshProUGUI cardPlayedInfoText;


    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // new code 05.08

    private IEnumerator Start()
    {
        InitializeDeck();
        ShuffleDeck();

        // deal the opening 4 cards/ player
        yield return StartCoroutine(DealStartingCards());

        // now random pick who will starts
        currentPlayer = Random.Range(0, 2); // 0 or 1
        canPlay = true;

        // update UI, players know who goes first
        string starterName = currentPlayer == 0 ? "Player 1" : "Player 2";
        Color starterColor = currentPlayer == 0 ? Color.green : Color.red;

        cardPlayedInfoText.text = $"{starterName} starts the game!";
        cardPlayedInfoText.color = starterColor;

        Debug.Log($"{starterName} begins.");

    }
   

    // old Start() method, for test reson comented out 05.08.
    /*private void Start()
    {
        InitializeDeck();
        ShuffleDeck();
        StartCoroutine(DealStartingCards());
    }
    */

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
        // old version
        //deck = deck.OrderBy(card => Random.value).ToList();

        //new version 06.24.
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            Card temp = deck[i];
            deck[i] = deck[rand];
            deck[rand] = temp;
        }

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
        // new version modifyed 05.08.
        // must be your turn
        if (!canPlay)
        {
            Debug.LogWarning("That's not your turn!");
            return;
        }

        // card must belong to current player
        var owner = card.GetComponent<CardOwner>();
        if (owner == null || owner.ownerPlayerId != currentPlayer)
        {
            Debug.LogWarning($"You can only play your own cards. It is Player {currentPlayer + 1}'s turn!");
            return;
        }

        // new version, for test reson commented out 05.08.
        /*if (!canPlay)
        {
            Debug.LogWarning($"It's not Player {currentPlayer + 1}'s turn! You cannot play right now.");
           
        }
        // ensure that only the current player can place a card
        if (card.transform.parent != GetCurrentPlayerArea())
        {
            Debug.LogWarning($"It's Player {currentPlayer + 1}'s turn! You cannot play right now.");
            
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
        }*/

        // new part in the script 06.24.

        // Add to visual middle list
        middleCards.Add(card);
        // Add to data history list
        CardUI cardUI = card.GetComponent<CardUI>();
        playedCards.Add(new Card(cardUI.suit, cardUI.value, false));
        // new part end

        // disable actions until the next turn starts, modified 05.08. belong to the previous version
        //canPlay = false;

        card.transform.SetParent(middleArea, false);

        // this is a new code 07.15./ modified 05.08.
        /*string playerName = currentPlayer == 0 ? "Player 1" : "Player 2";
        cardPlayedInfoText.text = $"{playerName} placed a card in the middle.";
        cardPlayedInfoText.color = currentPlayer == 0 ? Color.blue : Color.red;*/

        // new version 05.08.
        string playerName = owner.ownerPlayerId == 0 ? "Player 1" : "Player 2";
        Color textColor = owner.ownerPlayerId == 0 ? Color.green : Color.red;
        cardPlayedInfoText.text = $"{playerName} placed the card in the middle.";
        cardPlayedInfoText.color = textColor;

        canPlay = false;

        // new code ends here 07.15.
        card.transform.SetAsLastSibling(); // Ensures top visual layer, new version 06.24.
        card.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // new code 06.24

        // old 06.24. midified
        //card.transform.SetSiblingIndex(middleArea.childCount - 1); // ensure that the card appears on the top
        //card.transform.position = new Vector3(card.transform.position.x, card.transform.position.y, -middleArea.childCount);

        // new set up 06.24, for the fade-in effect
        CanvasGroup cg = card.AddComponent<CanvasGroup>();
       

        lastPlacedCard = card; // track last played card

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

        //new 08.07
        // assign player ownership to the card
        CardOwner owner = cardObj.GetComponent<CardOwner>();
        if (owner != null)
        {
            owner.ownerPlayerId = (playerArea == player1Area) ? 0 : 1;
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
        
        foreach (Sprite sprite in cardSprites)
        {
            if (sprite.name.Equals($"{card.suit}_{card.value}", System.StringComparison.OrdinalIgnoreCase))
            {
                return sprite; // Found the exact matching sprite
            }
        }

        Debug.LogError($"No matching sprite found for {card.suit} {card.value}");
        return null; // Return null or a default placeholder sprite
        
    }
}
