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

        if (followCardTargetGrid == null) // 같은 월이 깔린게 없으면 그냥 비어있는데에 내려놓습니다.
        {
            followCardTargetGrid = MatgoManager.Instance.GetNullGrid();
            followCardTargetGrid.Set(followCard);
        }
        else
        {
            int count = followCardTargetGrid.placedCards.Count;
            switch (count)
            {
                case 1: // 뒷패를 깠는데 하나만 있다는 거면, 무조건 가져오면 됨.
                    MatgoManager.Instance.OnScored(followCard);
                    MatgoManager.Instance.OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                    followCardTargetGrid.Reset();
                    break;

                case 2: // 두 개가 있다는 거면 둘 중 하나를 선택하게 하면 됨.
                    followCardTargetGrid.placedCards.Add(followCard);
                    MatgoManager.Instance.choiceCallBackQueue.Enqueue(() =>
                        MatgoManager.Instance.ChoiceCard(followCardTargetGrid.placedCards[0], followCardTargetGrid.placedCards[1], followCardTargetGrid));
                    MatgoManager.Instance.putCardQueue.Enqueue(followCard);
                    break;

                case 3: // 3개 있으면 다 가져오고 
                    while (followCardTargetGrid.placedCards.Count > 0)
                    {
                        MatgoManager.Instance.OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                    }

                    MatgoManager.Instance.OnScored(followCard); // 내가 낸 것도 가져오기.
                    MatgoManager.Instance.choiceCallBackQueue.Clear();
                    followCardTargetGrid.Reset();
                    break;
            }
        }

        MatgoManager.Instance.TryExecuteChoiceCallback();
    }
}
