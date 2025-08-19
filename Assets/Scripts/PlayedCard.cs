using System.Collections.Generic;
using UnityEngine;

public class PlayedCard
{
    public Card card;
    public int player;

    public PlayedCard(Card card, int player)
    {
        this.card = card;
        this.player = player;
    }
}