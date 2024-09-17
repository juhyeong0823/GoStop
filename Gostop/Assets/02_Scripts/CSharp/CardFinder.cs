using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardFinder 
{
    public static CardGrid GetSameMonthCardsGrid(List<CardGrid> grids, CardBase putCard)
    {
        return grids.Find((x) => x.curPlacedCardMonth == putCard.cardData.cardMonth);
    }

    public static CardBase FindJunkCard(List<CardBase> cards)
    {
        CardBase retCard = null;

        retCard = cards.Find((x) => (x.cardData.cardProperty & eProperty.Junk) != 0);
        if (retCard == null)
        {
            retCard = cards.Find((x) => (x.cardData.cardProperty & eProperty.Double) != 0 || (x.cardData.cardProperty & eProperty.DoubleNine) != 0);
        }

        return retCard;
    }
}
