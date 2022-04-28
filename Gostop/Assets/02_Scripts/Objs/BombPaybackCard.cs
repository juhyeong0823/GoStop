using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BombPaybackCard : CardBase
{
    public override void OnPointerDown(PointerEventData eventData)
    {
        img.raycastTarget = false; ;
        Putcard();
        GameManager.Instance.targetUserData.utilizeCards.Remove(this);
        Destroy(this.gameObject);
    }

    public void Putcard()
    {
        CardBase followCard = GameManager.Instance.followUpCardQueue.Dequeue();

        followCard.gameObject.SetActive(true);

        CardGrid followCardTargetGrid = CardFinder.GetSameMonthCardsGrid(GameManager.Instance.cardGrids, followCard);

        if (followCardTargetGrid == null) // ���� ���� �򸰰� ������ �׳� ����ִµ��� ���������ϴ�.
        {
            followCardTargetGrid = GameManager.Instance.GetNullGrid();
            followCardTargetGrid.Set(followCard);
        }
        else
        {
            int count = followCardTargetGrid.placedCards.Count;
            switch (count)
            {
                case 1: // ���и� ���µ� �ϳ��� �ִٴ� �Ÿ�, ������ �������� ��.
                    GameManager.Instance.OnScored(followCard);
                    GameManager.Instance.OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                    followCardTargetGrid.Reset();
                    break;

                case 2: // �� ���� �ִٴ� �Ÿ� �� �� �ϳ��� �����ϰ� �ϸ� ��.
                    followCard.MoveCardToGrid(followCardTargetGrid);

                    GameManager.Instance.choiceCallBackQueue.Enqueue(() =>
                        GameManager.Instance.ChoiceCard(followCardTargetGrid.placedCards[0], followCardTargetGrid.placedCards[1], followCardTargetGrid));
                    GameManager.Instance.putCardQueue.Enqueue(followCard);
                    break;

                case 3: // 3�� ������ �� �������� 
                    while (followCardTargetGrid.placedCards.Count > 0)
                    {
                        GameManager.Instance.OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                    }

                    GameManager.Instance.OnScored(followCard); // ���� �� �͵� ��������.
                    GameManager.Instance.choiceCallBackQueue.Clear();
                    followCardTargetGrid.Reset();
                    break;
            }
        }

        GameManager.Instance.TryExecuteChoiceCallback();
    }
}
