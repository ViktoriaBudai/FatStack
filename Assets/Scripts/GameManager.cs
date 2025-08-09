using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using TMPro;
using UnityEngine.UI;

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

    // new 08-08
    public List<TrickRecord> trickHistory = new List<TrickRecord>();
    private Dictionary<Transform, int> playerScores = new Dictionary<Transform, int>();
    private bool canPlay = true; // new flag to enforce turns

    public TextMeshProUGUI cardPlayedInfoText;
    [SerializeField] private MiddlePileRevealer middleRevealer; // new 08-08

    // new 08.06
    //new 08-07 11:35, commented out the next line
    //private bool[] hasLeftRound;

    // new modified now 08.06.
    public int CurrentPlayerIndex => currentPlayer;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return; // new 08.06. 15:25
        }
        
        //hasLeftRound = new bool[2] { false, false }; // commented out 08-07 11:35
        
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

        // new 08-07
        SetupTurn();

        // update UI, players know who goes first
        string starterName = currentPlayer == 0 ? "Player 1" : "Player 2";
        Color starterColor = currentPlayer == 0 ? Color.green : Color.red;

        cardPlayedInfoText.text = $"{starterName} starts the game!";
        cardPlayedInfoText.color = starterColor;

        Debug.Log($"{starterName} begins.");

    }

    // new 08.06. / called by LeaveManager when a player click leave / commented out 08-07 11:35
    /*public void OnPlayerLeaveRound(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex > 1) return;

        if (hasLeftRound[playerIndex])
        {
            Debug.LogWarning($"Player {playerIndex + 1} already left this round.");
            return;
        }

        hasLeftRound[playerIndex] = true;
        Debug.Log($"Player {playerIndex + 1} has left the round.");

        // Disable that player’s card buttons
        DisablePlayerCards(playerIndex);

        // Advance to next active turn
        StartCoroutine(NextTurn());
    }*/
    // replace the above code with this 08-07 11:35
    // new
    // Called by your "Skip Turn" UI button
    public void OnSkipTurnButton()
    {
        if (!canPlay) return;
        Debug.Log($"Player {currentPlayer + 1} skipped their turn.");

        canPlay = false;
        SetupTurn();
        StartCoroutine(NextTurn());
    }

  
    public void SetPlayerCardsInteractable(int playerIndex, bool interactable)
    {
        // old 08-07
        /*var list = GetCardList(playerIndex);
        foreach (var card in list)
        {
            // ensure CanvasGroup to block raycasts
            var cg = card.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = card.AddComponent<CanvasGroup>();

            cg.blocksRaycasts = interactable;

            // disable/enable drag script
            var drag = card.GetComponent<CardDrag>();
            if (drag != null)
                drag.enabled = interactable;
        }*/
        // new 08-07
        var list = (playerIndex == 0) ? player1Cards : player2Cards;
        foreach (var card in list)
        {
            // Block or unblock raycasts
            var cg = card.GetComponent<CanvasGroup>();
            if (cg == null) cg = card.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = interactable;

            // Enable or disable drag script
            var drag = card.GetComponent<CardDrag>();
            if (drag != null) drag.enabled = interactable;
        }

    }
    public void DisablePlayerCards(int playerIndex)
    {
        SetPlayerCardsInteractable(playerIndex, false);
    }

    public void EnablePlayerCards(int playerIndex)
    {
        SetPlayerCardsInteractable(playerIndex, true);
    }

    // new code 08-08 
    private void RefreshMiddleUI()
    {
        if (middleRevealer != null)
            middleRevealer.SetMiddleCards(middleCards);
    }

    // old
    /*private void SetupTurn()
    {
        for (int i = 0; i < 2; i++)
        {
            var ok = (i == currentPlayer && canPlay);
            SetPlayerCardsInteractable(i, ok);
        }
    }*/
    // new 08-07
    private void SetupTurn()
    {
        // Enable only the active player's cards
        for (int i = 0; i < 2; i++)
            SetPlayerCardsInteractable(i, i == currentPlayer && canPlay);
    }


    // new ends here 08.06 nnnnnn

    // new 08.06. I added the private word infront of the function
    private IEnumerator NextTurn()
    {
        // new 08-07 11:35
        yield return new WaitForSeconds(0.5f);

        // Flip between 0 and 1
        currentPlayer = (currentPlayer + 1) % 2;
        canPlay = true;
        SetupTurn();

        var name = currentPlayer == 0 ? "Player 1" : "Player 2";
        var color = currentPlayer == 0 ? Color.green : Color.red;
        cardPlayedInfoText.text = $"{name}'s turn to play.";
        cardPlayedInfoText.color = color;
        Debug.Log($"{name} now has the turn.");
        

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


        // Add to visual middle list
        middleCards.Add(card);
        // Add to data history list
        CardUI cardUI = card.GetComponent<CardUI>();
        playedCards.Add(new Card(cardUI.suit, cardUI.value, false));
        // new part end

        card.transform.SetParent(middleArea, false);


        // new version 05.08.
        string playerName = owner.ownerPlayerId == 0 ? "Player 1" : "Player 2";
        Color textColor = owner.ownerPlayerId == 0 ? Color.green : Color.red;
        cardPlayedInfoText.text = $"{playerName} placed the card in the middle.";
        cardPlayedInfoText.color = textColor;

        canPlay = false;
        // new put here 08-07 11:15
        SetupTurn();
        // ends here

        // new code ends here 07.15.
        card.transform.SetAsLastSibling(); // Ensures top visual layer, new version 06.24.
        card.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // new code 06.24


        // new set up 06.24, for the fade-in effect
        CanvasGroup cg = card.AddComponent<CanvasGroup>();
       

        lastPlacedCard = card; // track last played card

        // notify the UI
        RefreshMiddleUI();

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
