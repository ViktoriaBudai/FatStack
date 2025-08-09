using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrickRecord
{
    public int winnerPlayerIndex;    // 0 or 1
    public List<Card> cards;         // the Card data models in play order
    

    public TrickRecord(int winner, List<Card> played)
    {
        winnerPlayerIndex = winner;
        cards = new List<Card>(played);
        
    }
}

