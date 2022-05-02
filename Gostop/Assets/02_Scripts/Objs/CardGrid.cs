using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardGrid : MonoBehaviour
{
    public List<CardBase> placedCards = new List<CardBase>();

    public int curPlacedCardMonth = -1; // 1~12 �ƴ� ���� ��

    public void Reset()
    {
        curPlacedCardMonth = -1;
    }

    public void Set(CardBase placeCard)
    {
        curPlacedCardMonth = placeCard.cardData.cardMonth; // ���� �� �׸���� �� ī���� ���� �ش��ϴ� �͸� �޾ƿ� �� ����!
        placeCard.MoveCardToGrid(this);
    }
}
