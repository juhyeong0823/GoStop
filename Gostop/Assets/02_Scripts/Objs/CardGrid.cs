using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardGrid : MonoBehaviour
{
    public List<Card> placedCards = new List<Card>();

    public int curPlacedCardMonth = -1; // 1~12 �ƴ� ���� ��

    public void Reset()
    {
        curPlacedCardMonth = -1;
    }

    public void Set(Card placeCard)
    {
        curPlacedCardMonth = placeCard.cardData.cardMonth; // ���� �� �׸���� �� ī���� ���� �ش��ϴ� �͸� �޾ƿ� �� ����!
        placedCards.Add(placeCard);
        placeCard.transform.parent = transform;
    }


}
