using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardGrid : MonoBehaviour
{
    public List<Card> placedCards = new List<Card>();

    public int curPlacedCardMonth = -1; // 1~12 아닌 수면 돼

    public void Reset()
    {
        curPlacedCardMonth = -1;
    }

    public void Set(Card placeCard)
    {
        curPlacedCardMonth = placeCard.cardData.cardMonth; // 이제 이 그리드는 이 카드의 월에 해당하는 것만 받아올 수 있음!
        placedCards.Add(placeCard);
        placeCard.transform.parent = transform;
    }


}
