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

        nullGrid.curPlacedCardMonth = this.cardData.cardMonth; // 이제 이 그리드는 이 카드의 월에 해당하는 것만 받아올 수 있음!
        nullGrid.placedCards.Add(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 같은 월이 깔려있는 그리드를 찾습니다.
        CardGrid targetGrid = CardFinder.GetSameMonthCardsGrid(GameManager.Instance.cardGrids, this);

        // 내가 낸 패에 대한 처리.
        if(targetGrid == null) // 같은 월이 깔린게 없으면 그냥 비어있는데에 내려놓습니다.
        {
            CardGrid nullGrid = GameManager.Instance.GetNullGridPos();
            nullGrid.curPlacedCardMonth = this.cardData.cardMonth; // 이제 이 그리드는 이 카드의 월에 해당하는 것만 받아올 수 있음!
            nullGrid.placedCards.Add(this);                                         
        }
        else
        {
            int count = targetGrid.placedCards.Count;
            switch (count)
            {
                case 1: // 하나 있으면 그 친구한테 우선 가기. -> 뻑 가능성
                    this.transform.position = targetGrid.transform.position;
                    targetGrid.placedCards.Add(this);
                    break;

                case 2: // 두개 있으면 거기로 우선 가기 -> 따닥 가능성
                    this.transform.position = targetGrid.transform.position;
                    targetGrid.placedCards.Add(this);
                    break;

                case 3: // 3개 있으면 다 가져오고 
                    foreach (Card card in targetGrid.placedCards)
                    {
                        GameManager.Instance.OnScored(card);
                    }

                    // 내가 낸 것도 가져오기.
                    GameManager.Instance.OnScored(this);
                    break;
            }
        }



        // 뒷 패에 대한 처리 시작

        Card followCard = GameManager.Instance.followUpCardQueue.Dequeue();

        targetGrid = CardFinder.GetSameMonthCardsGrid(GameManager.Instance.cardGrids, followCard);

        if (targetGrid == null) // 같은 월이 깔린게 없으면 그냥 비어있는데에 내려놓습니다.
        {
            CardGrid nullGrid = GameManager.Instance.GetNullGridPos();
            nullGrid.curPlacedCardMonth = followCard.cardData.cardMonth; // 이제 이 그리드는 이 카드의 월에 해당하는 것만 받아올 수 있음!
            nullGrid.placedCards.Add(followCard);
        }
        else
        {
            int count = targetGrid.placedCards.Count;
            switch (count)
            {
                case 1: // 뒷패를 깠는데 하나만 있다는 거면, 무조건 가져오면 됨.
                    GameManager.Instance.OnScored(followCard);
                    GameManager.Instance.OnScored(targetGrid.placedCards[0]);

                    targetGrid.bPlacedCardIsExist = false;
                    break;

                case 2: // '' 두 개가 있다는 거면 둘 중 하나를 선택하게 하면 됨.
                    


                    break;

                case 3: // 3개 있으면 다 가져오고 
                    foreach (Card card in targetGrid.placedCards)
                    {
                        GameManager.Instance.OnScored(card);
                    }

                    // 내가 낸 것도 가져오기.
                    GameManager.Instance.OnScored(followCard);
                    break;
            }
        }
    }

}
