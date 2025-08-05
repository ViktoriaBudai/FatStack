using UnityEngine;
using TMPro;

public class CardOwner : MonoBehaviour
{
    public int ownerPlayerId; // player1 = 0; player2 = 1
    public TextMeshProUGUI ownerLabel;

    public void SetOwnerLabel()
    {
        if (ownerLabel != null )
        {
            ownerLabel.text = ownerPlayerId == 0 ? "Player 1" : "Player 2";
            ownerLabel.color = ownerPlayerId == 0 ? Color.blue : Color.red;
        }
    }
}
