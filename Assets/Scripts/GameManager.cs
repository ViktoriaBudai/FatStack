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
    public readonly string[] suits = { "acorn", "heart", "leaf", "bell" };
    public readonly string[] values = { "Ace", "10", "queen", "over", "under", "9", "8", "7" };


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
    private List<PlayedCard> playedCards = new List<PlayedCard>(); // stores played cards
    private List<GameObject> middleCardObjects = new List<GameObject>(); // what's visible in the middle area

    public List<GameObject> player1Cards = new List<GameObject>(); // Cards in Player 1 area
    public List<GameObject> player2Cards = new List<GameObject>(); // Cards in Player 2 area

    // new 08-08
    public List<TrickRecord> trickHistory = new List<TrickRecord>();
    private Dictionary<Transform, int> playerScores = new Dictionary<Transform, int>();
    private bool canPlay = true; // new flag to enforce turns

    public TextMeshProUGUI cardPlayedInfoText;
    [SerializeField] private MiddlePileRevealer middleRevealer; // new 08-08

    public int CurrentPlayerIndex => currentPlayer;
    private int trickStartIndex = 0; // to track where each trick starts, it needs to update this when a trick finishes

    public Button passButton;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return; 
        }
        
    }


    private IEnumerator Start()
    {
        InitializeDeck();
        ShuffleDeck();

        // deal the opening 4 cards/ player
        yield return StartCoroutine(DealStartingCards());

        // now random pick who will starts
        currentPlayer = Random.Range(0, 2); // 0 or 1
        canPlay = true;

        SetupTurn();

        // update UI, players know who goes first
        string starterName = currentPlayer == 0 ? "Player 1" : "Player 2";
        Color starterColor = currentPlayer == 0 ? Color.green : Color.red;

        cardPlayedInfoText.text = $"{starterName} starts the game!";
        cardPlayedInfoText.color = starterColor;

        Debug.Log($"{starterName} begins.");

    }

    
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

    private void RefreshMiddleUI()
    {
        if (middleRevealer != null)
            middleRevealer.SetMiddleCards(middleCardObjects);
    }

    
    private void SetupTurn()
    {
        // Enable only the active player's cards
        for (int i = 0; i < 2; i++)
            SetPlayerCardsInteractable(i, i == currentPlayer && canPlay);
    }


 
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
        foreach (string suit in suits)
        {
            foreach (string value in values)
            {
                deck.Add(new Card(suit, value, false));
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

        // when cards are played remove it from players hand
        if (owner.ownerPlayerId == 0)
        {
            player1Cards.Remove(card);
        }
        else
        {
            player2Cards.Remove(card);
        }

        // add to visual middle list
        middleCardObjects.Add(card);
        // add to data history list
        CardUI cardUI = card.GetComponent<CardUI>();
        playedCards.Add(new PlayedCard(new Card(cardUI.suit, cardUI.value, false), currentPlayer));

        card.transform.SetParent(middleArea, false);

        string playerName = owner.ownerPlayerId == 0 ? "Player 1" : "Player 2";
        Color textColor = owner.ownerPlayerId == 0 ? Color.green : Color.red;
        cardPlayedInfoText.text = $"{playerName} placed the card in the middle.";
        cardPlayedInfoText.color = textColor;

        card.transform.SetAsLastSibling(); // Ensures top visual layer
        card.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        // new set up for the fade-in effect
        CanvasGroup cg = card.AddComponent<CanvasGroup>();

        lastPlacedCard = card; // track last played card

        
        if (playedCards.Count % 2 == 0)
        {
            // even number in middle, just received the cover card (or second card of trick)
            int leaderIndex = playedCards[trickStartIndex].player;
            string leadValue = playedCards[trickStartIndex].card.value;

            // the card just played (cover)
            string coverValue = playedCards[playedCards.Count - 1].card.value;

            // if cover value didn't match lead and isn't a 7, it was a free discard -> leader wins now
            if (!(coverValue == leadValue || coverValue == "7"))
            {
                ResolveTrick(leaderIndex);
                return;
            }

            int moves = GetPlayableCardsForNextPlayer(leaderIndex, true);

            if (moves == 0)
            {
                int winner = DetermineRoundWinner();
                ResolveTrick(winner);
                return;
            }
            currentPlayer = leaderIndex;
            return; // wait for leader’s play or pass
        }
        else
        {
            int nextIndex = (currentPlayer + 1) % 2;
            GetPlayableCardsForNextPlayer(nextIndex, false);
            currentPlayer = nextIndex;
            return; // wait for cover
        }
        // Check if both players have played this trick
        /*if (playedCards.Count % 2 == 0)
        {
            int available = GetPlayableCardsForNextPlayer();

            if (available > 0)
            {
                // Show the pass button to the next player
                ShowPassButton();
                NextTurn();
                // Do not advance turn automatically—wait for player action
            }
            else
            {
                // No pass option: resolve trick
                int winner = DetermineRoundWinner();
                ResolveTrick(winner);
                Debug.Log("Round winner is " + (winner == 0 ? "Player 1" : "Player 2"));

                // Update captured cards UI
                CapturedCards();

                // Set next player and advance turn
                currentPlayer = winner;
                RefreshMiddleUI();
                StartCoroutine(NextTurn());
            }
        }
        else
        {
            // If first card of trick, just advance to next player
            RefreshMiddleUI();
            StartCoroutine(NextTurn());
        }*/
    }


    private int GetPlayableCardsForNextPlayer(int playerIndex, bool allowPassIfHasMoves)
    {
        List<GameObject> hand = (playerIndex == 0) ? player1Cards : player2Cards;

        if (playedCards.Count <= trickStartIndex || playedCards[trickStartIndex].card == null)
        {
            Debug.LogWarning("Current trick is empty or improperly set.");
            return 0;
        }

        string leadValue = playedCards[trickStartIndex].card.value;
        int availableCards = 0;

        foreach (GameObject cardObj in hand)
        {
            if (cardObj == null) continue;

            var cardView = cardObj.GetComponent<CardView>();
            if (cardView?.card == null) continue;

            bool legalMove = cardView.card.value == leadValue || cardView.card.value == "7";

            var cg = cardObj.GetComponent<CanvasGroup>() ?? cardObj.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = legalMove;

            var drag = cardObj.GetComponent<CardDrag>();
            if (drag != null) drag.enabled = legalMove;

            if (legalMove) availableCards++;
        }

        if (availableCards > 0)
        {
            // only legal plays enabled
            EnableHandInteraction(hand, true);
            if (allowPassIfHasMoves) ShowPassButton();
        }
        else
        {
            // there is no legal move
            if (!allowPassIfHasMoves)
            {
                // Cover turn -> allow any card as a free discard
                EnableHandInteraction(hand, true);
                cardPlayedInfoText.text = "No matching card. Discard any card; leader takes the trick.";
            }
            else
            {
                // Leader extend turn -> nothing to extend with; caller will resolve
                EnableHandInteraction(hand, false);
            }
        }

        return availableCards;
        // old
        /*foreach (GameObject cardObj in nextPlayerCards)
        {
            if (cardObj == null) continue;

            CardView cardView = cardObj.GetComponent<CardView>();
            if (cardView?.card == null) continue; // safe null check

            Card card = cardView.card;

            // use the card's value
            bool legalMove = card.value == leadValue || card.value == "7";

            // UI/interaction setup
            var cg = cardObj.GetComponent<CanvasGroup>() ?? cardObj.AddComponent<CanvasGroup>();
            var drag = cardObj.GetComponent<CardDrag>();

            if (drag != null) drag.enabled = legalMove;
            cg.blocksRaycasts = legalMove;

            if (legalMove)
            {
                availableCards++;
                // maybe highlight the card visually
            }
        }*/


        // Determine whose turn is next and which hand to check
        // new 08-10, I needed to create a new script becase the Card.cs was not inherit from MonoBehavious, so it can't be attached to a Gameobject
        /*int nextPlayerIndex = (currentPlayer + 1) % 2;
        List<GameObject> nextPlayerCards; 
        
        //List<Card> nextPlayerCards; // CS write this, try something else
        if (nextPlayerIndex == 0) nextPlayerCards = player1Cards;
        else nextPlayerCards = player2Cards; // it was  else(((currentPlayer + 1) % 2) == 1) nextPlayerCards = player2Cards;
       

        var leadValue = playedCards[0].card.value; 
        // for test reason
        if (playedCards.Count > 0 && playedCards[0] != null && playedCards[0].card != null)
        {
            leadValue = playedCards[0].card.value;
        }
        else
        {
            Debug.LogWarning("Played cards list is empty or improperly initialized.");
            return 0;
        }

        int availableCards = 0;
         
        foreach (GameObject cardObj in nextPlayerCards)
        {   // it was card in nextPlayerCards
            // new I put here 08-10
            // for test reason the next line
            if (cardObj == null) continue;
            CardView cardView = cardObj.GetComponent<CardView>();

            if (cardView == null || cardView.card == null) continue; // I put this here 08.10 22:28
            Card card = cardView.card; // new line 08-12
            if (card.value == leadValue || card.IsTrump())
            {
                // make it available to be played
                availableCards++;
            }
            else
            {
                // disable the card, since it is not playable
            }
        }
        return availableCards;*/
    }

    // here


    private void ResolveTrick(int winner)
    {
        // move middle cards to winner's pile
        Transform pile = (winner == 0) ? player1WinPile : player2WinPile;
        foreach (var obj in middleCardObjects)
        {
            if (obj == null) continue;
            LeanTween.move(obj, pile.position, 0.5f).setOnComplete(() => obj.transform.SetParent(pile, false));
        }

        // reset UI and trick state
        middleCardObjects.Clear();
        RefreshMiddleUI();

        // start refill sequence and continue the game
        StartCoroutine(RefillSequence(winner));

    }


    // return winner Player number 
    public int DetermineRoundWinner()
    {
        if (playedCards.Count == 0) throw new System.InvalidOperationException("No plays");

        var leadValue = playedCards[0].card.value;
        for (int i = playedCards.Count - 1; i >= 0; i--)
            if (playedCards[i].card.value == leadValue || playedCards[i].card.IsTrump()) return playedCards[i].player;

        return playedCards[0].player;
        // new
        /*if (playedCards.Count <= trickStartIndex) 
        throw new System.InvalidOperationException("No plays in current trick");

        string leadValue = playedCards[trickStartIndex].card.value;

        for (int i = playedCards.Count - 1; i >= trickStartIndex; i--)
        {
            var pc = playedCards[i];
            if (pc.card.value == leadValue || pc.card.IsTrump())
                return pc.player;
        }

        return playedCards[trickStartIndex].player;*/
    }

    //new 08-17
    private void EnableHandInteraction(List<GameObject> cards, bool enable)
    {
        foreach (var cardObj in cards)
        {
            var cg = cardObj.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.blocksRaycasts = enable;

            var drag = cardObj.GetComponent<CardDrag>();
            if (drag != null)
                drag.enabled = enable;
        }
    }
    

    private void ShowPassButton()
    {
        passButton.gameObject.SetActive(true);
        passButton.onClick.RemoveAllListeners();
        passButton.onClick.AddListener(() => OnPassPressed());

        int leaderIndex = playedCards[trickStartIndex].player;
        string leadValue = playedCards[trickStartIndex].card.value;

        // Disable all cards
        EnableHandInteraction(player1Cards, false);
        EnableHandInteraction(player2Cards, false);

        // enable only valid extension cards for leader
        var leaderHand = (leaderIndex == 0) ? player1Cards : player2Cards;
        foreach (var cardObj in leaderHand)
        {
            if (cardObj == null) continue;
            var view = cardObj.GetComponent<CardView>();
            if (view?.card == null) continue;

            bool legal = view.card.value == leadValue || view.card.value == "7";
            var cg = cardObj.GetComponent<CanvasGroup>() ?? cardObj.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = legal;

            var drag = cardObj.GetComponent<CardDrag>();
            if (drag != null) drag.enabled = legal;

            // visual outline
            var outline = cardObj.GetComponent<Outline>() ?? cardObj.AddComponent<Outline>();
            outline.effectColor = legal ? Color.yellow : Color.clear;
            outline.effectDistance = new Vector2(5f, 5f);
        }

        string leaderName = leaderIndex == 0 ? "Player 1" : "Player 2";
        Color leaderColor = leaderIndex == 0 ? Color.green : Color.red;
        cardPlayedInfoText.text = $"{leaderName}, extend with a 7 or same value, or press Pass.";
        cardPlayedInfoText.color = leaderColor;
        
    }


    // card outline, visual
    private void RemoveAllOutlines()
    {
        foreach (var cardObj in player1Cards.Concat(player2Cards))
        {
            if (cardObj == null) continue;
            var outline = cardObj.GetComponent<Outline>();
            if (outline != null) Destroy(outline);
        }
    }

    private void OnPassPressed()
    {
        passButton.gameObject.SetActive(false);
        // new 08-18
        RemoveAllOutlines();
        int winner = DetermineRoundWinner();
        ResolveTrick(winner);
    }


    public void DrawNextCard()
    {
        
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
        

        if (cardObj == null)
        {
            Debug.LogError("Card object failed to instantiate!");
            return;
        }

        
        // assign player ownership to the card
        CardOwner owner = cardObj.GetComponent<CardOwner>();
        if (owner != null)
        {
            owner.ownerPlayerId = (playerArea == player1Area) ? 0 : 1;
        }

        
        // assign the card to the correct player's list
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

        // bind Card into the prefab's CardView
        // data setup
        CardView cardView = newCard.GetComponent<CardView>();
        if (cardView != null)
            cardView.Bind(card);

        return newCard;
    }

    
    // checks how many cards a player needs to get back to 4 nad draws that number of cards from the deck
    private IEnumerator RefillHand(Transform playerArea, List<GameObject> playerCards)
    {
        while (playerCards.Count < 4 && deck.Count > 0)
        {
            DrawCard(playerArea);
            yield return new WaitForSeconds(0.5f);
        }
    }

    // refill sequence coroutine
    // refill players hands with the missing number of cards AND continue the Game
    private IEnumerator RefillSequence(int winner)
    {
        // Winner draws first
        if (winner == 0)
        {
            yield return StartCoroutine(RefillHand(player1Area, player1Cards));
            yield return StartCoroutine(RefillHand(player2Area, player2Cards));
        }
        else
        {
            yield return StartCoroutine(RefillHand(player2Area, player2Cards));
            yield return StartCoroutine(RefillHand(player1Area, player1Cards));
        }

        // Continue the game
        currentPlayer = winner;
        trickStartIndex = playedCards.Count;
        SetupTurn();
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
