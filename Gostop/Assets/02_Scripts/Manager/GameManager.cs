using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [HideInInspector] public List<Card> useCardList = new List<Card>(); // ���� ����� ��
    [HideInInspector] public Queue<Card> followUpCardQueue = new Queue<Card>(); // ���б� �� �� �뵵

    public List<Card> ownCards = new List<Card>();            // ���� ���� �е�

    public List<CardGrid> cardGrids = new List<CardGrid>(); // ī�带 �� ��ġ, ������ �ٸ��� �׳� �� ���� ã�Ƽ� ã�ư���
    List<int> takenMonthList = new List<int>();  // ���� ��� �� ����

    public Queue<Action> choiceCallBackQueue = new Queue<Action>(); // �ΰ��� �и� ġ��, ���߿� ������
    public Queue<Card> putCardQueue = new Queue<Card>(); // ����� �� �ֵ� �Ծ������ ��

    public bool isChoicing = false;
    public bool isUserTurn = true;
    bool isFirstChecking = true;

    public ScoreCounter sc = new ScoreCounter();


    public UserData user = new UserData();
    public TestAI ai = new TestAI();

    UserData targetUserData = null;

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
        CardManager.Instance.SetPlayersUtilizeCard(user,ai.userData);
        CardManager.Instance.SetUtilizeCards(user, ai.userData);
        SetFollowCardQueue();
    }



    public void TryExecuteChoiceCallback()
    {
        if (choiceCallBackQueue.Count > 0)
        {
            choiceCallBackQueue.Dequeue()?.Invoke();
        }
        else
        {
            OnTurnFinished();
        }
    }

    public void OnTurnFinished()
    {
        CheckScore(targetUserData.ownCards, targetUserData.scoreData);

        //���߿� �̰� ������ �ð����� �ع����°ɷ� �ϰ�, �ð� �ٵǸ� ���� �˾Ƽ� �ع����°ɷ� �ٲ���
        if (isUserTurn)
        {
            isUserTurn = false;
            ai.Turn();
        }
        else
        {
            isUserTurn = true;
        }

    }

    public void CheckScore(List<Card> checkCardList, ScoreData scoreData)
    {
        sc.CheckCards(checkCardList, scoreData);
        int score = sc.GetScore(scoreData);
        if(score >= 7 )
        {
            Debug.Log("Go or Stop?");
        }
    }

    public Card GetRandomCard(List<Card> useCardList)
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

    public void OnShaked(Card chosenCard, CardGrid cardGrid)
    {
        Debug.Log("����");
        cardGrid.Set(chosenCard);
        OnShakedCallback.Invoke(); // �̰� ������ �������Ŷ� ��üũ�ϸ� �ȵɵ�..
        OnShakedCallback = null;
    }

    public void OnChooseCard(Card chosenCard, CardGrid cardGrid) // ī�尡 �ִ� ���� ����Ʈ ������� ��.
    {
        Card card = putCardQueue.Dequeue();
        OnScored(card,cardGrid);
        OnScored(chosenCard, cardGrid);

        TryExecuteChoiceCallback();
    }

    public CardGrid GetNullGrid() => cardGrids.Find((x) => x.placedCards.Count == 0);

    public void OnScored(Card card, CardGrid cardGrid = null) // ī�带 ���� ��
    {
        CardManager.Instance.TransportCard(card);
        if (cardGrid != null) cardGrid.placedCards.Remove(card);

        card.img.raycastTarget = false;
        targetUserData.ownCards.Add(card);
    }

    void SetFollowCardQueue()
    {
        foreach (var item in useCardList) followUpCardQueue.Enqueue(item);
        useCardList.Clear(); // �ִ� ���� �� �� ����.
    }

    void CheckMyCards() // ���� ���� �� ��, ���� �� ī�尡 ����� -> ��ź ���� üũ����
    {
        foreach(var card in ownCards)
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
        targetUserData.scoreData.shakedCount++;
    }    

    public void Paulk()
    {
        targetUserData.scoreData.paulkCount++;
    }

    public void TakeOtherPlayerCard(UserData myData, UserData otherData)
    {

    }

    public void Kiss()
    {
        //TakeOtherPlayerCard();
        Debug.Log("��");
    }



    public void SetTargetUserData()
    {
        if (isUserTurn)
        {
            targetUserData = user;
        }
        else
        {
            targetUserData = ai.userData;
        }
    }


    CardGrid targetGrid = null;

    public Action OnShakedCallback;

    public void PutCard(Card card) // ���� �� �׸��带 ������ ������ ������?ai.scoreData
    { 
        SetTargetUserData();

        targetGrid = CardFinder.GetSameMonthCardsGrid(cardGrids, card);

        if (targetGrid == null) // ���� ���� �򸰰� ������ �׳� ����ִµ��� ���������ϴ�.
        {
            List<Card> cardList = CardManager.Instance.GetSameMonthCards(card);
            targetGrid = GetNullGrid();

            OnShakedCallback = () => FollowCord(card);

            if (cardList.Count >= 3)
            {
                UIManager.Instance.shakeUI.SetData(cardList[0], cardList[1], cardList[2], targetGrid);
                OnShakeOrBomb();
            }
            else
            {
                targetGrid.Set(card);
                FollowCord(card);
            }
        }
        else // �� ��� �ڿ������� �� �е� ��.
        {
            int count = targetGrid.placedCards.Count;
            switch (count)
            {
                case 1: // �ϳ� ������ �� ģ������ �켱 ����. -> �� ���ɼ�
                    List<Card> cards = CardManager.Instance.GetSameMonthCards(card);
                    if (cards.Count == 3)
                    {
                        OnScored(targetGrid.placedCards[0],targetGrid); // �ٴ� �� �԰�
                        while (cards.Count > 0)
                        {
                            OnScored(cards[0]);
                            cards.RemoveAt(0);
                        }
                        OnShakeOrBomb();
                    }
                    else
                    {
                        card.MoveCardToGrid(targetGrid);
                    }
                    
                    break;
                case 2: // �ΰ� ������ �ű�� �켱 ���� -> ���� ���ɼ�
                    card.MoveCardToGrid(targetGrid);

                    choiceCallBackQueue.Enqueue(() => 
                        ChoiceCard(targetGrid.placedCards[0], targetGrid.placedCards[1], targetGrid));
                    putCardQueue.Enqueue(card);
                    break;

                case 3: // 3�� ������ �� �������� 
                    while (targetGrid.placedCards.Count > 0)
                    {
                        OnScored(targetGrid.placedCards[0],targetGrid);
                    }

                    OnScored(card);
                    OnShakeOrBomb();
                    targetGrid.Reset();
                    break;
            }

            FollowCord(card);
        }

    }//�Լ��� ��.

    public void FollowCord(Card putCard)
    {
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
                    if (putCard.cardData.cardMonth == followCard.cardData.cardMonth)
                    {
                        Kiss();
                    }

                    OnScored(followCard);
                    OnScored(followCardTargetGrid.placedCards[0], followCardTargetGrid);
                    followCardTargetGrid.Reset();
                    break;

                case 2: // �� ���� �ִٴ� �Ÿ� �� �� �ϳ��� �����ϰ� �ϸ� ��.
                    followCard.MoveCardToGrid(followCardTargetGrid);

                    if (putCard.cardData.cardMonth == followCard.cardData.cardMonth) //����
                    {
                        Paulk();
                    }
                    else
                    {
                        choiceCallBackQueue.Enqueue(() =>
                            ChoiceCard(followCardTargetGrid.placedCards[0], followCardTargetGrid.placedCards[1], followCardTargetGrid));
                        putCardQueue.Enqueue(followCard);
                    }
                    break;

                case 3: // 3�� ������ �� �������� 
                    if (putCard.cardData.cardMonth == followCard.cardData.cardMonth)
                    {
                        //TakeOtherPlayerCard();
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

        CardManager.Instance.TakePairCard(targetGrid);
        TryExecuteChoiceCallback();

        targetGrid = null;
    }
}
