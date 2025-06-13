using UnityEngine;
using UnityEngine.UI;

public class ClickableDeck : MonoBehaviour
{
    public Button deckButton;

    void Start()
    {
        deckButton.onClick.AddListener(OnDeckClicked);
    }

    void OnDeckClicked()
    {
        Debug.Log("Deck button clicked!");
        GameManager.Instance.DrawNextCard();
    }
}
