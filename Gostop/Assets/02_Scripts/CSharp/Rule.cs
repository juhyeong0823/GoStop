using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rule
{
    MatgoManager matgoManager;

    public bool CanGo(int score, ScoreData scoreData)
    {
        if (score >= 7 && scoreData.bFirstGo)
        {
            scoreData.saidGoScore = score;
            scoreData.bFirstGo = false;
            return true;
        }
        else if (score > scoreData.saidGoScore)
        {
            scoreData.saidGoScore = score;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void CheckPresident() // 내가 가진 패 중, 같은 월 카드가 몇개인지 -> 폭탄 흔들기 체크용임
    {
        matgoManager = MatgoManager.Instance;

        for (int i = 0; i< 2; i++)
        {
            MatgoManager.Instance.SetTargetUserdata();
            foreach (var card in MatgoManager.Instance.targetUser.utilizeCards)
            {
                if (CardManager.Instance.GetSameMonthCards(card).Count == 4)
                {
                    UIManager.Instance.gostopUI.stopUI.OnStop(true, false);
                }
            }
            MatgoManager.Instance.isUserTurn = !MatgoManager.Instance.isUserTurn;
        }

        foreach (var grid in MatgoManager.Instance.cardGrids)
        {
            foreach(var card in grid.placedCards)
            {
                if (CardManager.Instance.GetSameMonthCards(card).Count == 4)
                {
                    UIManager.Instance.gostopUI.stopUI.OnStop(true, true);
                }
            }
        }
    }

    void CheckMyCards(UserData userData) // 내가 가진 패 중, 같은 월 카드가 몇개인지 -> 폭탄 흔들기 체크용임
    {
        foreach (var card in userData.ownCards)
        {
            int sameWithItemMonth = CardManager.Instance.GetSameMonthCards(card).Count;
            if (sameWithItemMonth > 3)
            {
                //첫 턴인지 아닌지 체크해서 처음이면 총통
                if (MatgoManager.Instance.isFirstChecking)
                {
                    // 총통, 승리 처리 후 게임 데이터 등에 static으로 박아놔서 Double! 박아두기
                    MatgoManager.Instance.isFirstChecking = false;
                }
                else
                {
                    // 여기는 흔들기 100%
                }
            }
            else if (sameWithItemMonth > 2)
            {
                // 폭탄 or 흔들기인데 이제 
                if (CardFinder.GetSameMonthCardsGrid(MatgoManager.Instance.cardGrids, card) != null)
                {
                    // 여기는 폭탄
                }
                else
                {
                    // 여기는 흔들기
                }
            }
            else if (IsTakenMonthCard(card))
            {
                // 카드 위 커서, 살아 있는 패 커서로
            }
            else if (CardFinder.GetSameMonthCardsGrid(MatgoManager.Instance.cardGrids, card) != null)
            {
                // 죽은 패
            }
            else
            {
                // 당장은 못 쓰는 패
            }
        }
    }

    public bool IsTakenMonthCard(CardBase card) // 죽은 패인지
    {
        foreach (int month in MatgoManager.Instance.takenMonthList)
        {
            if (month == card.cardData.cardMonth) return true;
        }
        return false;
    }

    public void Shake()
    {
        MatgoManager.Instance.targetUser.scoreData.shakedCount++;
    }

    public void Bomb(int paybackCardCount)
    {
        MatgoManager.Instance.targetUser.scoreData.shakedCount++;
        CardManager.Instance.OnDroppedBomb(paybackCardCount, MatgoManager.Instance.targetUser.utilizeCardsTrm);
    }

    public void Paulk()
    {
        MatgoManager.Instance.targetUser.scoreData.paulkCount++;
        if (MatgoManager.Instance.sc.CheckPaulkCount(MatgoManager.Instance.targetUser.scoreData))
        {
            UIManager.Instance.gostopUI.stopUI.PaulkWin();
        }
    }

    public void Sweep()
    {
        bool canSweep = true;

        foreach(var item in MatgoManager.Instance.cardGrids)
        {
            if(item.placedCards.Count != 0)
            {
                canSweep = false;
            }
        }

        if(canSweep)
        {
            Debug.Log("쓸!");

            MatgoManager.Instance.TakeOtherPlayerCard();
        }
    }

    public void Kiss()
    {
        Debug.Log("쪽");
        MatgoManager.Instance.TakeOtherPlayerCard();
    }
}
