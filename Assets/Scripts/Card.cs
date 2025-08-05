using System.Collections.Generic;
using UnityEngine;

public class Card 
{
    public string suit; // e.g. Heart, Leaf, ...
    public string value; // Ten, Nine, Eight, Seven, Upper, Lower ...
    public bool isQueen; // in case of this game the trump(adu) is the Queen, because of the twist, act like a Seven in the normal game
    public int points; // 10 ponts for Ace and Ten
    public int strength;
    
    
    public Card(string suit, string value, bool isQueen)
    {
        this.suit = suit;
        this.value = value;
        this.isQueen = isQueen;
        this.points = GetCardPoints();
    }

    private int GetCardPoints() 
    {
        if (value == "ace" || value == "10") return 10;
        return 0;
    }

    public int GetStrength() 
    {
        switch (value.ToLower())
        {
            case "queen": return isQueen ? 100 : 15; // Highest priority if trump
            case "ace": return 11;
            case "10": return 10;
            case "over": return 4;
            case "under": return 2;
            case "9": return 0;
            case "8": return 0;
            case "7": return 1; // low but not nothing
            default: return 0;
        }
    }
}
