using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [HideInInspector] public List<Card> useCardList = new List<Card>(); // ���� ����� ��
    [HideInInspector] public Queue<Card> followUpCardQueue = new Queue<Card>(); // ���б� �� �� �뵵

    public List<Card> myCards = new List<Card>();            // ���� ���� �е�
    public List<Card> otherCards = new List<Card>();         // ��밡 ���� �е�

    public List<CardGrid> cardGrids = new List<CardGrid>(); // ī�带 �� ��ġ, ������ �ٸ��� �׳� �� ���� ã�Ƽ� ã�ư���
    List<int> takenMonthList = new List<int>();  // ���� ��� �� ����

    public Queue<Action> choiceCallBackQueue = new Queue<Action>(); // �ΰ��� �и� ġ��, ���߿� ������
    public Queue<Card> putCardQueue = new Queue<Card>(); // ����� �� �ֵ� �Ծ������ ��

    public bool isChoicing = false;
    public bool isUserTurn = true;
    bool isFirstChecking = true;

    public ScoreCounter sc = new ScoreCounter();

    public ScoreData user = new ScoreData();
    public ScoreData other = new ScoreData();

    private void Start()
    {
        GameStart();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }

    void GameStart()
    {
        CardManager.Instance.SetUseCardList();
        CardManager.Instance.SetPlayersUtilizeCard();
        CardManager.Instance.SetUtilizeCards();
        SetFollowCardQueue();
    }

    public void TryExecuteChoiceCallback()
    {
        if (choiceCallBackQueue.Count > 0)
        {
            choiceCallBackQueue.Dequeue()?.Invoke();
        }
    }

    public Card GetRandomCard()
    {
        int rand = UnityEngine.Random.Range(0, useCardList.Count);
        Card retCard = useCardList[rand];
        useCardList.RemoveAt(rand);
        return retCard;
    }

    public void ChoiceCard(Card card1, Card card2, CardGrid grid)
    {
        if (grid.placedCards.Count == 0)  // 2�� �� �������� �˾Ҵµ� �������� ��������. 
        {
            putCardQueue.Dequeue(); // �ѹ� �����ϸ� �ϳ� ����� ��
            TryExecuteChoiceCallback(); // ������ �ֳ� Ȯ�����ְ�.
        }
        else
        {
            if(isUserTurn)
            {
                isChoicing = true; // ���� �ൿ ��� �ʿ��� ��. �������� ���� �ƹ��͵� ���ϰ� -> �гη� ���Ƶα� ������ �� ���� ��� �ֳ�?
                UIManager.Instance.choiceUI.SetData(card1, card2, grid);
            }
            else
            {
                OnChooseCard(card1, grid);
            }
        }
    }

    public void OnChooseCard(Card chosenCard, CardGrid cardGrid) // ī�尡 �ִ� ���� ����Ʈ ������� ��.
    {
        Card card = putCardQueue.Dequeue();
        OnScored(card, cardGrid);
        OnScored(chosenCard, cardGrid);

        TryExecuteChoiceCallback();
    }

    public CardGrid GetNullGrid() => cardGrids.Find((x) => x.placedCards.Count == 0);

    public void OnScored(Card card, CardGrid cardGrid = null) // ī�带 ���� ��
    {
        CardManager.Instance.TransportCard(card);
        if (cardGrid != null) cardGrid.placedCards.Remove(card);

        if(isUserTurn)
        {
            myCards.Add(card);
        }
        else
        {
            otherCards.Add(card);
        }

    }

    void SetFollowCardQueue()
    {
        foreach (var item in useCardList) followUpCardQueue.Enqueue(item);
        useCardList.Clear(); // �ִ� ���� �� �� ����.
    }

    void CheckMyCards() // ���� ���� �� ��, ���� �� ī�尡 ����� -> ��ź ���� üũ����
    {
        foreach(var card in myCards)
        {
            int sameWithItemMonth = CardManager.Instance.GetSameMonthCards(card).Count;
            if (sameWithItemMonth > 3)
            {
                //ù ������ �ƴ��� üũ�ؼ� ó���̸� ����
                if(isFirstChecking)
                {
                    // ����, �¸� ó�� �� ���� ������ � static���� �ھƳ��� Double! �ھƵα�
                    isFirstChecking = false;
                }
                else
                {
                    // ����� ���� 100%
                }
            }
            else if(sameWithItemMonth > 2)
            {
                // ��ź or �����ε� ���� 
                if(CardFinder.GetSameMonthCardsGrid(cardGrids,card) != null)
                {
                    // ����� ��ź
                }
                else
                {
                    // ����� ����
                }
            }
            else if(IsTakenMonthCard(card))
            {
                // ī�� �� Ŀ��, ��� �ִ� �� Ŀ����
            }
            else if(CardFinder.GetSameMonthCardsGrid(cardGrids, card) != null)
            {
                // ���� ��
            }
            else
            {
                // ������ �� ���� ��
            }
        }
    }

    void MoveCardToGrid(Card card, CardGrid grid)
    {
        card.transform.parent = grid.transform;
        grid.placedCards.Add(card);
    }

    public bool IsTakenMonthCard(Card card) // ���� ������
    {
        foreach (int month in takenMonthList)
        {
            if (month == card.cardData.cardMonth) return true;
        }
        return false;
    }

    public void OnShakeOrBomb()
    {
        if(isUserTurn)
        {
            user.shakedCount++;
        }
        else
        {
            other.shakedCount++;
        }
    }    

    public void PutCard(Card card) // ���� �� �׸��带 ������ ������ ������?
    {
        CardGrid targetGrid = CardFinder.GetSameMonthCardsGrid(cardGrids, card);

        if (targetGrid == null) // ���� ���� �򸰰� ������ �׳� ����ִµ��� ���������ϴ�.
        {
            if (CardManager.Instance.GetSameMonthCards(card).Count >= 3)
            {
                Debug.Log("����"); // ó�� ���ֱ� , UI �����
                OnShakeOrBomb();
            }
            targetGrid = GetNullGrid();
            targetGrid.Set(card);
        }
        else
        {
            int count = targetGrid.placedCards.Count;
            switch (count)
            {
                case 1: // �ϳ� ������ �� ģ������ �켱 ����. -> �� ���ɼ�
                    List<Card> cards = CardManager.Instance.GetSameMonthCards(card);
                    if (cards.Count == 3)
                    {
                        OnScored(targetGrid.placedCards[0], targetGrid); // �ٴ� �� �԰�
                        while (cards.Count > 0)
                        {
                            OnScored(cards[0]);
                            cards.RemoveAt(0);
                        }
                        OnShakeOrBomb();
                    }
                    else
                    {
                        MoveCardToGrid(card, targetGrid);
                    }
                    
                    break;
                case 2: // �ΰ� ������ �ű�� �켱 ���� -> ���� ���ɼ�
                    MoveCardToGrid(card, targetGrid);

                    choiceCallBackQueue.Enqueue(() => 
                        ChoiceCard(targetGrid.placedCards[0], targetGrid.placedCards[1], targetGrid));
                    putCardQueue.Enqueue(card);
                    break;

                case 3: // 3�� ������ �� �������� 
                    while (targetGrid.placedCards.Count > 0)
                    {
                        OnScored(targetGrid.placedCards[0], targetGrid);
                    }

                    OnScored(card);
                    OnShakeOrBomb();
                    targetGrid.Reset();
                    break;
            }
        }

        Card followCard = followUpCardQueue.Dequeue();

        followCard.gameObject.SetActive(true);

        CardGrid followCardTargetGrid = CardFinder.GetSameMonthCardsGrid(cardGrids, followCard);

        if (followCardTargetGrid == null) // ���� ���� �򸰰� ������ �׳� ����ִµ��� ���������ϴ�.
        {
            followCardTargetGrid = GetNullGrid();
            followCardTargetGrid.Set(followCard);
        }
        else
        {
            int count = followCardTargetGrid.placedCards.Count;
            switch (count)
            {
                case 1: // ���и� ���µ� �ϳ��� �ִٴ� �Ÿ�, ������ �������� ��.
                    if (card.cardData.cardMonth == followCard.cardData.cardMonth)
                    {
                        Debug.Log("��");
                        // ���� ����
                    }

                    OnScored(followCard);
                    OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                    followCardTargetGrid.Reset();
                    break;

                case 2: // �� ���� �ִٴ� �Ÿ� �� �� �ϳ��� �����ϰ� �ϸ� ��.
                    MoveCardToGrid(followCard, followCardTargetGrid);

                    if (card.cardData.cardMonth == followCard.cardData.cardMonth) //����
                    {
                        Debug.Log("��");
                    }
                    else
                    {
                        choiceCallBackQueue.Enqueue(() => 
                            ChoiceCard(followCardTargetGrid.placedCards[0], followCardTargetGrid.placedCards[1], followCardTargetGrid));
                        putCardQueue.Enqueue(followCard);
                    }
                    break;

                case 3: // 3�� ������ �� �������� 
                    if (card.cardData.cardMonth == followCard.cardData.cardMonth)
                    {
                        Debug.Log("����");

                        //���� ����
                    }


                    while (followCardTargetGrid.placedCards.Count > 0)
                    {
                        OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                    }

                    OnScored(followCard); // ���� �� �͵� ��������.
                    choiceCallBackQueue.Clear();
                    followCardTargetGrid.Reset();
                    break;
            }
        }

        if(targetGrid != null)
        {
            CardManager.Instance.TakePairCard(targetGrid);
        }
        else
        {
            Debug.Log("�ѹ�����~");
        }
        TryExecuteChoiceCallback();

        if (isUserTurn)
        {
            sc.CheckCards(myCards, user);
            Debug.Log(sc.GetScore(user));
        }
        else
        {
            sc.CheckCards(otherCards, other);
            Debug.Log(sc.GetScore(other));
        }
    }
}
