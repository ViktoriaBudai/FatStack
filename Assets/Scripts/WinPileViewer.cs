using UnityEngine;
using UnityEngine.UI;

public class WinPileViewer : MonoBehaviour
{
    public Transform player1WinPile;
    public GameObject cardUIPrefab;
    public Transform expandedViewParent; // grid/ horizontal layout for UI cards

    public void ShowPile()
    {
        foreach (Transform child in expandedViewParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform card in player1WinPile)
        {
            var cardUI = Instantiate(cardUIPrefab, expandedViewParent);
            cardUI.GetComponent<Image>().sprite = card.GetComponent<SpriteRenderer>().sprite;
        }

        expandedViewParent.parent.gameObject.SetActive(true);
    }

    public void HidePile()
    {
        expandedViewParent.parent.gameObject.SetActive(false);
    }
    
}
