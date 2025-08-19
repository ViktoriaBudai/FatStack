using System.Collections.Generic;
using UnityEngine;
public class CardView : MonoBehaviour
{
    public Card card;

    public string SuitOrNull => card?.suit;
    public string ValueOrNull => card?.value;
    public bool? IsTrumpOrNull => card != null ? card.IsTrump() : (bool?)null;
    public int? PointsOrNull => card?.points;

    public void Bind(Card data) => card = data;
}


