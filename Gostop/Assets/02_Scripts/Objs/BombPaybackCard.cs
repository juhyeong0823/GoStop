using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BombPaybackCard : CardBase
{
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (MatgoManager.Instance.isGameProcessing) return;
        Putcard();
        MatgoManager.Instance.targetUser.utilizeCards.Remove(this);
        Destroy(this.gameObject);

    }

    public void Putcard()
    {
        img.raycastTarget = false;
        MatgoManager.Instance.SetTargetUserdata();
        CardBase followCard = MatgoManager.Instance.GetRandomCard(MatgoManager.Instance.followUpCards);

        followCard.gameObject.SetActive(true);

        CardGrid followCardTargetGrid = CardFinder.GetSameMonthCardsGrid(MatgoManager.Instance.cardGrids, followCard);

        if (followCardTargetGrid == null) // ���� ���� �򸰰� ������ �׳� ����ִµ��� ���������ϴ�.
        {
            followCardTargetGrid = MatgoManager.Instance.GetNullGrid();
            followCardTargetGrid.Set(followCard);
        }
        else
        {
            int count = followCardTargetGrid.placedCards.Count;
            switch (count)
            {
                case 1: // ���и� ���µ� �ϳ��� �ִٴ� �Ÿ�, ������ �������� ��.
                    MatgoManager.Instance.OnScored(followCard);
                    MatgoManager.Instance.OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                    followCardTargetGrid.Reset();
                    break;

                case 2: // �� ���� �ִٴ� �Ÿ� �� �� �ϳ��� �����ϰ� �ϸ� ��.
                    followCardTargetGrid.placedCards.Add(followCard);
                    MatgoManager.Instance.choiceCallBackQueue.Enqueue(() =>
                        MatgoManager.Instance.ChoiceCard(followCardTargetGrid.placedCards[0], followCardTargetGrid.placedCards[1], followCardTargetGrid));
                    MatgoManager.Instance.putCardQueue.Enqueue(followCard);
                    break;

                case 3: // 3�� ������ �� �������� 
                    while (followCardTargetGrid.placedCards.Count > 0)
                    {
                        MatgoManager.Instance.OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                    }

                    MatgoManager.Instance.OnScored(followCard); // ���� �� �͵� ��������.
                    MatgoManager.Instance.choiceCallBackQueue.Clear();
                    followCardTargetGrid.Reset();
                    break;
            }
        }

        MatgoManager.Instance.TryExecuteChoiceCallback();
    }
}
