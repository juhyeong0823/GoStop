using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rule
{
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
        GameManager.Instance.SetTargetUserData();
        GameManager.Instance.isUserTurn = !GameManager.Instance.isUserTurn;

        foreach (var card in GameManager.Instance.targetUserData.utilizeCards)
        {
            if (CardManager.Instance.GetSameMonthCards(card).Count == 4)
            {
                UIManager.Instance.gostopUI.stopUI.OnStop(true, false);
            }
        }

        GameManager.Instance.SetTargetUserData();
        GameManager.Instance.isUserTurn = !GameManager.Instance.isUserTurn;

        foreach (var card in GameManager.Instance.targetUserData.utilizeCards)
        {
            if (CardManager.Instance.GetSameMonthCards(card).Count == 4)
            {
                UIManager.Instance.gostopUI.stopUI.OnStop(true, false);
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
                if (GameManager.Instance.isFirstChecking)
                {
                    // 총통, 승리 처리 후 게임 데이터 등에 static으로 박아놔서 Double! 박아두기
                    GameManager.Instance.isFirstChecking = false;
                }
                else
                {
                    // 여기는 흔들기 100%
                }
            }
            else if (sameWithItemMonth > 2)
            {
                // 폭탄 or 흔들기인데 이제 
                if (CardFinder.GetSameMonthCardsGrid(GameManager.Instance.cardGrids, card) != null)
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
            else if (CardFinder.GetSameMonthCardsGrid(GameManager.Instance.cardGrids, card) != null)
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
        foreach (int month in GameManager.Instance.takenMonthList)
        {
            if (month == card.cardData.cardMonth) return true;
        }
        return false;
    }

    public void Shake()
    {
        GameManager.Instance.targetUserData.scoreData.shakedCount++;
    }

    public void Bomb(int paybackCardCount)
    {
        GameManager.Instance.targetUserData.scoreData.shakedCount++;
        CardManager.Instance.OnDroppedBomb(paybackCardCount, GameManager.Instance.targetUserData.utilizeCardsTrm);
    }

    public void Paulk()
    {
        GameManager.Instance.targetUserData.scoreData.paulkCount++;
        if (GameManager.Instance.sc.CheckPaulkCount(GameManager.Instance.targetUserData.scoreData))
        {
            UIManager.Instance.gostopUI.stopUI.PaulkWin();
        }
    }

    public void Sweep()
    {
        bool canSweep = true;

        foreach(var item in GameManager.Instance.cardGrids)
        {
            if(item.placedCards.Count != 0)
            {
                canSweep = false;
            }
        }

        if(canSweep)
        {
            Debug.Log("쓸!");
            GameManager.Instance.TakeOtherPlayerCard();
        }
    }

    public void Kiss()
    {
        Debug.Log("쪽");
        GameManager.Instance.TakeOtherPlayerCard();
    }
}
