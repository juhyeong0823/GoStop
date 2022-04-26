using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardFinder 
{
    public static CardGrid GetSameMonthCardsGrid(List<CardGrid> grids, Card putCard)
    {
        return grids.Find((x) => x.curPlacedCardMonth == putCard.cardData.cardMonth);
    }

}
