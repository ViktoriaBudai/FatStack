using TMPro;
using UnityEngine;

public class LeaveManager : MonoBehaviour
{
    // old code, modified 08.06.
    /*public TextMeshProUGUI counterText; // or TextMeshProUGUI if using TMP
    private int leaveCount = 0;

    public GameManager gameManager;
    public void OnLeaveButtonPressed()
    {
        // new 08.06.
        // Update internal counter and UI
        leaveCount++;
        if (counterText != null)
        {
            counterText.text = $"Leave button pressed: {leaveCount} times";
        }
        else
        {
            Debug.LogWarning("LeaveManager: counterText is not assigned!");
        }

        Debug.Log("Player has left the round.");

        // Invoke GameManager logic
        if (gameManager != null)
        {
            int idx = gameManager.CurrentPlayerIndex;
            gameManager.OnPlayerLeaveRound(idx);
        }
       
    }*/
    // new 08.06
    public GameManager gameManager;
    public int playerIndex = 0;

    // Called by your Leave button’s OnClick()
    public void OnLeaveButtonPressed()
    {
        if (gameManager == null)
        {
            Debug.LogError("LeaveManager: GameManager not assigned!");
            return;
        }

        // Tell GM that this player has left the trick;
        // GM will disable their cards and advance the turn.
        //gameManager.OnPlayerLeaveRound(playerIndex); // commented out 08-07 11:35

        Debug.Log($"Player {playerIndex + 1} left this trick — cards disabled.");
    }
}

