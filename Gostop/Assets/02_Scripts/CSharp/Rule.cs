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

    public void CheckPresident() // ���� ���� �� ��, ���� �� ī�尡 ����� -> ��ź ���� üũ����
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

    void CheckMyCards(UserData userData) // ���� ���� �� ��, ���� �� ī�尡 ����� -> ��ź ���� üũ����
    {
        foreach (var card in userData.ownCards)
        {
            int sameWithItemMonth = CardManager.Instance.GetSameMonthCards(card).Count;
            if (sameWithItemMonth > 3)
            {
                //ù ������ �ƴ��� üũ�ؼ� ó���̸� ����
                if (MatgoManager.Instance.isFirstChecking)
                {
                    // ����, �¸� ó�� �� ���� ������ � static���� �ھƳ��� Double! �ھƵα�
                    MatgoManager.Instance.isFirstChecking = false;
                }
                else
                {
                    // ����� ���� 100%
                }
            }
            else if (sameWithItemMonth > 2)
            {
                // ��ź or �����ε� ���� 
                if (CardFinder.GetSameMonthCardsGrid(MatgoManager.Instance.cardGrids, card) != null)
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
            else if (CardFinder.GetSameMonthCardsGrid(MatgoManager.Instance.cardGrids, card) != null)
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
            Debug.Log("��!");

            MatgoManager.Instance.TakeOtherPlayerCard();
        }
    }

    public void Kiss()
    {
        Debug.Log("��");
        MatgoManager.Instance.TakeOtherPlayerCard();
    }
}
