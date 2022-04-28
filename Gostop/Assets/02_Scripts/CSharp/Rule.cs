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

    public void CheckPresident() // ���� ���� �� ��, ���� �� ī�尡 ����� -> ��ź ���� üũ����
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

    void CheckMyCards(UserData userData) // ���� ���� �� ��, ���� �� ī�尡 ����� -> ��ź ���� üũ����
    {
        foreach (var card in userData.ownCards)
        {
            int sameWithItemMonth = CardManager.Instance.GetSameMonthCards(card).Count;
            if (sameWithItemMonth > 3)
            {
                //ù ������ �ƴ��� üũ�ؼ� ó���̸� ����
                if (GameManager.Instance.isFirstChecking)
                {
                    // ����, �¸� ó�� �� ���� ������ � static���� �ھƳ��� Double! �ھƵα�
                    GameManager.Instance.isFirstChecking = false;
                }
                else
                {
                    // ����� ���� 100%
                }
            }
            else if (sameWithItemMonth > 2)
            {
                // ��ź or �����ε� ���� 
                if (CardFinder.GetSameMonthCardsGrid(GameManager.Instance.cardGrids, card) != null)
                {
                    // ����� ��ź
                }
                else
                {
                    // ����� ����
                }
            }
            else if (IsTakenMonthCard(card))
            {
                // ī�� �� Ŀ��, ��� �ִ� �� Ŀ����
            }
            else if (CardFinder.GetSameMonthCardsGrid(GameManager.Instance.cardGrids, card) != null)
            {
                // ���� ��
            }
            else
            {
                // ������ �� ���� ��
            }
        }
    }

    public bool IsTakenMonthCard(CardBase card) // ���� ������
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
            Debug.Log("��!");
            GameManager.Instance.TakeOtherPlayerCard();
        }
    }

    public void Kiss()
    {
        Debug.Log("��");
        GameManager.Instance.TakeOtherPlayerCard();
    }
}
