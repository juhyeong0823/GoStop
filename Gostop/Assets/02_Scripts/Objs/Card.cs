using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerDownHandler
{
    public CardDataSO cardData;

    public void InitNullGrid()
    {
        CardGrid nullGrid = GameManager.Instance.GetNullGridPos();

        nullGrid.bPlacedCardIsExist = true;

        nullGrid.curPlacedCardMonth = this.cardData.cardMonth; // ���� �� �׸���� �� ī���� ���� �ش��ϴ� �͸� �޾ƿ� �� ����!
        nullGrid.placedCards.Add(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // ���� ���� ����ִ� �׸��带 ã���ϴ�.
        CardGrid targetGrid = CardFinder.GetSameMonthCardsGrid(GameManager.Instance.cardGrids, this);

        // ���� �� �п� ���� ó��.
        if(targetGrid == null) // ���� ���� �򸰰� ������ �׳� ����ִµ��� ���������ϴ�.
        {
            CardGrid nullGrid = GameManager.Instance.GetNullGridPos();
            nullGrid.curPlacedCardMonth = this.cardData.cardMonth; // ���� �� �׸���� �� ī���� ���� �ش��ϴ� �͸� �޾ƿ� �� ����!
            nullGrid.placedCards.Add(this);                                         
        }
        else
        {
            int count = targetGrid.placedCards.Count;
            switch (count)
            {
                case 1: // �ϳ� ������ �� ģ������ �켱 ����. -> �� ���ɼ�
                    this.transform.position = targetGrid.transform.position;
                    targetGrid.placedCards.Add(this);
                    break;

                case 2: // �ΰ� ������ �ű�� �켱 ���� -> ���� ���ɼ�
                    this.transform.position = targetGrid.transform.position;
                    targetGrid.placedCards.Add(this);
                    break;

                case 3: // 3�� ������ �� �������� 
                    foreach (Card card in targetGrid.placedCards)
                    {
                        GameManager.Instance.OnScored(card);
                    }

                    // ���� �� �͵� ��������.
                    GameManager.Instance.OnScored(this);
                    break;
            }
        }



        // �� �п� ���� ó�� ����

        Card followCard = GameManager.Instance.followUpCardQueue.Dequeue();

        targetGrid = CardFinder.GetSameMonthCardsGrid(GameManager.Instance.cardGrids, followCard);

        if (targetGrid == null) // ���� ���� �򸰰� ������ �׳� ����ִµ��� ���������ϴ�.
        {
            CardGrid nullGrid = GameManager.Instance.GetNullGridPos();
            nullGrid.curPlacedCardMonth = followCard.cardData.cardMonth; // ���� �� �׸���� �� ī���� ���� �ش��ϴ� �͸� �޾ƿ� �� ����!
            nullGrid.placedCards.Add(followCard);
        }
        else
        {
            int count = targetGrid.placedCards.Count;
            switch (count)
            {
                case 1: // ���и� ���µ� �ϳ��� �ִٴ� �Ÿ�, ������ �������� ��.
                    GameManager.Instance.OnScored(followCard);
                    GameManager.Instance.OnScored(targetGrid.placedCards[0]);

                    targetGrid.bPlacedCardIsExist = false;
                    break;

                case 2: // '' �� ���� �ִٴ� �Ÿ� �� �� �ϳ��� �����ϰ� �ϸ� ��.
                    


                    break;

                case 3: // 3�� ������ �� �������� 
                    foreach (Card card in targetGrid.placedCards)
                    {
                        GameManager.Instance.OnScored(card);
                    }

                    // ���� �� �͵� ��������.
                    GameManager.Instance.OnScored(followCard);
                    break;
            }
        }
    }

}
