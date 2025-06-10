using System.Collections.Generic;
using UnityEngine;

public class Card
{
    public string suit; // e.g. Heart, Leaf, ...
    public string value; // Ten, Nine, Eight, Seven, Upper, Lower ...
    public bool isQueen; // in case of this game the trump(adu) is the Queen, because of the twist, act like a Seven in the normal game
    public int points; // 10 ponts for Ace and Ten
    
    
    public Card(string suit, string value, bool isQueen)
    {
        this.suit = suit;
        this.value = value;
        this.isQueen = isQueen;
    }
}
