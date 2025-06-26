using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaveManager : MonoBehaviour
{
    public TextMeshProUGUI counterText; // or TextMeshProUGUI if using TMP
    private int leaveCount = 0;

    public void OnLeaveButtonPressed()
    {
        leaveCount++;
        counterText.text = "Leave button pressed: " + leaveCount + " times";
        Debug.Log("Player has left the round."); // You can replace this with game logic later
    }
}

